using System;
using Microsoft.Extensions.Logging;

namespace NetworkMonitorBackup.Services
{
    public class CompositeLogger<T> : ILogger<T>
    {
        private readonly ILogger _serilogLogger;
        private readonly ScreenLogger _screenLogger;

        public CompositeLogger(ILoggerFactory loggerFactory, ScreenLogger screenLogger)
        {
            _serilogLogger = loggerFactory.CreateLogger<T>();
            _screenLogger = screenLogger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _serilogLogger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _serilogLogger.IsEnabled(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Format the message
            var message = formatter(state, exception);

            // Log to Serilog
            _serilogLogger.Log(logLevel, eventId, state, exception, formatter);

            // Log to ScreenLogger
            _screenLogger.Log(message, logLevel);
        }
    }
}
