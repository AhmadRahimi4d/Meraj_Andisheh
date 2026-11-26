namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>دلیل ناموفق بودن ثبت‌نام. فقط کد است؛ متن فارسی از resx می‌آید.</summary>
public enum RegistrationError
{
    DisplayNameInvalid = 1,
    AgeGroupInvalid,
    EmailInvalid,
    EmailTaken,
    PasswordTooShort,
    Unknown,

    // مرحله‌ی ۵ (ثبت‌نام با موبایل)
    PhoneInvalid,
    PhoneTaken
}
