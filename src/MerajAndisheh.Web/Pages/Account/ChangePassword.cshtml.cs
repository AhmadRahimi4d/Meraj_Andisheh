using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Services;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

[Authorize]
public class ChangePasswordModel(
    UserManager<AppUser> users,
    SignInManager<AppUser> signInManager,
    IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    /// <summary>برای مدیر کل اولیه: اعلام اینکه رمز موقت باید عوض شود.</summary>
    public bool MustChange => User.FindFirst(AppClaimTypes.MustChangePassword)?.Value == "true";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Input.CurrentPassword))
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.CurrentPasswordWrong"]);
            return Page();
        }
        if (string.IsNullOrEmpty(Input.NewPassword) || Input.NewPassword.Length < AccountRules.MinPasswordLength)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.PasswordTooShort", AccountRules.MinPasswordLength]);
            return Page();
        }
        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.PasswordMismatch"]);
            return Page();
        }
        if (Input.NewPassword == Input.CurrentPassword)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.PasswordSame"]);
            return Page();
        }

        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await users.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.Any(e => e.Code == "PasswordMismatch")
                ? L["Account.Error.CurrentPasswordWrong"]
                : L["Account.Error.Unknown"]);
            return Page();
        }

        user.MustChangePassword = false;
        await users.UpdateAsync(user);

        // کوکی از نو ساخته می‌شود تا پرچم «تغییر رمز اجباری» از آن برداشته شود
        await signInManager.RefreshSignInAsync(user);

        // مرحله‌ی ۵: همکار و مدیر کل بعد از تغییر رمز به پنل می‌روند، عضو به صفحه‌ی اصلی
        var roles = await users.GetRolesAsync(user);
        return LocalRedirect(LoginRedirectRules.ResolveTarget(roles, null));
    }
}
