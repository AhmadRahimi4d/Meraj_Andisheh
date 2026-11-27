using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Areas.Admin.Pages.Staff;

/// <summary>ویرایش دسترسی‌ها و فعال/غیرفعال‌سازی یک همکار (فقط مدیر کل).</summary>
[Authorize(Policy = PolicyNames.SuperAdminOnly)]
public class EditModel(StaffService staff, IStringLocalizer<SharedResource> L) : PageModel
{
    public StaffRow? Staff { get; private set; }

    [BindProperty]
    public StaffPermissionsInput Input { get; set; } = new();

    /// <summary>کلید پیام موفقیت (در حالت بازگشت پس از ذخیره).</summary>
    [TempData]
    public string? NoticeKey { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        Staff = await staff.GetAsync(id, cancellationToken);
        if (Staff is null) return NotFound();

        Input.Permissions = Staff.Permissions.ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id, CancellationToken cancellationToken)
    {
        var result = await staff.UpdatePermissionsAsync(id, Input.Permissions, User.GetUserId() ?? 0, cancellationToken);
        if (result.Error == AdminOperationError.NotFound) return NotFound();

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, L["Panel.Error.Operation"]);
            Staff = await staff.GetAsync(id, cancellationToken);
            return Staff is null ? NotFound() : Page();
        }

        NoticeKey = "Panel.Staff.PermissionsSaved";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostActivateAsync(int id, CancellationToken cancellationToken)
        => await ChangeActiveAsync(id, true, "Panel.Staff.Activated", cancellationToken);

    public async Task<IActionResult> OnPostDeactivateAsync(int id, CancellationToken cancellationToken)
        => await ChangeActiveAsync(id, false, "Panel.Staff.Deactivated", cancellationToken);

    private async Task<IActionResult> ChangeActiveAsync(int id, bool active, string noticeKey, CancellationToken cancellationToken)
    {
        var result = await staff.SetActiveAsync(id, active, User.GetUserId() ?? 0, cancellationToken);
        if (result.Error == AdminOperationError.NotFound) return NotFound();

        NoticeKey = result.Succeeded ? noticeKey : "Panel.Error.Operation";
        return RedirectToPage(new { id });
    }
}
