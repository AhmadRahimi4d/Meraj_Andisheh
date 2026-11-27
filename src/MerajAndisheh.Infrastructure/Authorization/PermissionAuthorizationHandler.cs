using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace MerajAndisheh.Infrastructure.Authorization;

/// <summary>
/// مدیر کل: همیشه مجاز. همکار: فقط اگر کلید در Claim های کوکی‌اش باشد (هنگام ورود از StaffPermission آمده).
/// Claim دسترسی بدون نقش همکار هیچ ارزشی ندارد.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return Task.CompletedTask;

        var granted = user.FindAll(AppClaimTypes.Permission).Select(c => c.Value);
        var allowed = PermissionRules.HasPermission(
            user.IsInRole(RoleNames.SuperAdmin),
            user.IsInRole(RoleNames.Staff),
            granted,
            requirement.Permission);

        if (allowed) context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
