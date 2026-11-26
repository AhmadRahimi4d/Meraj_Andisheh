using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Account;
using MerajAndisheh.Web.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

/// <summary>
/// آخرین گام ثبت‌نام با موبایل: نام نمایشی و رده‌ی سنی. شماره از «برگه‌ی تأیید شماره» می‌آید
/// (رمزنگاری‌شده و ۱۰ دقیقه‌ای)، نه از ورودی کاربر؛ پس کسی نمی‌تواند برای شماره‌ی تأییدنشده حساب بسازد.
/// </summary>
[EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
public class CompleteProfileModel(
    PhoneRegistrationService registration,
    OtpVerificationTicket tickets,
    SignInManager<AppUser> signInManager,
    AppDbContext db,
    IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Ticket { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Remember { get; set; }

    [BindProperty]
    public CompleteProfileInput Input { get; set; } = new();

    public List<SelectListItem> AgeGroups { get; private set; } = [];

    /// <summary>شماره‌ی تأییدشده برای نمایش.</summary>
    public string? Phone { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true) return LocalRedirect("/");

        Phone = tickets.Validate(Ticket);
        if (Phone is null) return RedirectToPage("/Account/Login");

        await LoadAgeGroupsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var phone = tickets.Validate(Ticket);
        if (phone is null) return RedirectToPage("/Account/Login"); // برگه منقضی یا نامعتبر
        Phone = phone;

        await LoadAgeGroupsAsync();

        var displayName = Input.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length == 0 || displayName.Length > AccountRules.DisplayNameMaxLength)
            ModelState.AddModelError("Input.DisplayName", L["Account.Error.DisplayNameInvalid", AccountRules.DisplayNameMaxLength]);

        if (Input.AgeGroupId is null)
            ModelState.AddModelError("Input.AgeGroupId", L["Account.Error.AgeGroupInvalid"]);

        if (!ModelState.IsValid) return Page();

        var result = await registration.RegisterAsync(
            new PhoneRegistrationRequest(phone, displayName, Input.AgeGroupId), cancellationToken);

        if (!result.Succeeded)
        {
            // شماره‌ی تأییدشده از قبل حساب دارد (مثلاً برگه دوباره استفاده شد): به ورود برگرد
            if (result.Errors.Contains(RegistrationError.PhoneTaken))
                return RedirectToPage("/Account/Login");

            foreach (var error in result.Errors)
            {
                // آرگومان {0} پیام: برای نام نمایشی سقف طول (در بقیه‌ی پیام‌ها استفاده نمی‌شود)
                ModelState.AddModelError(string.Empty, L[$"Account.Error.{error}", AccountRules.DisplayNameMaxLength]);
            }
            return Page();
        }

        await signInManager.SignInAsync(result.User!, isPersistent: Remember);

        var safeReturnUrl = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : null;
        return LocalRedirect(LoginRedirectRules.ResolveTarget([RoleNames.Member], safeReturnUrl));
    }

    private async Task LoadAgeGroupsAsync()
    {
        AgeGroups = await db.AgeGroups
            .Where(a => a.IsActive)
            .OrderBy(a => a.SortOrder)
            .Select(a => new SelectListItem(a.Name, a.Id.ToString()))
            .ToListAsync();
    }
}
