using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Infrastructure.Otp;
using MerajAndisheh.Web.Models.Account;
using MerajAndisheh.Web.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

/// <summary>
/// ورود یکپارچه (مرحله‌ی ۵): اگر ورودی شبیه شماره‌ی موبایل بود ← کد پیامکی (OTP)؛
/// اگر ایمیل یا نام کاربری بود ← رمز عبور (مسیر مرحله‌ی ۴).
/// بعد از ورود: عضو ← صفحه‌ی قبلی یا اصلی؛ همکار و مدیر کل ← پنل مدیریت (LoginRedirectRules).
/// </summary>
[EnableRateLimiting(RateLimitingSetup.AuthPolicy)]
public class LoginModel(
    UserManager<AppUser> users,
    SignInManager<AppUser> signInManager,
    OtpService otp,
    IClock clock,
    ILogger<LoginModel> logger,
    IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>وقتی ایمیل تأیید نشده، لینک «ارسال دوباره» نشان داده می‌شود.</summary>
    public bool ShowResendLink { get; private set; }

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true ? LocalRedirect("/") : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var login = Input.Login?.Trim() ?? string.Empty;
        if (login.Length == 0)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.InvalidCredentials"]);
            return Page();
        }

        // شبیه شماره‌ی موبایل ← مسیر کد پیامکی؛ در غیر این صورت ایمیل/نام کاربری و رمز
        if (PhoneNumberNormalizer.LooksLikePhone(login))
            return await StartPhoneLoginAsync(login, cancellationToken);

        return await PasswordLoginAsync(login);
    }

    /// <summary>مسیر موبایل: کد پیامک می‌شود و کاربر به صفحه‌ی وارد کردن کد می‌رود.</summary>
    private async Task<IActionResult> StartPhoneLoginAsync(string login, CancellationToken cancellationToken)
    {
        if (!PhoneNumberNormalizer.TryNormalize(login, out var phone))
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.PhoneInvalid"]);
            return Page();
        }

        var result = await otp.SendAsync(phone, cancellationToken);
        if (result.Decision == OtpSendDecision.HourlyLimit)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.OtpHourlyLimit"]);
            return Page();
        }

        // Allowed: کد تازه ارسال شد. TooSoon: کدی همین الان فرستاده شده و هنوز معتبر است.
        // در هر دو حالت کاربر به صفحه‌ی کد می‌رود. پاسخ برای شماره‌ی ثبت‌شده و ثبت‌نشده یکسان است.
        return RedirectToPage("/Account/VerifyOtp", new { phone, returnUrl = ReturnUrl, remember = Input.RememberMe });
    }

    /// <summary>مسیر ایمیل/نام کاربری و رمز (مرحله‌ی ۴؛ فقط مقصد بعد از ورود با نقش تعیین می‌شود).</summary>
    private async Task<IActionResult> PasswordLoginAsync(string login)
    {
        var password = Input.Password ?? string.Empty;
        if (password.Length == 0)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.InvalidCredentials"]);
            return Page();
        }

        // ایمیل یا نام کاربری
        var user = login.Contains('@') ? await users.FindByEmailAsync(login) : await users.FindByNameAsync(login);
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.InvalidCredentials"]);
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(user, password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = clock.UtcNow;
            await users.UpdateAsync(user);
            logger.LogInformation("User {UserId} signed in", user.Id);

            // مدیر کل اولیه با رمز موقت: اول تغییر رمز
            if (user.MustChangePassword) return LocalRedirect(PasswordChangeGate.ChangePasswordPath);

            var roles = await users.GetRolesAsync(user);
            var safeReturnUrl = Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : null;
            return LocalRedirect(LoginRedirectRules.ResolveTarget(roles, safeReturnUrl));
        }

        if (result.IsLockedOut)
            ModelState.AddModelError(string.Empty, L["Account.Error.LockedOut", AccountRules.LockoutMinutes]);
        else if (result.IsNotAllowed)
        {
            ShowResendLink = true;
            ModelState.AddModelError(string.Empty, L["Account.Error.EmailNotConfirmed"]);
        }
        else
            ModelState.AddModelError(string.Empty, L["Account.Error.InvalidCredentials"]);

        return Page();
    }
}
