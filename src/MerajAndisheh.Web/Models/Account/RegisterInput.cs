namespace MerajAndisheh.Web.Models.Account;

public class RegisterInput
{
    public string? DisplayName { get; set; }
    public int? AgeGroupId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
