using Microsoft.Xna.Framework;

namespace Astora.Core.UI.Layout;

/// <summary>
/// Information reported by a child to its parent during layout (Pass 1). Used by containers like BoxContainer.
/// </summary>
public struct DesiredSizeInfo
{
    /// <summary>
    /// Desired size of the control.
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// Minimum size constraint (optional). Used by layout to enforce minimums.
    /// </summary>
    public Vector2 MinSize;

    /// <summary>
    /// Stretch ratio in container layout (e.g. BoxContainer). Higher values get more space when extra is available.
    /// </summary>
    public float StretchRatio;

    public static DesiredSizeInfo FromSize(Vector2 size, Vector2 minSize = default, float stretchRatio = 0f)
    {
        return new DesiredSizeInfo
        {
            Size = size,
            MinSize = minSize,
            StretchRatio = stretchRatio
        };
    }
}
