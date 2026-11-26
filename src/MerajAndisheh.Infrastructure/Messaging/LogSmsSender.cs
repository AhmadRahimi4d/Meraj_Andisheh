using MerajAndisheh.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Infrastructure.Messaging;

/// <summary>
/// نسخه‌ی Dev: پیامک نمی‌فرستد و فقط در لاگ می‌نویسد (کد را از خط «DEV SMS» لاگ بردار).
/// ارائه‌دهنده‌ی واقعی پیامک پیش از مرحله‌ی ۲۲ جایگزین می‌شود. در تولید نباید این نسخه فعال بماند.
/// </summary>
public sealed class LogSmsSender(ILogger<LogSmsSender> logger) : ISmsSender
{
    public Task SendOtpCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("DEV SMS | To: {To} | Code: {Code}", phoneNumber, code);
        return Task.CompletedTask;
    }
}
