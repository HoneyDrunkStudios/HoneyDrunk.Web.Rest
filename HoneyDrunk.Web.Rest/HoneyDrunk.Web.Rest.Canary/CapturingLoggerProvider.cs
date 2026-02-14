using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// A logger provider that captures log entries for canary assertions.
/// </summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Gets the captured log entries.
    /// </summary>
    public ConcurrentBag<CapturedLogEntry> Entries { get; } = [];

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new CapturingLogger(categoryName, this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // No resources to dispose.
    }

    private sealed class CapturingLogger(string category, CapturingLoggerProvider provider) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            provider.Entries.Add(new CapturedLogEntry
            {
                Category = category,
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception,
            });
        }
    }
}
