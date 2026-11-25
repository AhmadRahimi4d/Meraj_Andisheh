namespace MerajAndisheh.Web.Models.Account;

public class ResetPasswordInput
{
    public int UserId { get; set; }

    /// <summary>توکن کدگذاری‌شده‌ی لینک ایمیل.</summary>
    public string? Code { get; set; }

    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
