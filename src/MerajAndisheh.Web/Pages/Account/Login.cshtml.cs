using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

public class LoginModel(
    UserManager<AppUser> users,
    SignInManager<AppUser> signInManager,
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

    public async Task<IActionResult> OnPostAsync()
    {
        var login = Input.Login?.Trim() ?? string.Empty;
        var password = Input.Password ?? string.Empty;

        if (login.Length == 0 || password.Length == 0)
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

            return LocalRedirect(!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
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
