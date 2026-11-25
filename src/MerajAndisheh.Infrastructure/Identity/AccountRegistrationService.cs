using System.ComponentModel.DataAnnotations;
using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// ثبت‌نام با ایمیل: اعتبارسنجی، ساخت کاربر با نقش Member و تولید توکن تأیید ایمیل.
/// ارسال ایمیل با لایه‌ی Web است (متن ایمیل از resx می‌آید).
/// </summary>
public sealed class AccountRegistrationService(UserManager<AppUser> users, AppDbContext db, IClock clock)
{
    public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new List<RegistrationError>();

        var email = request.Email?.Trim() ?? string.Empty;
        if (email.Length == 0 || !new EmailAddressAttribute().IsValid(email))
            errors.Add(RegistrationError.EmailInvalid);

        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length == 0 || displayName.Length > AccountRules.DisplayNameMaxLength)
            errors.Add(RegistrationError.DisplayNameInvalid);

        var ageGroupId = request.AgeGroupId;
        var ageGroupValid = ageGroupId is not null &&
                            await db.AgeGroups.AnyAsync(a => a.Id == ageGroupId && a.IsActive, cancellationToken);
        if (!ageGroupValid)
            errors.Add(RegistrationError.AgeGroupInvalid);

        if (errors.Count > 0)
            return RegistrationResult.Failure(errors);

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            AgeGroupId = ageGroupId,
            PersonalizationEnabled = true,
            IsActive = true,
            CreatedAt = clock.UtcNow
        };

        var create = await users.CreateAsync(user, request.Password ?? string.Empty);
        if (!create.Succeeded)
            return RegistrationResult.Failure(MapErrors(create.Errors));

        var addRole = await users.AddToRoleAsync(user, RoleNames.Member);
        if (!addRole.Succeeded)
        {
            await users.DeleteAsync(user); // نقش نداشتن کاربر را نیمه‌کاره نگذار
            return RegistrationResult.Failure([RegistrationError.Unknown]);
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        return RegistrationResult.Success(user, token);
    }

    private static List<RegistrationError> MapErrors(IEnumerable<IdentityError> identityErrors)
    {
        var mapped = new List<RegistrationError>();
        foreach (var error in identityErrors)
        {
            var item = error.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => RegistrationError.EmailTaken,
                "PasswordTooShort" => RegistrationError.PasswordTooShort,
                "InvalidEmail" or "InvalidUserName" => RegistrationError.EmailInvalid,
                _ => RegistrationError.Unknown
            };
            if (!mapped.Contains(item)) mapped.Add(item);
        }
        return mapped;
    }
}
