using System.Net;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Identity;

/// <summary>
/// پاسخ «۴۰۳» برای همکاری که وارد شده ولی کلید دسترسی آن بخش را ندارد (بخش ۱۰، مرحله‌ی ۶).
/// ورودنکرده و عضو پیش‌تر در AdminHiddenMiddleware «۴۰۴» گرفته‌اند. هر ۴۰۳ در لاگ ثبت می‌شود.
/// </summary>
public static class AdminForbiddenResponse
{
    public static async Task WriteAsync(HttpContext context)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("AdminAccess");
        var userId = context.User.GetUserId();
        logger.LogWarning("Panel access denied: user {UserId} requested {Method} {Path}", userId, context.Request.Method, context.Request.Path);

        var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        var title = WebUtility.HtmlEncode(localizer["Panel.Forbidden.Title"].Value);
        var text = WebUtility.HtmlEncode(localizer["Panel.Forbidden.Text"].Value);
        var back = WebUtility.HtmlEncode(localizer["Panel.Forbidden.Back"].Value);
        var siteName = WebUtility.HtmlEncode(localizer["App.Name"].Value);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store";

        await context.Response.WriteAsync(
            "<!DOCTYPE html><html lang=\"fa\" dir=\"rtl\" data-theme=\"editorial\"><head><meta charset=\"utf-8\" />" +
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" /><meta name=\"robots\" content=\"noindex, nofollow\" />" +
            $"<title>{title} - {siteName}</title>" +
            "<link rel=\"stylesheet\" href=\"/css/tokens.css\" /><link rel=\"stylesheet\" href=\"/css/base.css\" />" +
            "<link rel=\"stylesheet\" href=\"/css/layout.css\" /><link rel=\"stylesheet\" href=\"/css/components.css\" />" +
            "</head><body><main class=\"container site-main\"><section class=\"page-intro auth-card\">" +
            $"<h1>{title}</h1><p>{text}</p><p><a class=\"btn btn--secondary\" href=\"/admin\">{back}</a></p>" +
            "</section></main></body></html>");
    }
}
