using MerajAndisheh.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure.Otp;

public static class OtpServiceCollectionExtensions
{
    /// <summary>
    /// ثبت سرویس‌های OTP. پیش‌نیاز: AddDbContext&lt;AppDbContext&gt;، IClock و ISmsSender.
    /// تنظیمات از بخش «Otp» با services.Configure&lt;OtpOptions&gt; بسته می‌شود (Program.cs).
    /// </summary>
    public static IServiceCollection AddOtp(this IServiceCollection services)
    {
        services.AddOptions<OtpOptions>();
        services.AddSingleton<IOtpCodeGenerator, RandomOtpCodeGenerator>();
        services.AddSingleton<OtpCodeHasher>();
        services.AddScoped<OtpService>();
        return services;
    }
}
