using System.Security.Claims;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Web.Identity;

/// <summary>
/// پنل مدیریت را پنهان نگه می‌دارد: هر کسی که همکار یا مدیر کل نباشد (عضو یا ورودنکرده) برای /admin
/// همان پاسخ «۴۰۴» را می‌گیرد که برای هر نشانی ناموجود، پس وجود پنل لو نمی‌رود.
/// بعد از UseAuthentication و پیش از UseMustChangePassword قرار می‌گیرد.
/// </summary>
public sealed class AdminHiddenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (AdminAccessRules.IsAdminPath(context.Request.Path.Value) && !IsStaff(context.User))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next(context);
    }

    private static bool IsStaff(ClaimsPrincipal user)
        => user.Identity?.IsAuthenticated == true
           && (user.IsInRole(RoleNames.SuperAdmin) || user.IsInRole(RoleNames.Staff));
}

public static class AdminHiddenMiddlewareExtensions
{
    public static IApplicationBuilder UseAdminHidden(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AdminHiddenMiddleware>();
    }
}
