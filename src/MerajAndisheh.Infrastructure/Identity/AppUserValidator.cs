using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// اعتبارسنج کاربر برای ایمیل اختیاری و شماره‌ی موبایل یکتا.
///
/// چرا لازم شد: با RequireUniqueEmail = true، Identity ایمیلِ خالی را رد می‌کند، در حالی که کاربرِ ثبت‌نام‌شده
/// با موبایل ایمیل ندارد. پس آن گزینه خاموش شد و همین کلاس همان قواعد را فقط برای ایمیلِ پرشده اجرا می‌کند:
/// قالب درست و یکتا بودن (بدون حساسیت به بزرگی و کوچکی حروف). یکتا بودن شماره هم اینجا و با ایندکس یکتای دیتابیس تضمین می‌شود.
/// </summary>
public sealed class AppUserValidator(IdentityErrorDescriber describer) : IUserValidator<AppUser>
{
    public const string DuplicatePhoneNumberCode = "DuplicatePhoneNumber";

    public async Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
    {
        var errors = new List<IdentityError>();

        var email = await manager.GetEmailAsync(user);
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add(describer.InvalidEmail(email));
            }
            else
            {
                var owner = await manager.FindByEmailAsync(email);
                if (owner is not null && owner.Id != user.Id)
                    errors.Add(describer.DuplicateEmail(email));
            }
        }

        var phone = await manager.GetPhoneNumberAsync(user);
        if (!string.IsNullOrWhiteSpace(phone))
        {
            var taken = await manager.Users.AnyAsync(u => u.PhoneNumber == phone && u.Id != user.Id);
            if (taken)
            {
                errors.Add(new IdentityError
                {
                    Code = DuplicatePhoneNumberCode,
                    Description = $"Phone number '{phone}' is already taken."
                });
            }
        }

        return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
    }
}
