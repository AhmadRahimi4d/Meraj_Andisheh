using System.Globalization;
using System.Security.Cryptography;
using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;

namespace MerajAndisheh.Infrastructure.Otp;

/// <summary>کد تصادفی امن (RandomNumberGenerator، نه Random).</summary>
public sealed class RandomOtpCodeGenerator : IOtpCodeGenerator
{
    public string Generate()
    {
        var upperBound = 1;
        for (var i = 0; i < OtpRules.CodeLength; i++) upperBound *= 10;

        var number = RandomNumberGenerator.GetInt32(upperBound);
        return number.ToString("D" + OtpRules.CodeLength, CultureInfo.InvariantCulture);
    }
}
