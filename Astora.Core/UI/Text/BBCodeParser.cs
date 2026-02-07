using System.Text;

namespace Astora.Core.UI.Text;

/// <summary>
/// Converts BBCode-style tags to FontStashSharp rich text commands.
/// Static mapping only; no animation. Use with Label when UseBBCode and RichText are true.
/// </summary>
public static class BBCodeParser
{
    /// <summary>
    /// Converts BBCode to FSS command string (no animation).
    /// </summary>
    public static string ToRichTextCommands(string bbCode) => ToRichTextCommands(bbCode, null);

    /// <summary>
    /// Converts BBCode to FSS command string. When animationTime is set, [wave], [shake], [rainbow] use it.
    /// </summary>
    public static string ToRichTextCommands(string bbCode, float? animationTime)
    {
        if (string.IsNullOrEmpty(bbCode)) return bbCode;

        var sb = new StringBuilder(bbCode.Length);
        var i = 0;
        while (i < bbCode.Length)
        {
            if (bbCode[i] == '[')
            {
                var close = bbCode.IndexOf(']', i);
                if (close == -1)
                {
                    sb.Append(bbCode[i]);
                    i++;
                    continue;
                }

                var tag = bbCode.Substring(i + 1, close - i - 1);
                i = close + 1;

                var (replacement, skipContent) = ParseTag(tag, animationTime);
                if (replacement != null)
                {
                    sb.Append(replacement);
                    if (skipContent && tag.StartsWith("/", StringComparison.Ordinal) == false)
                    {
                        var tagName = tag.Split('=')[0].Trim();
                        var endTag = "[/" + tagName + "]";
                        var endIdx = bbCode.IndexOf(endTag, i, StringComparison.OrdinalIgnoreCase);
                        if (endIdx != -1)
                        {
                            var inner = bbCode.Substring(i, endIdx - i);
                            sb.Append(ToRichTextCommands(inner, animationTime));
                            i = endIdx + endTag.Length;
                            sb.Append(GetClosingCommand(tagName));
                            continue;
                        }
                    }
                }
                else
                {
                    sb.Append('[').Append(tag).Append(']');
                }
                continue;
            }

            if (bbCode[i] == '\\' && i + 1 < bbCode.Length && bbCode[i + 1] == '[')
            {
                sb.Append('[');
                i += 2;
                continue;
            }

            sb.Append(bbCode[i]);
            i++;
        }

        return sb.ToString();
    }

    private static (string? Replacement, bool SkipContent) ParseTag(string tag, float? time)
    {
        var t = tag.Trim();
        if (t.StartsWith("/", StringComparison.Ordinal))
        {
            var name = t.AsSpan(1).Trim().ToString();
            return (GetClosingCommand(name), false);
        }

        if (t.StartsWith("color=", StringComparison.OrdinalIgnoreCase))
        {
            var value = t.Substring(6).Trim();
            if (value.Length > 0)
                return ("/c[" + value + "]", true);
        }

        if (string.Equals(t, "b", StringComparison.OrdinalIgnoreCase))
            return ("/es", true);
        if (string.Equals(t, "stroke", StringComparison.OrdinalIgnoreCase))
            return ("/es", true);
        if (string.Equals(t, "n", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "br", StringComparison.OrdinalIgnoreCase))
            return ("/n", false);
        if (string.Equals(t, "i", StringComparison.OrdinalIgnoreCase))
            return (null, false);

        if (time.HasValue)
        {
            if (string.Equals(t, "wave", StringComparison.OrdinalIgnoreCase))
            {
                var offset = (int)(4 * Math.Sin(time.Value * 4));
                return ("/v[" + offset + "]", true);
            }
            if (string.Equals(t, "rainbow", StringComparison.OrdinalIgnoreCase))
            {
                var (r, g, b) = HsvToRgb((time.Value * 2f) % 1f, 1f, 1f);
                var hex = $"#{(int)(r * 255):X2}{(int)(g * 255):X2}{(int)(b * 255):X2}";
                return ("/c[" + hex + "]", true);
            }
            if (string.Equals(t, "shake", StringComparison.OrdinalIgnoreCase))
            {
                var seed = (int)(time.Value * 60);
                var ox = (seed * 7) % 3 - 1;
                var oy = (seed * 11) % 3 - 1;
                return ("/v[" + oy + "]", true);
            }
        }

        return (null, false);
    }

    private static (float R, float G, float B) HsvToRgb(float h, float s, float v)
    {
        if (s <= 0) return (v, v, v);
        var i = (int)(h * 6);
        var f = h * 6 - i;
        i %= 6;
        var p = v * (1 - s);
        var q = v * (1 - s * f);
        var t_ = v * (1 - s * (1 - f));
        return i switch
        {
            0 => (v, t_, p),
            1 => (q, v, p),
            2 => (p, v, t_),
            3 => (p, q, v),
            4 => (t_, p, v),
            _ => (v, p, q)
        };
    }

    private static string GetClosingCommand(string tagName)
    {
        var name = tagName.Trim();
        if (string.Equals(name, "color", StringComparison.OrdinalIgnoreCase)) return "/cd";
        if (string.Equals(name, "b", StringComparison.OrdinalIgnoreCase)) return "/ed";
        if (string.Equals(name, "stroke", StringComparison.OrdinalIgnoreCase)) return "/ed";
        if (string.Equals(name, "i", StringComparison.OrdinalIgnoreCase)) return "/vd";
        if (string.Equals(name, "wave", StringComparison.OrdinalIgnoreCase)) return "/vd";
        if (string.Equals(name, "rainbow", StringComparison.OrdinalIgnoreCase)) return "/cd";
        if (string.Equals(name, "shake", StringComparison.OrdinalIgnoreCase)) return "/vd";
        return string.Empty;
    }
}
