using MerajAndisheh.Core.Services;

namespace MerajAndisheh.Web.Theming;

/// <summary>
/// مرحله‌ی ۲: تم پیش‌فرض «رسمی» است و فقط در حالت توسعه می‌شود با ?theme=playful|modern|editorial عوضش کرد.
/// مرحله‌ی ۱۱ این کلاس را با تم بر پایه‌ی رده‌ی سنی کاربر جایگزین می‌کند.
/// </summary>
public sealed class ThemeResolver(IWebHostEnvironment environment) : IThemeResolver
{
    public string Resolve(HttpContext context)
    {
        if (environment.IsDevelopment() && context.Request.Query.TryGetValue("theme", out var requested))
        {
            return ThemeCatalog.Normalize(requested.ToString());
        }

        return ThemeCatalog.Default;
    }
}
