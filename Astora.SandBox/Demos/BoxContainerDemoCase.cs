using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: vertical BoxContainer with three panels.</summary>
public sealed class BoxContainerDemoCase : IUIDemoCase
{
    public string Name => "Layout: BoxContainer (vertical)";

    public void Build(Node root)
    {
        LayoutDemos.BuildBoxContainerVertical(root);
    }
}
