using MerajAndisheh.Core.Abstractions;

namespace MerajAndisheh.Tests;

/// <summary>پیامک‌فرست دست‌ساز: کدهای ارسال‌شده را نگه می‌دارد.</summary>
public sealed class FakeSmsSender : ISmsSender
{
    public sealed record SentCode(string PhoneNumber, string Code);

    public List<SentCode> Sent { get; } = [];

    public string LastCode => Sent[^1].Code;

    public Task SendOtpCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        Sent.Add(new SentCode(phoneNumber, code));
        return Task.CompletedTask;
    }
}
