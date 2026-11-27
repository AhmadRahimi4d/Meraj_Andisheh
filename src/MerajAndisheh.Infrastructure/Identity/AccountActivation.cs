using Microsoft.AspNetCore.Identity;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// فعال/غیرفعال‌کردن حساب (مشترک بین صفحه‌ی کاربران و صفحه‌ی همکاران). غیرفعال‌کردن Security Stamp را هم عوض می‌کند
/// تا نشست‌های باز، حداکثر پس از AccountRules.SessionValidationMinutes دقیقه از کار بیفتند.
/// </summary>
internal static class AccountActivation
{
    public static async Task<bool> ApplyAsync(UserManager<AppUser> users, AppUser user, bool active)
    {
        user.IsActive = active;

        var update = await users.UpdateAsync(user);
        if (!update.Succeeded) return false;

        if (!active)
        {
            var stamp = await users.UpdateSecurityStampAsync(user);
            if (!stamp.Succeeded) return false;
        }

        return true;
    }
}
