using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// ثبت Policy ها (بخش ۷.۷): یکی برای هر کلید دسترسی (هم‌نام کلید)، SuperAdminOnly و AnyStaff.
    /// استفاده: [Authorize(Policy = PermissionKeys.Users)]
    /// </summary>
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var key in PermissionKeys.All)
            {
                options.AddPolicy(key, policy => policy.AddRequirements(new PermissionRequirement(key)));
            }

            options.AddPolicy(PolicyNames.SuperAdminOnly, policy => policy.RequireRole(RoleNames.SuperAdmin));
            options.AddPolicy(PolicyNames.AnyStaff, policy => policy.RequireRole(RoleNames.SuperAdmin, RoleNames.Staff));
        });

        return services;
    }
}
