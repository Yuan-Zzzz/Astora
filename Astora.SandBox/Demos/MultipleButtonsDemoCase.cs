using Astora.Core.Nodes;
using Astora.SandBox.Application;

namespace Astora.SandBox.Demos;

/// <summary>Demo case: Row of buttons for focus and hit-testing.</summary>
public sealed class MultipleButtonsDemoCase : IUIDemoCase
{
    public string Name => "Interaction: Multiple buttons";

    public void Build(Node root)
    {
        InteractionDemos.BuildMultipleButtons(root);
    }
}
