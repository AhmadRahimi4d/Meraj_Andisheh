using System.Security.Claims;
using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Web.Identity;

/// <summary>خواندن اطلاعات کاربر جاری از Claim های کوکی.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>شناسه‌ی عددی کاربر، یا null اگر وارد نشده باشد.</summary>
    public static int? GetUserId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole(RoleNames.SuperAdmin);

    /// <summary>کلیدهای دسترسی داده‌شده به همکار (برای مدیر کل خالی است؛ او همه را ضمنی دارد).</summary>
    public static IReadOnlyList<string> GetPermissions(this ClaimsPrincipal user)
        => user.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToList();
}
