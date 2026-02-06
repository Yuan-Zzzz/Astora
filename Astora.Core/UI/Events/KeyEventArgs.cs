using Microsoft.Xna.Framework.Input;

namespace Astora.Core.UI.Events;

/// <summary>
/// Keyboard key event arguments. Used for key down/up routing with Handled.
/// </summary>
public class KeyEventArgs : UIEventArgs
{
    public Keys Key { get; set; }
    public bool Pressed { get; set; }
}
