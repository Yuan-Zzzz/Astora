namespace Astora.Core.Diagnostics;

public sealed class CompositeLogger : ILogger
{
    private readonly ILogger[] _loggers;
    public CompositeLogger(params ILogger[] loggers) => _loggers = loggers;
    public LogLevel Level { get; set; } = LogLevel.Info;

    public void Log(LogLevel level, string message, string? category = null, Exception? ex = null, string? member = null)
    {
        foreach (var l in _loggers)
        {
            // 子 logger 自己也有 Level；这里同时考虑总阈值
            if (level >= Level) l.Log(level, message, category, ex, member);
        }
    }
}