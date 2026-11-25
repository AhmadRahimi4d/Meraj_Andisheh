using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using MerajAndisheh.Web.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Pages.Account;

public class ForgotPasswordModel(UserManager<AppUser> users, AccountEmailComposer emails) : PageModel
{
    [BindProperty]
    public EmailInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var email = Input.Email?.Trim();
        var user = string.IsNullOrEmpty(email) ? null : await users.FindByEmailAsync(email);

        // پاسخ همیشه یکسان است تا معلوم نشود ایمیل در سایت ثبت شده یا نه
        if (user is not null && user.IsActive && await users.IsEmailConfirmedAsync(user))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var link = Url.PageLink("/Account/ResetPassword", values: new { userId = user.Id, code = TokenEncoding.Encode(token) })!;
            await emails.SendPasswordResetAsync(user, link, cancellationToken);
        }

        return RedirectToPage("/Account/CheckEmail", new { kind = "reset" });
    }
}
