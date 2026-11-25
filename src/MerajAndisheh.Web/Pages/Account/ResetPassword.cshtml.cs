using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

public class ResetPasswordModel(UserManager<AppUser> users, IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    /// <summary>لینک بدون پارامتر یا خراب است.</summary>
    public bool LinkInvalid { get; private set; }

    /// <summary>رمز با موفقیت عوض شد.</summary>
    public bool Done { get; private set; }

    public void OnGet(int? userId, string? code)
    {
        if (userId is null || string.IsNullOrEmpty(code))
        {
            LinkInvalid = true;
            return;
        }

        Input.UserId = userId.Value;
        Input.Code = code;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var token = TokenEncoding.Decode(Input.Code);
        if (token is null)
        {
            LinkInvalid = true;
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

        var user = await users.FindByIdAsync(Input.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, L["Account.Error.TokenInvalid"]);
            return Page();
        }

        var result = await users.ResetPasswordAsync(user, token, Input.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.Any(e => e.Code == "PasswordTooShort")
                ? L["Account.Error.PasswordTooShort", AccountRules.MinPasswordLength]
                : L["Account.Error.TokenInvalid"]);
            return Page();
        }

        // بعد از بازنشانی، قفل و تلاش‌های ناموفق قبلی پاک شود
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);

        Done = true;
        return Page();
    }
}
