using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.Core.UI.Events;

/// <summary>
/// Event args for mouse button events (press/release). Position is in design resolution (screen) space.
/// </summary>
public class MouseButtonEventArgs : UIEventArgs
{
    /// <summary>
    /// Mouse position in design resolution coordinates at the time of the event.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// The button that was pressed or released.
    /// </summary>
    public MouseButton Button { get; init; }

    /// <summary>
    /// True when button was pressed, false when released.
    /// </summary>
    public bool Pressed { get; init; }
}
