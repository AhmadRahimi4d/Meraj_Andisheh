using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure.Administration;

public static class AdministrationServiceCollectionExtensions
{
    /// <summary>سرویس‌های پنل مدیریت (مرحله‌ی ۶). پیش‌نیاز: AddAppIdentity و AddDbContext و IClock.</summary>
    public static IServiceCollection AddAdministration(this IServiceCollection services)
    {
        services.AddSingleton<ITemporaryPasswordGenerator, TemporaryPasswordGenerator>();
        services.AddScoped<StaffService>();
        services.AddScoped<MemberAdminService>();
        services.AddScoped<SiteSettingService>();
        services.AddScoped<FeatureFlagService>();
        return services;
    }
}
