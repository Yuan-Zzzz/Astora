namespace Astora.Core;

/// <summary>
/// Lightweight logging utility for Astora Engine.
/// Allows redirecting log output by assigning custom handlers.
/// </summary>
public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    /// <summary>
    /// Minimum log level to output. Messages below this level are ignored.
    /// </summary>
    public static LogLevel MinLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Custom log handler. Set this to redirect log output (e.g., to a file or UI console).
    /// Default: writes to Console.
    /// </summary>
    public static Action<LogLevel, string> LogHandler { get; set; } = DefaultLogHandler;

    public static void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public static void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public static void Warn(string message)
    {
        Log(LogLevel.Warn, message);
    }

    public static void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    public static void Error(string message, Exception ex)
    {
        Log(LogLevel.Error, $"{message}: {ex.Message}");
    }

    private static void Log(LogLevel level, string message)
    {
        if (level >= MinLevel)
        {
            LogHandler?.Invoke(level, message);
        }
    }

    private static void DefaultLogHandler(LogLevel level, string message)
    {
        Console.WriteLine($"[Astora/{level}] {message}");
    }
}
