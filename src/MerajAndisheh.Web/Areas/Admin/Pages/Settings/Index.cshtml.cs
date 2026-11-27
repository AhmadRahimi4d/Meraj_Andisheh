using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Administration;
using MerajAndisheh.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Areas.Admin.Pages.Settings;

/// <summary>
/// تنظیمات و ماژول‌ها (کلید settings.manage): روشن/خاموش‌کردن ماژول‌های اختیاری و ویرایش مقادیر SiteSetting.
/// هر تغییر در لاگ ثبت می‌شود. اجبار ۴۰۴ برای ماژول خاموش در مرحله‌ی خودِ هر ماژول ساخته می‌شود.
/// </summary>
[Authorize(Policy = PermissionKeys.Settings)]
public class IndexModel(FeatureFlagService features, SiteSettingService settings) : PageModel
{
    public IReadOnlyList<FeatureFlagItem> Features { get; private set; } = [];
    public IReadOnlyList<SiteSettingItem> Settings { get; private set; } = [];

    /// <summary>مقادیر فرم تنظیمات (کلید ← مقدار). در خطا، همان‌چیزی که کاربر نوشته دوباره نشان داده می‌شود.</summary>
    [BindProperty]
    public Dictionary<string, string?> Values { get; set; } = new();

    /// <summary>خطای هر کلید پس از ذخیره‌ی ناموفق.</summary>
    public IReadOnlyDictionary<string, SiteSettingError> Errors { get; private set; } = new Dictionary<string, SiteSettingError>();

    [TempData]
    public string? NoticeKey { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostModuleAsync(string? key, bool enabled, CancellationToken cancellationToken)
    {
        var result = await features.SetAsync(key, enabled, User.GetUserId() ?? 0, cancellationToken);

        NoticeKey = result.Succeeded
            ? (enabled ? "Panel.Settings.ModuleEnabled" : "Panel.Settings.ModuleDisabled")
            : "Panel.Error.Operation";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await settings.UpdateAsync(Values, User.GetUserId() ?? 0, cancellationToken);

        if (result.Succeeded)
        {
            NoticeKey = "Panel.Settings.Saved";
            return RedirectToPage();
        }

        Errors = result.Errors;
        await LoadAsync(cancellationToken, keepSubmittedValues: true);
        return Page();
    }

    private async Task LoadAsync(CancellationToken cancellationToken, bool keepSubmittedValues = false)
    {
        Features = await features.GetAllAsync(cancellationToken);
        Settings = await settings.GetAllAsync(cancellationToken);

        if (!keepSubmittedValues)
        {
            Values = Settings.ToDictionary(item => item.Definition.Key, item => (string?)item.Value, StringComparer.Ordinal);
        }
    }
}
