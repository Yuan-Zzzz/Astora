using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: Button with click for interactive UI testing.</summary>
public sealed class ButtonClickDemoCase : IUIDemoCase
{
    public string Name => "Interaction: Button click";

    public void Build(Node root)
    {
        InteractionDemos.BuildButtonClick(root);
    }
}
