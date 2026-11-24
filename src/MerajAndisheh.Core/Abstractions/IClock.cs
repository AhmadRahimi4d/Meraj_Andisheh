namespace MerajAndisheh.Core.Abstractions;

/// <summary>
/// زمان فعلی (UTC). برای تست‌پذیری قوانینی که به زمان وابسته‌اند (مثلاً قاعده‌ی ۳۰ دقیقه‌ی بازدید).
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
