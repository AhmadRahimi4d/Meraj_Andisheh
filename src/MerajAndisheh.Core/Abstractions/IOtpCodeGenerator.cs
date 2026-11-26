namespace MerajAndisheh.Core.Abstractions;

/// <summary>تولید کد تأیید تصادفی. جدا شده تا در تست‌ها کد ثابت داد.</summary>
public interface IOtpCodeGenerator
{
    /// <summary>کدی با OtpRules.CodeLength رقم (ممکن است با صفر شروع شود).</summary>
    string Generate();
}
