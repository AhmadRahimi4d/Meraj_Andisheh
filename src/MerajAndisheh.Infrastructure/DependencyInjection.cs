using MerajAndisheh.Core.Abstractions;
using MerajAndisheh.Infrastructure.Data;
using MerajAndisheh.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MerajAndisheh.Infrastructure;

/// <summary>ثبت سرویس‌های لایه‌ی Infrastructure (مثل ماژول Hilt در اندروید).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is empty.", nameof(connectionString));

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
