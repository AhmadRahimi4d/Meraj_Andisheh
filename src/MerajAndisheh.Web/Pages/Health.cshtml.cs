using MerajAndisheh.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Pages;

/// <summary>صفحه‌ی /health: وضعیت برنامه و اتصال دیتابیس.</summary>
public class HealthModel(AppDbContext db, ILogger<HealthModel> logger) : PageModel
{
    public bool DatabaseConnected { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            DatabaseConnected = await db.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "بررسی اتصال دیتابیس ناموفق بود");
            DatabaseConnected = false;
        }

        Response.Headers.CacheControl = "no-store";
        if (!DatabaseConnected)
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

        return Page();
    }
}
