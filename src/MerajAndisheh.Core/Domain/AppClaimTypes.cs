namespace MerajAndisheh.Core.Domain;

/// <summary>نوع Claim های اختصاصی که هنگام ورود در کوکی قرار می‌گیرند.</summary>
public static class AppClaimTypes
{
    public const string DisplayName = "ma:display_name";
    public const string MustChangePassword = "ma:must_change_password";

    /// <summary>کلید دسترسی همکار (یک Claim برای هر کلید). هنگام ورود از StaffPermission خوانده می‌شود، پس تغییر دسترسی پس از ورود مجدد اعمال می‌شود.</summary>
    public const string Permission = "ma:permission";
}
