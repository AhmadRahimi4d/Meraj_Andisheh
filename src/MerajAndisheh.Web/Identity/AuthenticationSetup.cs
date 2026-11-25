using Microsoft.AspNetCore.Http;

namespace MerajAndisheh.Web.Identity;

/// <summary>تنظیمات کوکی ورود. نشست ۸ ساعته‌ی همکار و مدیر کل در مرحله‌ی ۶ می‌آید.</summary>
public static class AuthenticationSetup
{
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IHostEnvironment environment)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "ma_auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;

            // در توسعه ممکن است با http اجرا شود؛ در تولید فقط HTTPS
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/account/login";

            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

        return services;
    }
}
