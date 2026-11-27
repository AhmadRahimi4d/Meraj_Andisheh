using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Areas.Admin.Pages.Staff;

/// <summary>
/// ساخت حساب همکار (فقط مدیر کل). رمز موقت را سیستم می‌سازد و فقط همین یک‌بار، در پاسخ همین ساخت، نشان داده می‌شود
/// (به‌جای هدایت، چون هدایت آن را از دست می‌داد). همکار در اولین ورود مجبور به عوض‌کردن آن است.
/// </summary>
[Authorize(Policy = PolicyNames.SuperAdminOnly)]
public class CreateModel(StaffService staff, IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty]
    public CreateStaffInput Input { get; set; } = new();

    /// <summary>پس از ساخت موفق پر می‌شود.</summary>
    public string? CreatedEmail { get; private set; }
    public string? TemporaryPassword { get; private set; }
    public int? CreatedStaffId { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        // رمز موقت در پاسخ است؛ نباید در حافظه‌ی نهان مرورگر یا پروکسی بماند
        Response.Headers.CacheControl = "no-store";

        var request = new CreateStaffRequest(Input.Email, Input.DisplayName, Input.Permissions);
        var result = await staff.CreateAsync(request, User.GetUserId() ?? 0, cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, L[$"Panel.Staff.Error.{error}", AccountRules.DisplayNameMaxLength]);
            }
            return Page();
        }

        CreatedEmail = Input.Email?.Trim();
        TemporaryPassword = result.TemporaryPassword;
        CreatedStaffId = result.UserId;
        Input = new CreateStaffInput(); // فرم خالی برای ساخت بعدی
        ModelState.Clear();
        return Page();
    }
}
