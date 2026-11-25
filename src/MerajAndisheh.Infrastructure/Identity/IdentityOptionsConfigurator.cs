using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>قواعد رمز، قفل حساب و ورود (بخش ۳.۱۴). یک جا تعریف شده تا برنامه و تست‌ها یکی باشند.</summary>
public static class IdentityOptionsConfigurator
{
    public static void Configure(IdentityOptions options)
    {
        // رمز: حداقل ۸ نویسه؛ سند قاعده‌ی دیگری نگذاشته
        options.Password.RequiredLength = AccountRules.MinPasswordLength;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 1;

        // ۵ ورود ناموفق ← قفل ۱۵ دقیقه‌ای
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = AccountRules.MaxFailedAttempts;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(AccountRules.LockoutMinutes);

        options.User.RequireUniqueEmail = true;

        // ثبت‌نام با ایمیل نیاز به تأیید ایمیل دارد
        options.SignIn.RequireConfirmedEmail = true;
    }
}
