using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace MerajAndisheh.Web.Identity;

/// <summary>کدگذاری توکن‌های Identity برای قرار گرفتن امن در لینک ایمیل.</summary>
public static class TokenEncoding
{
    public static string Encode(string token)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public static string? Decode(string? encoded)
    {
        if (string.IsNullOrEmpty(encoded)) return null;

        try
        {
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encoded));
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
