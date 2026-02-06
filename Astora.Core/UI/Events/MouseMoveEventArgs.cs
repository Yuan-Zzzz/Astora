using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Events;

/// <summary>
/// Event args for mouse move. Position is in design resolution (screen) space.
/// </summary>
public class MouseMoveEventArgs : UIEventArgs
{
    /// <summary>
    /// Current mouse position in design resolution coordinates.
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// Previous frame mouse position in design resolution coordinates.
    /// </summary>
    public Vector2 PreviousPosition { get; init; }
}
