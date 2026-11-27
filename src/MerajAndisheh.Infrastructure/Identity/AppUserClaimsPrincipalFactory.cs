using System.Security.Claims;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// نام نمایشی، پرچم «تغییر رمز اجباری» و (برای همکار) کلیدهای دسترسی را به Claim های کوکی ورود اضافه می‌کند.
/// چون دسترسی‌ها هنگام ورود در کوکی می‌نشینند، تغییر دسترسی همکار پس از ورود مجدد او اعمال می‌شود
/// و هر درخواست پنل به دیتابیس نمی‌رود. مدیر کل کلید ندارد؛ همه‌ی کلیدها را ضمنی دارد.
/// </summary>
public sealed class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<IdentityOptions> options,
    AppDbContext db)
    : UserClaimsPrincipalFactory<AppUser, AppRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(AppClaimTypes.DisplayName, user.DisplayName));
        identity.AddClaim(new Claim(AppClaimTypes.MustChangePassword, user.MustChangePassword ? "true" : "false"));

        if (await UserManager.IsInRoleAsync(user, RoleNames.Staff))
        {
            var keys = await db.StaffPermissions
                .Where(p => p.UserId == user.Id)
                .Select(p => p.PermissionKey)
                .ToListAsync();

            foreach (var key in keys)
            {
                identity.AddClaim(new Claim(AppClaimTypes.Permission, key));
            }
        }

        return identity;
    }
}
