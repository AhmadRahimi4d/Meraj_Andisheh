using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Areas.Admin.Pages.Staff;

/// <summary>فهرست همکاران. فقط مدیر کل؛ به همکار (حتی با همه‌ی کلیدها) واگذار نمی‌شود.</summary>
[Authorize(Policy = PolicyNames.SuperAdminOnly)]
public class IndexModel(StaffService staff) : PageModel
{
    public IReadOnlyList<StaffRow> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Rows = await staff.ListAsync(cancellationToken);
    }
}
