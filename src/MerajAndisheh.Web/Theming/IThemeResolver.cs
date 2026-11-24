namespace MerajAndisheh.Web.Theming;

/// <summary>تعیین تم فعلی صفحه (مقدار data-theme روی تگ html).</summary>
public interface IThemeResolver
{
    string Resolve(HttpContext context);
}
