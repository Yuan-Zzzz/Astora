namespace Astora.Core.UI.Rendering;

/// <summary>
/// Scoped context for the current UI draw pass. Set by the render pass before drawing UI, cleared after.
/// Allows controls (e.g. Panel) to obtain the white texture without depending on engine context.
/// </summary>
public static class UIDrawContext
{
    [ThreadStatic]
    private static IWhiteTextureProvider? s_current;

    /// <summary>
    /// Current white texture provider for this frame's UI draw. Null when not drawing UI.
    /// </summary>
    public static IWhiteTextureProvider? Current => s_current;

    /// <summary>
    /// Sets the current provider. Called by the render pass before drawing the UI subtree.
    /// </summary>
    public static void SetCurrent(IWhiteTextureProvider? provider)
    {
        s_current = provider;
    }
}
