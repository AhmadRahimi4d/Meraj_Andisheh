using MerajAndisheh.Core.Abstractions;

namespace MerajAndisheh.Tests;

/// <summary>
/// تولیدکننده‌ی کد قطعی برای تست: اول از صف کدهای دستی، بعد کدهای پشت‌سرهم 100001، 100002، ...
/// (تا تست‌ها به‌خاطر برخورد تصادفی دو کد، گاهی خطا ندهند).
/// </summary>
public sealed class FakeOtpCodeGenerator : IOtpCodeGenerator
{
    private readonly Queue<string> _queued = new();
    private int _counter = 100000;

    public void Enqueue(params string[] codes)
    {
        foreach (var code in codes) _queued.Enqueue(code);
    }

    public string Generate()
    {
        if (_queued.Count > 0) return _queued.Dequeue();
        _counter++;
        return _counter.ToString();
    }
}
