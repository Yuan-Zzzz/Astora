using System.Collections.Generic;
using System.Linq;
using Astora.Core.Nodes;
using Astora.Core.UI;

namespace Astora.SandBox.Application;

/// <summary>
/// Runs the current UI demo case by building its UI tree on the scene root.
/// Supports cycling through a list of demos (e.g. via F2 key) without recompiling.
/// </summary>
public sealed class DemoRunner
{
    private readonly IReadOnlyList<IUIDemoCase> _demos;
    private int _index;

    public DemoRunner(IUIDemoCase singleDemo)
    {
        _demos = new[] { singleDemo };
        _index = 0;
    }

    public DemoRunner(IReadOnlyList<IUIDemoCase> demos)
    {
        _demos = demos?.Count > 0 ? demos : new List<IUIDemoCase>();
        _index = 0;
    }

    /// <summary>Builds the current demo's UI tree under <paramref name="sceneRoot"/>.</summary>
    public void Run(Node sceneRoot)
    {
        ClearUIChildren(sceneRoot);
        _demos[_index].Build(sceneRoot);
    }

    /// <summary>Switches to the next demo and rebuilds UI on <paramref name="sceneRoot"/>.</summary>
    public void SwitchToNext(Node sceneRoot)
    {
        if (_demos.Count == 0) return;
        _index = (_index + 1) % _demos.Count;
        Run(sceneRoot);
    }

    public string CurrentDemoName => _demos.Count > 0 ? _demos[_index].Name : "";

    private static void ClearUIChildren(Node sceneRoot)
    {
        var toRemove = sceneRoot.Children
            .Where(c => c is Control || c is CanvasLayer)
            .ToList();
        foreach (var child in toRemove)
            sceneRoot.RemoveChild(child);
    }
}
