using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Infrastructure.Otp;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Tests;

/// <summary>
/// میزبان تست برای OTP: همان ثبت سرویس‌های برنامه (AddAppIdentity و AddOtp) روی SQLite درون‌حافظه،
/// با ساعت، پیامک‌فرست و تولیدکننده‌ی کدِ دست‌ساز.
/// </summary>
public sealed class OtpTestHost : IDisposable
{
    /// <summary>کلید آزمایشی هش (فقط در تست؛ کلید واقعی در User Secrets است).</summary>
    public const string TestHashKey = "unit-test-only-hash-key-0123456789-abcdef";

    private readonly SqliteConnection _connection;

    private OtpTestHost(SqliteConnection connection, ServiceProvider services, FakeClock clock, FakeSmsSender sms, FakeOtpCodeGenerator codes)
    {
        _connection = connection;
        Services = services;
        Clock = clock;
        Sms = sms;
        Codes = codes;
    }

    public ServiceProvider Services { get; }
    public FakeClock Clock { get; }
    public FakeSmsSender Sms { get; }
    public FakeOtpCodeGenerator Codes { get; }

    public IServiceScope CreateScope() => Services.CreateScope();

    public static async Task<OtpTestHost> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        connection.Open();

        var clock = new FakeClock(new DateTime(2026, 11, 26, 10, 0, 0, DateTimeKind.Utc));
        var sms = new FakeSmsSender();
        var codes = new FakeOtpCodeGenerator();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IClock>(clock);
        services.AddSingleton<ISmsSender>(sms);
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        services.Configure<OtpOptions>(options => options.HashKey = TestHashKey);
        services.AddAppIdentity();
        services.AddOtp();
        services.AddSingleton<IOtpCodeGenerator>(codes); // ثبت آخر برنده است: جایگزین تولیدکننده‌ی تصادفی

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(db);
            await scope.ServiceProvider.GetRequiredService<IdentitySeeder>().SeedRolesAsync();
        }

        return new OtpTestHost(connection, provider, clock, sms, codes);
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
