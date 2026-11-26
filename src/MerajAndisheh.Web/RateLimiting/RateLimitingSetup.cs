using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.RateLimiting;

/// <summary>
/// محدودیت نرخ (Rate Limiting) برای صفحه‌های ورود، تأیید کد و تکمیل پروفایل، بر اساس نشانی IP.
/// سقف‌های هر شماره (۶۰ ثانیه، ۵ کد در ساعت، ۵ تلاش) جدا در OtpService اعمال می‌شود؛
/// این لایه جلوی حجم زیاد درخواست از یک IP را می‌گیرد.
/// </summary>
public static class RateLimitingSetup
{
    public const string AuthPolicy = "auth";

    /// <summary>حداکثر درخواست از یک IP در هر پنجره (شامل بازکردن صفحه‌ها).</summary>
    public const int AuthPermitLimit = 20;

    public static readonly TimeSpan AuthWindow = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = AuthPermitLimit,
                        Window = AuthWindow,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
                context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                await context.HttpContext.Response.WriteAsync(localizer["Common.TooManyRequests"].Value, cancellationToken);
            };
        });

        return services;
    }
}
