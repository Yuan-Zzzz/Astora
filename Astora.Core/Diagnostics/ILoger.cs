using System.Runtime.CompilerServices;

namespace Astora.Core.Diagnostics;

public interface ILogger
{
    LogLevel Level { get; set; }

    void Log(LogLevel level, string message,
        string? category = null,
        Exception? ex = null,
        [CallerMemberName] string? member = null);

    // 便捷方法
    void Trace(string msg, string? category = null) => Log(LogLevel.Trace, msg, category);
    void Debug(string msg, string? category = null) => Log(LogLevel.Debug, msg, category);
    void Info (string msg, string? category = null) => Log(LogLevel.Info , msg, category);
    void Warn (string msg, string? category = null) => Log(LogLevel.Warn , msg, category);
    void Error(string msg, string? category = null, Exception? ex = null) => Log(LogLevel.Error, msg, category, ex);
    void Fatal(string msg, string? category = null, Exception? ex = null) => Log(LogLevel.Fatal, msg, category, ex);
}