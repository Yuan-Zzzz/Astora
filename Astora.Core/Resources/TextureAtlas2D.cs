using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Resources;

public class TextureAtlas2D
{
    public Texture2D SourceTexture { get; private set; }

    private Dictionary<string, Rectangle> _frames = new Dictionary<string, Rectangle>();

    public TextureAtlas2D(Texture2D sourceTexture)
    {
        SourceTexture = sourceTexture;
        _frames = new Dictionary<string, Rectangle>();
    }
    
    public void AddFrame(string name, Rectangle region)
    {
        _frames[name] = region;
    }
    
    public Rectangle? GetFrame(string name)
    {
        if (_frames.TryGetValue(name, out var rect))
        {
            return rect;
        }
        return null;
    }

    /// <summary>
    /// Slice the texture atlas into uniform frames
    /// </summary>
    /// <param name="frameWidth"></param>
    /// <param name="frameHeight"></param>
    public void Slice(int frameWidth, int frameHeight)
    {
        int columns = SourceTexture.Width / frameWidth;
        int rows = SourceTexture.Height / frameHeight;
        _frames.Clear();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = y * columns + x;
                Rectangle frameRect = new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
                _frames.Add(index.ToString(), frameRect);
            }
        }
    }

    /// <summary>
    /// Slice the texture atlas using custom JSON definitions
    /// </summary>
    /// <param name="jsonContent">JSON string defining frame regions</param>
    public void Slice(string jsonContent)
    {
        //TOOD : Implement JSON parsing for custom frame definitions
    }
}