using Microsoft.Extensions.Logging;

namespace MerajAndisheh.Tests;

/// <summary>لاگ‌های نوشته‌شده را برای تست نگه می‌دارد (تا «ثبت تغییر در لاگ» قابل‌سنجش باشد).</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages
    {
        get
        {
            lock (_messages) return _messages.ToList();
        }
    }

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

    public void Dispose()
    {
    }

    private sealed class CapturingLogger(CapturingLoggerProvider owner) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (owner._messages) owner._messages.Add($"{logLevel}: {formatter(state, exception)}");
        }
    }
}
