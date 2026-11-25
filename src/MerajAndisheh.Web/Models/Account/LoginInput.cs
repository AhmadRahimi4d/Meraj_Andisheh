namespace MerajAndisheh.Web.Models.Account;

public class LoginInput
{
    /// <summary>ایمیل یا نام کاربری.</summary>
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool RememberMe { get; set; }
}
