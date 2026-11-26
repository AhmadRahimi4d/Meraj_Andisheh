using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Core.Domain;
using MerajAndisheh.Infrastructure;
using MerajAndisheh.Infrastructure.Messaging;
using MerajAndisheh.Infrastructure.Otp;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

/// <summary>ثبت سرویس‌های OTP در AddInfrastructure و تولیدکننده‌ی کد تصادفی.</summary>
public class OtpRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersDevSmsSenderAndOtpPieces()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure("Server=.;Database=Test;Trusted_Connection=True;TrustServerCertificate=True");

        using var provider = services.BuildServiceProvider();

        Assert.IsType<LogSmsSender>(provider.GetRequiredService<ISmsSender>());
        Assert.IsType<RandomOtpCodeGenerator>(provider.GetRequiredService<IOtpCodeGenerator>());
        Assert.NotNull(provider.GetRequiredService<OtpCodeHasher>());
    }

    [Fact]
    public void RandomGenerator_ProducesSixDigitCodes_WithVariety()
    {
        var generator = new RandomOtpCodeGenerator();

        var codes = Enumerable.Range(0, 300).Select(_ => generator.Generate()).ToList();

        Assert.All(codes, code =>
        {
            Assert.Equal(OtpRules.CodeLength, code.Length);
            Assert.True(code.All(char.IsAsciiDigit));
        });
        Assert.True(codes.Distinct().Count() > 250, "کدها نباید تکراری یا قابل‌پیش‌بینی باشند");
    }
}
