using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace MerajAndisheh.Web.Identity;

/// <summary>
/// تنظیمات کوکی ورود: نشست ۳۰ روزه‌ی عضو و ۸ ساعته‌ی همکار و مدیر کل (بخش ۴)،
/// بررسی دقیقه‌ای Security Stamp (غیرفعال‌شدن حساب و تغییر دسترسی زود اعمال شود) و ۴۰۳ برای پنل.
/// </summary>
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

            options.ExpireTimeSpan = TimeSpan.FromDays(AccountRules.MemberSessionDays);
            options.SlidingExpiration = true;

            // همکار و مدیر کل: دقیقاً ۸ ساعت از لحظه‌ی ورود، بدون تمدید خودکار
            options.Events.OnSigningIn = context =>
            {
                var principal = context.Principal;
                if (principal is not null &&
                    (principal.IsInRole(RoleNames.SuperAdmin) || principal.IsInRole(RoleNames.Staff)))
                {
                    var issued = context.Properties.IssuedUtc ?? DateTimeOffset.UtcNow;
                    context.Properties.ExpiresUtc = issued.AddHours(AccountRules.StaffSessionHours);
                    context.Properties.AllowRefresh = false;
                }

                return Task.CompletedTask;
            };

            // دسترسی ردشده: برای پنل «۴۰۳» (همکاری که کلید آن بخش را ندارد)؛ برای جاهای دیگر همان رفتار قبلی
            options.Events.OnRedirectToAccessDenied = async context =>
            {
                if (AdminAccessRules.IsAdminPath(context.Request.Path.Value))
                {
                    await AdminForbiddenResponse.WriteAsync(context.HttpContext);
                    return;
                }

                context.Response.Redirect(context.RedirectUri);
            };
        });

        // غیرفعال‌شدن حساب یا تغییر دسترسی (Security Stamp) حداکثر ظرف همین مدت نشست‌های باز را می‌بندد
        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(AccountRules.SessionValidationMinutes));

        return services;
    }
}
