using System.ComponentModel.DataAnnotations;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace MerajAndisheh.Web.Pages.Account;

public class RegisterModel(
    AccountRegistrationService registration,
    AccountEmailComposer emails,
    AppDbContext db,
    IStringLocalizer<SharedResource> L) : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public List<SelectListItem> AgeGroups { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true) return LocalRedirect("/");

        await LoadAgeGroupsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        await LoadAgeGroupsAsync();

        var email = Input.Email?.Trim() ?? string.Empty;
        if (email.Length == 0 || !new EmailAddressAttribute().IsValid(email))
            ModelState.AddModelError("Input.Email", L["Account.Error.EmailInvalid"]);

        if (string.IsNullOrEmpty(Input.Password) || Input.Password.Length < AccountRules.MinPasswordLength)
            ModelState.AddModelError("Input.Password", L["Account.Error.PasswordTooShort", AccountRules.MinPasswordLength]);
        else if (Input.Password != Input.ConfirmPassword)
            ModelState.AddModelError("Input.ConfirmPassword", L["Account.Error.PasswordMismatch"]);

        var displayName = Input.DisplayName?.Trim() ?? string.Empty;
        if (displayName.Length == 0 || displayName.Length > AccountRules.DisplayNameMaxLength)
            ModelState.AddModelError("Input.DisplayName", L["Account.Error.DisplayNameInvalid", AccountRules.DisplayNameMaxLength]);

        if (Input.AgeGroupId is null)
            ModelState.AddModelError("Input.AgeGroupId", L["Account.Error.AgeGroupInvalid"]);

        if (!ModelState.IsValid) return Page();

        var result = await registration.RegisterAsync(
            new RegistrationRequest(email, Input.Password, displayName, Input.AgeGroupId), cancellationToken);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                // آرگومان {0} پیام: برای نام نمایشی سقف طول، برای رمز حداقل طول (در بقیه‌ی پیام‌ها استفاده نمی‌شود)
                var argument = error == RegistrationError.DisplayNameInvalid
                    ? AccountRules.DisplayNameMaxLength
                    : AccountRules.MinPasswordLength;
                ModelState.AddModelError(string.Empty, L[$"Account.Error.{error}", argument]);
            }
            return Page();
        }

        var link = Url.PageLink("/Account/ConfirmEmail", values: new
        {
            userId = result.User!.Id,
            code = TokenEncoding.Encode(result.ConfirmationToken!)
        })!;
        await emails.SendConfirmationAsync(result.User, link, cancellationToken);

        return RedirectToPage("/Account/CheckEmail", new { kind = "register" });
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
