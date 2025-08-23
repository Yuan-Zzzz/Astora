using System.Diagnostics;

namespace Astora.Core.Diagnostics;

public readonly struct MeasureScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _name;
    private readonly Stopwatch _sw;

    public MeasureScope(ILogger logger, string name)
    {
        _logger = logger;
        _name = name;
        _sw = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _sw.Stop();
        _logger.Debug($"{_name} took {_sw.Elapsed.TotalMilliseconds:F2} ms", category: "perf");
    }
}