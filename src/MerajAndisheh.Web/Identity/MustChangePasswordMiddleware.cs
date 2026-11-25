using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Web.Identity;

/// <summary>کاربری که باید رمز موقتش را عوض کند را به صفحه‌ی تغییر رمز می‌فرستد (قاعده در PasswordChangeGate).</summary>
public sealed class MustChangePasswordMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;
        var mustChange = user.FindFirst(AppClaimTypes.MustChangePassword)?.Value == "true";

        if (PasswordChangeGate.ShouldRedirect(user.Identity?.IsAuthenticated == true, mustChange, context.Request.Path.Value))
        {
            context.Response.Redirect(PasswordChangeGate.ChangePasswordPath);
            return;
        }

        await next(context);
    }
}

public static class MustChangePasswordMiddlewareExtensions
{
    /// <summary>بعد از UseAuthentication و پیش از UseAuthorization قرار بگیرد.</summary>
    public static IApplicationBuilder UseMustChangePassword(this IApplicationBuilder app)
    {
        return app.UseMiddleware<MustChangePasswordMiddleware>();
    }
}
