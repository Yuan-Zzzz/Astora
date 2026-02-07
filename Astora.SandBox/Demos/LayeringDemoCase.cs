using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: Two CanvasLayers to verify draw and hit order.</summary>
public sealed class LayeringDemoCase : IUIDemoCase
{
    public string Name => "Layering: Two CanvasLayers";

    public void Build(Node root)
    {
        LayeringDemos.BuildTwoLayers(root);
    }
}
