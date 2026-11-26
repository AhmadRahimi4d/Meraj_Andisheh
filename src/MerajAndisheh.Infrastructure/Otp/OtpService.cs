using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>
/// ساخت، ارسال و بررسی کد پیامکی (قواعد در OtpRules و OtpSendGate).
/// این سرویس وجود یا نبود حساب را نمی‌داند و نمی‌گوید؛ پس پاسخ ارسال کد برای شماره‌ی ثبت‌شده و ثبت‌نشده یکسان است.
/// </summary>
public sealed class OtpService(
    AppDbContext db,
    IClock clock,
    ISmsSender sms,
    IOtpCodeGenerator generator,
    OtpCodeHasher hasher)
{
    /// <param name="phoneNumber">شماره‌ی یکسان‌سازی‌شده (09xxxxxxxxx).</param>
    public async Task<OtpSendResult> SendAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var windowStart = now.AddHours(-1);
        var purgeBefore = now.AddDays(-1);

        // کدهای یک ساعت اخیر (تعداد کم است؛ مقایسه‌ی زمان در حافظه انجام می‌شود)
        var recent = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.CreatedAt > windowStart)
            .Select(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        DateTime? lastSentAt = recent.Count == 0 ? null : recent.Max();
        var evaluation = OtpSendGate.Evaluate(now, lastSentAt, recent.Count);
        if (evaluation.Decision != OtpSendDecision.Allowed)
            return new OtpSendResult(evaluation.Decision, evaluation.RetryAfterSeconds);

        // فقط جدیدترین کد معتبر است: کدهای قبلی استفاده‌نشده باطل می‌شوند
        var active = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.UsedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var old in active) old.UsedAt = now;

        // پاک‌سازی کدهای بیش از یک شبانه‌روز قدیمی همین شماره
        var stale = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.CreatedAt < purgeBefore)
            .ToListAsync(cancellationToken);
        db.OtpCodes.RemoveRange(stale);

        var code = generator.Generate();
        db.OtpCodes.Add(new OtpCode
        {
            PhoneNumber = phoneNumber,
            CodeHash = hasher.Hash(phoneNumber, code),
            ExpiresAt = now.AddMinutes(OtpRules.ValidityMinutes),
            Attempts = 0,
            CreatedAt = now
        });
        await db.SaveChangesAsync(cancellationToken);

        await sms.SendOtpCodeAsync(phoneNumber, code, cancellationToken);
        return new OtpSendResult(OtpSendDecision.Allowed, 0);
    }

    /// <param name="phoneNumber">شماره‌ی یکسان‌سازی‌شده (09xxxxxxxxx).</param>
    /// <param name="code">کد واردشده؛ ارقام فارسی و عربی هم پذیرفته می‌شود.</param>
    public async Task<OtpVerifyResult> VerifyAsync(string phoneNumber, string? code, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;

        var record = await db.OtpCodes
            .Where(o => o.PhoneNumber == phoneNumber && o.UsedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (record is null || now > record.ExpiresAt)
            return new OtpVerifyResult(OtpVerifyStatus.Expired);

        if (record.Attempts >= OtpRules.MaxAttempts)
            return new OtpVerifyResult(OtpVerifyStatus.TooManyAttempts);

        var normalized = DigitNormalizer.ToLatin(code).Trim();
        if (!HasValidFormat(normalized))
        {
            // قالب نادرست حدس به حساب نمی‌آید؛ از تلاش‌ها کم نمی‌شود
            return new OtpVerifyResult(OtpVerifyStatus.Invalid, OtpRules.MaxAttempts - record.Attempts);
        }

        if (hasher.Verify(phoneNumber, normalized, record.CodeHash))
        {
            record.UsedAt = now;
            await db.SaveChangesAsync(cancellationToken);
            return new OtpVerifyResult(OtpVerifyStatus.Verified);
        }

        record.Attempts++;
        await db.SaveChangesAsync(cancellationToken);

        return record.Attempts >= OtpRules.MaxAttempts
            ? new OtpVerifyResult(OtpVerifyStatus.TooManyAttempts)
            : new OtpVerifyResult(OtpVerifyStatus.Invalid, OtpRules.MaxAttempts - record.Attempts);
    }

    private static bool HasValidFormat(string code)
        => code.Length == OtpRules.CodeLength && code.All(char.IsAsciiDigit);
}
