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

        // مرحله‌ی ۵: خاموش. با true، Identity ایمیلِ خالی را رد می‌کند و کاربرِ ثبت‌نام‌شده با موبایل ساخته نمی‌شود.
        // یکتایی و قالب ایمیل (برای ایمیل‌های پرشده) را AppUserValidator تضمین می‌کند.
        options.User.RequireUniqueEmail = false;

        // ثبت‌نام با ایمیل نیاز به تأیید ایمیل دارد (فقط مسیر ورود با رمز؛ ورود با کد پیامکی ایمیل لازم ندارد)
        options.SignIn.RequireConfirmedEmail = true;
    }
}
