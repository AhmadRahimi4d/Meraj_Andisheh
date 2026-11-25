using MerajAndisheh.Infrastructure;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Theming;
using Microsoft.AspNetCore.Builder;
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

    // مرحله‌ی ۲: متن‌های رابط از فایل منابع (Resources/SharedResource.resx)
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // مرحله‌ی ۲: تعیین تم صفحه (مرحله‌ی ۱۱ بر پایه‌ی رده‌ی سنی می‌شود)
    builder.Services.AddSingleton<IThemeResolver, ThemeResolver>();

    var connectionString = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:Default is not set.");
    builder.Services.AddInfrastructure(connectionString);

    // مرحله‌ی ۴: حساب کاربری (Identity)، کوکی ورود و ایمیل‌های حساب
    builder.Services.AddAppIdentity();
    builder.Services.AddAppAuthentication(builder.Environment);
    builder.Services.AddScoped<AccountEmailComposer>();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    // مرحله‌ی ۲: فرهنگ پیش‌فرض فارسی. زبان دوم در مرحله‌ی ۳۰ به این فهرست اضافه می‌شود.
    var supportedCultures = new[] { "fa" };
    app.UseRequestLocalization(new RequestLocalizationOptions()
        .SetDefaultCulture("fa")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures));

    app.UseRouting();

    // مرحله‌ی ۴: شناسایی کاربر؛ سپس اجبار تغییر رمز موقت؛ سپس مجوزها
    app.UseAuthentication();
    app.UseMustChangePassword();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    // فقط در توسعه: ساخت دیتابیس / اجرای Migrationها / داده‌ی اولیه / نقش‌ها و مدیر کل اولیه.
    // (در تولید، به‌روزرسانی دیتابیس دستی و کنترل‌شده است؛ مرحله‌ی ۲۲)
    if (app.Environment.IsDevelopment())
    {
        await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);
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
