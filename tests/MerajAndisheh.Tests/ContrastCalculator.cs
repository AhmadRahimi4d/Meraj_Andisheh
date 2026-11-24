namespace MerajAndisheh.Tests;

/// <summary>محاسبه‌ی نسبت کنتراست WCAG 2.x بین دو رنگ hex (مثل #FFFFFF).</summary>
public static class ContrastCalculator
{
    public static double Ratio(string hexA, string hexB)
    {
        var a = Luminance(hexA);
        var b = Luminance(hexB);
        var lighter = Math.Max(a, b);
        var darker = Math.Min(a, b);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double Luminance(string hex)
    {
        var value = hex.TrimStart('#');
        var r = Channel(Convert.ToInt32(value.Substring(0, 2), 16));
        var g = Channel(Convert.ToInt32(value.Substring(2, 2), 16));
        var b = Channel(Convert.ToInt32(value.Substring(4, 2), 16));
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static double Channel(int value)
    {
        var c = value / 255.0;
        return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
