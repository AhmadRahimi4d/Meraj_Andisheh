using System.Security.Cryptography;
using MerajAndisheh.Core.Abstractions;

namespace MerajAndisheh.Infrastructure.Identity;

/// <summary>
/// رمز موقت تصادفی امن برای حساب همکار: ۱۴ نویسه، بدون نویسه‌های شبیه هم (I، l، 1، O، 0)،
/// با دست‌کم یک حرف بزرگ، یک حرف کوچک و یک رقم. همکار در اولین ورود مجبور به عوض‌کردنش است.
/// </summary>
public sealed class TemporaryPasswordGenerator : ITemporaryPasswordGenerator
{
    private const int Length = 14;
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";
    private const string AllChars = Upper + Lower + Digits;

    public string Generate()
    {
        while (true)
        {
            var chars = new char[Length];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = AllChars[RandomNumberGenerator.GetInt32(AllChars.Length)];
            }

            var candidate = new string(chars);
            if (candidate.Any(Upper.Contains) && candidate.Any(Lower.Contains) && candidate.Any(Digits.Contains))
                return candidate;
        }
    }
}
