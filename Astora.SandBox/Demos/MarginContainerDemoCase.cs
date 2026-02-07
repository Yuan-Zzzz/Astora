using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: MarginContainer with single panel.</summary>
public sealed class MarginContainerDemoCase : IUIDemoCase
{
    public string Name => "Layout: MarginContainer";

    public void Build(Node root)
    {
        LayoutDemos.BuildMarginContainer(root);
    }
}
