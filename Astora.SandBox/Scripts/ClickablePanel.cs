using Astora.Core.UI;
using Astora.Core.UI.Events;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scripts;

/// <summary>
/// Panel that handles click (sets Handled) and toggles color for interactive UI testing.
/// </summary>
public class ClickablePanel : Panel
{
    private bool _toggled;

    public ClickablePanel() : base("ClickablePanel") { }

    public ClickablePanel(string name) : base(name) { }

    public override void OnMouseButtonDown(MouseButtonEventArgs e)
    {
        e.Handled = true;
        _toggled = !_toggled;
        Modulate = _toggled
            ? new Color(200, 220, 100, 255)
            : new Color(80, 180, 120, 220);
    }
}
