using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Areas.Admin.Pages;

/// <summary>
/// صفحه‌ی خالیِ موقت پنل مدیریت (پوسته‌ی کامل در مرحله‌ی ۶). هیچ‌جای بخش عمومی به این صفحه لینک نمی‌دهد.
/// غیرهمکاران پیش از رسیدن به اینجا در AdminHiddenMiddleware پاسخ ۴۰۴ می‌گیرند؛ این مجوز لایه‌ی دوم دفاع است.
/// </summary>
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.Staff)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
