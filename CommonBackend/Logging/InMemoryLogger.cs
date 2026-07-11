using Microsoft.Extensions.Logging;

namespace CommonBackend.Logging;

public interface IInMemoryLogStore
{
    void Add(string message);
    List<string> GetLogs();
}

public class InMemoryLogStore : IInMemoryLogStore
{
    private readonly List<string> _logs = new();
    private readonly object _lock = new();

    public void Add(string message)
    {
        lock (_lock)
        {
            _logs.Add($"{DateTime.Now:HH:mm:ss} - {message}");
            if (_logs.Count > 500) _logs.RemoveAt(0);
        }
    }

    public List<string> GetLogs()
    {
        lock (_lock) return new List<string>(_logs);
    }
}

public class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly IInMemoryLogStore _logStore;

    public InMemoryLoggerProvider(IInMemoryLogStore logStore)
    {
        _logStore = logStore;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new InMemoryLogger(_logStore, categoryName);
    }

    public void Dispose() { }

    private class InMemoryLogger : ILogger
    {
        private readonly IInMemoryLogStore _logStore;
        private readonly string _categoryName;

        public InMemoryLogger(IInMemoryLogStore logStore, string categoryName)
        {
            _logStore = logStore;
            _categoryName = categoryName;
        }

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = $"{logLevel} [{_categoryName}] {formatter(state, exception)}";
            _logStore.Add(message);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
