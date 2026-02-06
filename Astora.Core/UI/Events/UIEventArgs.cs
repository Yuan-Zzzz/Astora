using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Events;

/// <summary>
/// Base class for UI event arguments. Supports event routing with Handled to stop propagation.
/// </summary>
public class UIEventArgs
{
    /// <summary>
    /// When set to true, stops further propagation (tunneling or bubbling) of the event.
    /// </summary>
    public bool Handled { get; set; }
}
