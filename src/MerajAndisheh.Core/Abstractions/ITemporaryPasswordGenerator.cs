namespace MerajAndisheh.Core.Abstractions;

/// <summary>تولید رمز موقت برای حساب همکار. جدا شده تا در تست‌ها قابل‌جایگزینی باشد.</summary>
public interface ITemporaryPasswordGenerator
{
    string Generate();
}
