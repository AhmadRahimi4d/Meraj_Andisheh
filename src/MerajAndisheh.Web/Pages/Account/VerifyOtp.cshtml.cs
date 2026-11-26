using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Infrastructure.Otp;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Account;
using MerajAndisheh.Web.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

/// <summary>
/// وارد کردن کد پیامکی. کد درست و شماره‌ی دارای حساب ← ورود؛ کد درست و شماره‌ی بدون حساب ← صفحه‌ی تکمیل پروفایل
/// (حساب فقط بعد از تکمیل پروفایل ساخته می‌شود).
/// </summary>
[EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
public class VerifyOtpModel(
    OtpService otp,
    UserManager<AppUser> users,
    SignInManager<AppUser> signInManager,
    OtpVerificationTicket tickets,
    IClock clock,
    ILogger<VerifyOtpModel> logger,
    IStringLocalizer<SharedResource> L) : PageModel
{
    /// <summary>شماره‌ی یکسان‌سازی‌شده (09xxxxxxxxx).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Phone { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Remember { get; set; }

    [BindProperty]
    public VerifyOtpInput Input { get; set; } = new();

    /// <summary>کلید پیام اطلاع‌رسانی (مثلاً «کد تازه ارسال شد»).</summary>
    public string? NoticeKey { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true) return LocalRedirect("/");

        if (!PhoneNumberNormalizer.TryNormalize(Phone, out var phone))
            return RedirectToPage("/Account/Login");

        Phone = phone;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!PhoneNumberNormalizer.TryNormalize(Phone, out var phone))
            return RedirectToPage("/Account/Login");
        Phone = phone;

        var result = await otp.VerifyAsync(phone, Input.Code, cancellationToken);

        switch (result.Status)
        {
            case OtpVerifyStatus.Verified:
                return await ContinueAfterVerifiedAsync(phone, cancellationToken);

            case OtpVerifyStatus.Invalid:
                ModelState.AddModelError(string.Empty, L["Account.Error.OtpInvalid", result.AttemptsLeft]);
                break;

            case OtpVerifyStatus.TooManyAttempts:
                ModelState.AddModelError(string.Empty, L["Account.Error.OtpTooManyAttempts"]);
                break;

            default:
                ModelState.AddModelError(string.Empty, L["Account.Error.OtpExpired"]);
                break;
        }

        // کد اشتباه دوباره در کادر نمایش داده نشود
        ModelState.Remove("Input.Code");
        Input.Code = string.Empty;
        return Page();
    }

    /// <summary>«ارسال دوباره‌ی کد» (قواعد ۶۰ ثانیه و ۵ کد در ساعت در OtpService).</summary>
    public async Task<IActionResult> OnPostResendAsync(CancellationToken cancellationToken)
    {
        if (!PhoneNumberNormalizer.TryNormalize(Phone, out var phone))
            return RedirectToPage("/Account/Login");
        Phone = phone;

        var result = await otp.SendAsync(phone, cancellationToken);

        switch (result.Decision)
        {
            case OtpSendDecision.Allowed:
                NoticeKey = "Account.Otp.Resent";
                break;

            case OtpSendDecision.TooSoon:
                ModelState.AddModelError(string.Empty, L["Account.Error.OtpTooSoon", result.RetryAfterSeconds]);
                break;

            default:
                ModelState.AddModelError(string.Empty, L["Account.Error.OtpHourlyLimit"]);
                break;
        }

        return Page();
    }

    private async Task<IActionResult> ContinueAfterVerifiedAsync(string phone, CancellationToken cancellationToken)
    {
        var user = await users.Users.SingleOrDefaultAsync(u => u.PhoneNumber == phone, cancellationToken);

        // شماره‌ی بدون حساب: برگه‌ی تأیید شماره می‌گیرد و پروفایلش را تکمیل می‌کند
        if (user is null)
        {
            var ticket = tickets.Create(phone);
            return RedirectToPage("/Account/CompleteProfile", new { ticket, returnUrl = ReturnUrl, remember = Remember });
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.InvalidCredentials"]);
            return Page();
        }

        if (await users.IsLockedOutAsync(user))
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.LockedOut", AccountRules.LockoutMinutes]);
            return Page();
        }

        user.PhoneNumberConfirmed = true;
        user.LastLoginAt = clock.UtcNow;
        await users.UpdateAsync(user);

        await signInManager.SignInAsync(user, isPersistent: Remember);
        logger.LogInformation("User {UserId} signed in with phone code", user.Id);

        if (user.MustChangePassword) return LocalRedirect(PasswordChangeGate.ChangePasswordPath);

        var roles = await users.GetRolesAsync(user);
        var safeReturnUrl = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : null;
        return LocalRedirect(LoginRedirectRules.ResolveTarget(roles, safeReturnUrl));
    }
}
