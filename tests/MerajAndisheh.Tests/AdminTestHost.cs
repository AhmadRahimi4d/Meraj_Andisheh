using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Infrastructure.Authorization;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Infrastructure.Time;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Tests;

/// <summary>
/// میزبان تست برای پنل مدیریت (مرحله‌ی ۶): همان ثبت سرویس‌های برنامه (Identity، Policy ها و سرویس‌های پنل)
/// روی SQLite درون‌حافظه، با نقش‌های ساخته‌شده و لاگ قابل‌بررسی.
/// </summary>
public sealed class AdminTestHost : IDisposable
{
    private readonly SqliteConnection _connection;

    private AdminTestHost(SqliteConnection connection, ServiceProvider services, CapturingLoggerProvider logs)
    {
        _connection = connection;
        Services = services;
        Logs = logs;
    }

    public ServiceProvider Services { get; }
    public CapturingLoggerProvider Logs { get; }

    public IServiceScope CreateScope() => Services.CreateScope();

    public static async Task<AdminTestHost> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        connection.Open();

        var logs = new CapturingLoggerProvider();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(logs));
        services.AddSingleton<IClock, SystemClock>();
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.AddAppIdentity();
        services.AddAppAuthorization();
        services.AddAdministration();

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(db);
            await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedRolesAsync();
        }

        return new AdminTestHost(connection, provider, logs);
    }

    /// <summary>یک کاربر بی‌رمز با یک نقش (برای تست جستجو، فعال/غیرفعال‌سازی و ...).</summary>
    public async Task<int> CreateUserAsync(string userName, string role, string displayName = "کاربر", string? email = null, string? phone = null, bool isActive = true)
    {
        using var scope = CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = email is not null,
            PhoneNumber = phone,
            PhoneNumberConfirmed = phone is not null,
            DisplayName = displayName,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

        var create = await users.CreateAsync(user);
        if (!create.Succeeded) throw new InvalidOperationException(string.Join(", ", create.Errors.Select(e => e.Code)));

        var addRole = await users.AddToRoleAsync(user, role);
        if (!addRole.Succeeded) throw new InvalidOperationException(string.Join(", ", addRole.Errors.Select(e => e.Code)));

        return user.Id;
    }

    public void Dispose()
    {
        Services.Dispose();
        _connection.Dispose();
    }
}
