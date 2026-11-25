using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MerajAndisheh.Infrastructure.Data;

namespace MerajAndisheh.Infrastructure.Identity;

public static class AppIdentityServiceCollectionExtensions
{
    /// <summary>ثبت Identity، نقش‌ها، سرویس ثبت‌نام و Seed نقش‌ها و مدیر کل. پیش‌نیاز: AddDbContext&lt;AppDbContext&gt;.</summary>
    public static IServiceCollection AddAppIdentity(this IServiceCollection services)
    {
        services.AddDataProtection();

        services
            .AddIdentity<AppUser, AppRole>(IdentityOptionsConfigurator.Configure)
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

        services.AddScoped<AccountRegistrationService>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
