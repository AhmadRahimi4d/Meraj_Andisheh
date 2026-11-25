using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Infrastructure.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Tests;

/// <summary>
/// میزبان تست برای سرویس‌های Identity: همان ثبت سرویس‌های برنامه (AddAppIdentity)
/// روی دیتابیس SQLite درون‌حافظه، با رده‌های سنی و نقش‌های اولیه.
/// </summary>
public sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection _connection;

    private IdentityTestHost(SqliteConnection connection, ServiceProvider services)
    {
        _connection = connection;
        Services = services;
    }

    public ServiceProvider Services { get; }

    public IServiceScope CreateScope() => Services.CreateScope();

    public static async Task<IdentityTestHost> CreateAsync(bool seedRoles = true)
    {
        var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IClock, SystemClock>();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddAppIdentity();

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(db);

            if (seedRoles)
            {
                await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedRolesAsync();
            }
        }

        return new IdentityTestHost(connection, provider);
    }

    /// <summary>شناسه‌ی یک رده‌ی سنی (child, teen, youth, adult).</summary>
    public async Task<int> GetAgeGroupIdAsync(string key)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.AgeGroups.Where(a => a.Key == key).Select(a => a.Id).SingleAsync();
    }

    public void Dispose()
    {
        Services.Dispose();
        _connection.Dispose();
    }
}
