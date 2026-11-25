using MerajAndisheh.Infrastructure.Identity;
using MerajAndisheh.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerajAndisheh.Web.Pages.Account;

public class ConfirmEmailModel(UserManager<AppUser> users) : PageModel
{
    public bool Succeeded { get; private set; }

    public async Task OnGetAsync(int? userId, string? code)
    {
        var token = TokenEncoding.Decode(code);
        if (userId is null || token is null) return;

        var user = await users.FindByIdAsync(userId.Value.ToString());
        if (user is null) return;

        var result = await users.ConfirmEmailAsync(user, token);
        Succeeded = result.Succeeded;
    }
}
