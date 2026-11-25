using MerajAndisheh.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure.Data;

/// <summary>
/// اجرای Migrationها و داده‌ی اولیه هنگام شروع برنامه (فقط در حالت توسعه فراخوانی می‌شود).
/// ترتیب: Migration ← داده‌ی اولیه ← نقش‌ها ← مدیر کل اولیه (از تنظیمات مخفی).
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(db);

        var identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();
        await identitySeeder.SeedRolesAsync();
        await identitySeeder.SeedSuperAdminAsync(
            configuration["SuperAdmin:Email"],
            configuration["SuperAdmin:TemporaryPassword"]);
    }
}
