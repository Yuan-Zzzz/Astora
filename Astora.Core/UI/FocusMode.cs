namespace Astora.Core.UI;

/// <summary>
/// Whether a control can receive focus.
/// </summary>
public enum FocusMode
{
    /// <summary>
    /// Cannot receive focus.
    /// </summary>
    None,

    /// <summary>
    /// Can receive focus when clicked.
    /// </summary>
    Click,

    /// <summary>
    /// Can receive focus from click or keyboard navigation.
    /// </summary>
    All
}
