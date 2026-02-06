using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Layout;

/// <summary>
/// Two-pass layout interface. Pass 1: ComputeDesiredSize (bottom-up). Pass 2: ArrangeChildren (top-down).
/// </summary>
public interface ILayoutable
{
    /// <summary>
    /// Pass 1 (bottom-up). Compute and return desired size; may depend on children's DesiredSize.
    /// </summary>
    Vector2 ComputeDesiredSize();

    /// <summary>
    /// Pass 2 (top-down). Assign final rect to this control and arrange children within the given rectangle.
    /// </summary>
    /// <param name="finalRect">The rectangle allocated to this control (in parent/screen space).</param>
    void ArrangeChildren(Rectangle finalRect);
}
