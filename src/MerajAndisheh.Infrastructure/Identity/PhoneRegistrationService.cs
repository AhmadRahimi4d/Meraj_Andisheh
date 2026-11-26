using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// ساخت حساب عضو با شماره‌ی موبایل. درستیِ مالکیت شماره پیش‌تر با کد پیامکی ثابت شده (OtpService)؛
/// این سرویس فقط پروفایل (نام نمایشی و رده‌ی سنی) را می‌سنجد و حساب را می‌سازد.
/// حساب رمز ندارد و ایمیلش خالی است؛ نام کاربری همان شماره است.
/// </summary>
public sealed class PhoneRegistrationService(UserManager<AppUser> users, AppDbContext db, IClock clock)
{
    public async Task<PhoneRegistrationResult> RegisterAsync(PhoneRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<RegistrationError>();

        if (!PhoneNumberNormalizer.TryNormalize(request.PhoneNumber, out var phone))
            errors.Add(RegistrationError.PhoneInvalid);

        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length == 0 || displayName.Length > AccountRules.DisplayNameMaxLength)
            errors.Add(RegistrationError.DisplayNameInvalid);

        var ageGroupId = request.AgeGroupId;
        var ageGroupValid = ageGroupId is not null &&
                            await db.AgeGroups.AnyAsync(a => a.Id == ageGroupId && a.IsActive, cancellationToken);
        if (!ageGroupValid)
            errors.Add(RegistrationError.AgeGroupInvalid);

        if (errors.Count > 0)
            return PhoneRegistrationResult.Failure(errors);

        var user = new AppUser
        {
            UserName = phone!,
            PhoneNumber = phone!,
            PhoneNumberConfirmed = true,
            DisplayName = displayName,
            AgeGroupId = ageGroupId,
            PersonalizationEnabled = true,
            IsActive = true,
            CreatedAt = clock.UtcNow
        };

        var create = await users.CreateAsync(user);
        if (!create.Succeeded)
            return PhoneRegistrationResult.Failure(MapErrors(create.Errors));

        var addRole = await users.AddToRoleAsync(user, RoleNames.Member);
        if (!addRole.Succeeded)
        {
            await users.DeleteAsync(user); // نقش نداشتن کاربر را نیمه‌کاره نگذار
            return PhoneRegistrationResult.Failure([RegistrationError.Unknown]);
        }

        return PhoneRegistrationResult.Success(user);
    }

    private static List<RegistrationError> MapErrors(IEnumerable<IdentityError> identityErrors)
    {
        var mapped = new List<RegistrationError>();
        foreach (var error in identityErrors)
        {
            var item = error.Code switch
            {
                AppUserValidator.DuplicatePhoneNumberCode or "DuplicateUserName" => RegistrationError.PhoneTaken,
                _ => RegistrationError.Unknown
            };
            if (!mapped.Contains(item)) mapped.Add(item);
        }
        return mapped;
    }
}
