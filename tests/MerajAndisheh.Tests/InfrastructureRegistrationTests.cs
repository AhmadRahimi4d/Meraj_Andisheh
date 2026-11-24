using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure;
using MerajAndisheh.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MerajAndisheh.Tests;

public class InfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructure_RegistersClock()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure("Server=.;Database=Test;Trusted_Connection=True;TrustServerCertificate=True");

        using var provider = services.BuildServiceProvider();

        Assert.IsType<SystemClock>(provider.GetRequiredService<IClock>());
    }

    [Fact]
    public void AddInfrastructure_EmptyConnectionString_Throws()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() => services.AddInfrastructure(""));
    }
}
