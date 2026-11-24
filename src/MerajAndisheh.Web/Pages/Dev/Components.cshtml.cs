using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Pages.Dev;

/// <summary>صفحه‌ی آزمایشی کامپوننت‌ها — فقط در حالت توسعه؛ در تولید ۴۰۴ می‌دهد.</summary>
public class ComponentsModel(IWebHostEnvironment environment) : PageModel
{
    public IActionResult OnGet()
    {
        return environment.IsDevelopment() ? Page() : NotFound();
    }
}
