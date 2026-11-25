using Microsoft.AspNetCore.Identity;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// کاربر سایت (بر پایه‌ی ASP.NET Core Identity؛ ایمیل، موبایل و هش رمز را خود Identity نگه می‌دارد).
/// </summary>
public class AppUser : IdentityUser<int>
{
    /// <summary>نام نمایشی (لقب، نه نام واقعی). فقط همین در نظرها دیده می‌شود.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>رده‌ی سنی پروفایل. فقط رده ذخیره می‌شود، نه تاریخ تولد.</summary>
    public int? AgeGroupId { get; set; }

    public bool PersonalizationEnabled { get; set; } = true;

    /// <summary>برای مدیر کل اولیه با رمز موقت: تا رمز را عوض نکند به جای دیگر نمی‌رود.</summary>
    public bool MustChangePassword { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
