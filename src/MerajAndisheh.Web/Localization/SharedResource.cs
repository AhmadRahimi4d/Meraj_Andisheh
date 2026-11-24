using System.Resources;

// زبان پایه‌ی فایل منابع فارسی است (فایل Resources/SharedResource.resx).
[assembly: NeutralResourcesLanguage("fa")]

namespace MerajAndisheh.Web;

/// <summary>
/// کلاس نشانه (marker) برای IStringLocalizer. خود کلاس خالی است؛
/// متن‌ها در Resources/SharedResource.resx هستند. زبان دوم بعداً با SharedResource.en.resx اضافه می‌شود.
/// </summary>
public class SharedResource
{
}
