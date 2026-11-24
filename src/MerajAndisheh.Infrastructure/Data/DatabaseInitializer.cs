using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure.Data;

/// <summary>
/// اجرای Migrationها هنگام شروع برنامه (فقط در حالت توسعه فراخوانی می‌شود).
/// اگر دیتابیس وجود نداشته باشد ساخته می‌شود.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
