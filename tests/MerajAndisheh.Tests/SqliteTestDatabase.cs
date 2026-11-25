using MerajAndisheh.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MerajAndisheh.Tests;

/// <summary>دیتابیس SQLite درون‌حافظه با همان مدل برنامه، برای تست‌های Seed و قیدهای دیتابیس.</summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        Options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public DbContextOptions<AppDbContext> Options { get; }

    public AppDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
