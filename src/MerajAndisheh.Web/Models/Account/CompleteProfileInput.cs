namespace MerajAndisheh.Web.Models.Account;

/// <summary>فرم تکمیل پروفایل پس از تأیید شماره‌ی موبایل.</summary>
public class CompleteProfileInput
{
    public string? DisplayName { get; set; }
    public int? AgeGroupId { get; set; }
}
