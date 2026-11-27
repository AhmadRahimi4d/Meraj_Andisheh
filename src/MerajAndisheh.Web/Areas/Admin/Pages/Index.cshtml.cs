using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Areas.Admin.Pages;

/// <summary>
/// صفحه‌ی اصلی پنل: بخش‌هایی را که کاربر به آن‌ها دسترسی دارد نشان می‌دهد (داشبورد آمار در مرحله‌ی ۱۹ می‌آید).
/// هیچ‌جای بخش عمومی به این صفحه لینک نمی‌دهد. غیرهمکاران پیش از رسیدن به اینجا در AdminHiddenMiddleware «۴۰۴» می‌گیرند.
/// </summary>
[Authorize(Policy = PolicyNames.AnyStaff)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
