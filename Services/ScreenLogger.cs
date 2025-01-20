using System;
using Microsoft.Extensions.Logging;

namespace NetworkMonitorBackup.Services
{
    public class ScreenLogger
    {
        private readonly object _lock = new();

        public void Log(string message, LogLevel level = LogLevel.Information)
        {
            lock (_lock)
            {
                var (color, prefix) = GetLogStyle(level);

                // Set console color based on log level
                Console.ForegroundColor = color;

                // Write styled message to the console
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{prefix}] {message}");

                // Reset color
                Console.ResetColor();
            }
        }

        private (ConsoleColor, string) GetLogStyle(LogLevel level)
        {
            return level switch
            {
                LogLevel.Critical => (ConsoleColor.Red, "CRITICAL"),
                LogLevel.Error => (ConsoleColor.DarkRed, "ERROR"),
                LogLevel.Warning => (ConsoleColor.Yellow, "WARN"),
                LogLevel.Information => (ConsoleColor.White, "INFO"),
                LogLevel.Debug => (ConsoleColor.Gray, "DEBUG"),
                LogLevel.Trace => (ConsoleColor.DarkGray, "TRACE"),
                _ => (ConsoleColor.White, "LOG")
            };
        }
    }
}
