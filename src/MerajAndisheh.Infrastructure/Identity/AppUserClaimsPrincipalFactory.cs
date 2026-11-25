using System.Security.Claims;
using MerajAndisheh.Core.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>نام نمایشی و پرچم «تغییر رمز اجباری» را به Claim های کوکی ورود اضافه می‌کند.</summary>
public sealed class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<AppUser, AppRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(AppClaimTypes.DisplayName, user.DisplayName));
        identity.AddClaim(new Claim(AppClaimTypes.MustChangePassword, user.MustChangePassword ? "true" : "false"));
        return identity;
    }
}
