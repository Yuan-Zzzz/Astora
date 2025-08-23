using System.Diagnostics;

namespace Astora.Core.Diagnostics;

public sealed class ConsoleLogger : ILogger
{
    private static readonly object _gate = new();
    public LogLevel Level { get; set; } = LogLevel.Info;

    public void Log(LogLevel level, string message, string? category = null, Exception? ex = null, string? member = null)
    {
        if (level < Level) return;

        var time = DateTime.Now.ToString("HH:mm:ss.fff");
        var cat  = string.IsNullOrWhiteSpace(category) ? "-" : category;
        var lvl  = level.ToString().ToUpper();

        lock (_gate)
        {
            Console.WriteLine($"[{time}] [{lvl,-5}] [{cat}] {message}{(member is null ? "" : $"  <{member}>")}");
            if (ex is not null)
            {
                Console.WriteLine(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}