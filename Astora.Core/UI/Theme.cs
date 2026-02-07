using System.Collections.Generic;
using Astora.Core.Resources;
using Microsoft.Xna.Framework;

namespace Astora.Core.UI;

/// <summary>
/// Theme resource (Godot Theme style). Holds default colors, constants and fonts by name.
/// Controls inherit theme from parent; override with AddThemeColorOverride etc.
/// </summary>
public class Theme
{
    private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>();
    private readonly Dictionary<string, int> _constants = new Dictionary<string, int>();
    private readonly Dictionary<string, FontResource> _fonts = new Dictionary<string, FontResource>();

    public void SetColor(string name, Color color)
    {
        _colors[name] = color;
    }

    public void SetConstant(string name, int value)
    {
        _constants[name] = value;
    }

    public void SetFont(string name, FontResource font)
    {
        _fonts[name] = font;
    }

    public bool GetColor(string name, out Color color)
    {
        return _colors.TryGetValue(name, out color);
    }

    public bool GetConstant(string name, out int value)
    {
        return _constants.TryGetValue(name, out value);
    }

    public bool GetFont(string name, out FontResource font)
    {
        return _fonts.TryGetValue(name, out font);
    }
}
