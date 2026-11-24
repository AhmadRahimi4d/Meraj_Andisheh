using MerajAndisheh.Infrastructure;
using MerajAndisheh.Infrastructure.Data;
using Serilog;

// لاگر اولیه: اگر برنامه پیش از خواندن تنظیمات خراب شود، خطا دیده شود.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // لاگ اصلی: تنظیمات از بخش "Serilog" در appsettings خوانده می‌شود.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddRazorPages();

    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:Default is not set.");
    builder.Services.AddInfrastructure(connectionString);

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    // فقط در توسعه: ساخت دیتابیس / اجرای Migrationها. در تولید، به‌روزرسانی دیتابیس دستی و کنترل‌شده است.
    if (app.Environment.IsDevelopment())
    {
        await DatabaseInitializer.InitializeAsync(app.Services);
    }

    Log.Information("برنامه‌ی معراج اندیشه شروع شد (محیط: {Environment})", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "برنامه به‌دلیل خطا متوقف شد");
}
finally
{
    Log.CloseAndFlush();
}
