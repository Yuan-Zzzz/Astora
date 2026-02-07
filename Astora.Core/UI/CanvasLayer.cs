using Astora.Core.Nodes;

namespace Astora.Core.UI;

/// <summary>
/// Logical container for UI trees (Godot-style). Does not draw; its direct Control children
/// are treated as UI roots for layout, hit testing, and rendering. Layer order: higher drawn and hit-tested on top.
/// </summary>
public class CanvasLayer : Node
{
    private int _layer;

    /// <summary>
    /// Layer index for draw and hit-test order. Higher layer is drawn and hit-tested on top.
    /// </summary>
    public int Layer
    {
        get => _layer;
        set => _layer = value;
    }

    public CanvasLayer() : base("CanvasLayer") { }

    public CanvasLayer(string name) : base(name) { }
}
