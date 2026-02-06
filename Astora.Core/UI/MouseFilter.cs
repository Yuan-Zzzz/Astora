namespace Astora.Core.UI;

/// <summary>
/// Whether a control participates in hit testing and whether input passes through to children.
/// </summary>
public enum MouseFilter
{
    /// <summary>
    /// Ignore mouse events (not hit, pass through).
    /// </summary>
    Ignore,

    /// <summary>
    /// Receive events and continue hit testing children.
    /// </summary>
    Pass,

    /// <summary>
    /// Receive events and stop hit testing (this control is the leaf for hit test).
    /// </summary>
    Stop
}
