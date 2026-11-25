namespace MerajAndisheh.Web.Models.Account;

public class ChangePasswordInput
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}
