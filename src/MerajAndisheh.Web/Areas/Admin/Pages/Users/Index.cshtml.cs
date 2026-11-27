using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Areas.Admin.Pages.Users;

/// <summary>فهرست و جستجوی اعضا و فعال/غیرفعال‌سازی (کلید users.manage). فقط اعضای عادی دیده می‌شوند.</summary>
[Authorize(Policy = PermissionKeys.Users)]
public class IndexModel(MemberAdminService members) : PageModel
{
    private const int PageSize = 20;

    /// <summary>عبارت جستجو (نام نمایشی، ایمیل یا شماره‌ی موبایل).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    /// <summary>شماره‌ی صفحه. (نام «page» برای مسیر Razor Pages رزرو است، پس P گذاشته شد.)</summary>
    [BindProperty(SupportsGet = true)]
    public int P { get; set; } = 1;

    public PagedResult<MemberRow> Result { get; private set; } = new([], 1, PageSize, 0);

    [TempData]
    public string? NoticeKey { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Result = await members.SearchAsync(Q, P, PageSize, cancellationToken);
    }

    public async Task<IActionResult> OnPostSetActiveAsync(int id, bool active, CancellationToken cancellationToken)
    {
        var result = await members.SetActiveAsync(id, active, User.GetUserId() ?? 0, cancellationToken);

        NoticeKey = result.Succeeded
            ? (active ? "Panel.Users.Activated" : "Panel.Users.Deactivated")
            : "Panel.Error.Operation";

        return RedirectToPage(new { q = Q, p = P });
    }
}
