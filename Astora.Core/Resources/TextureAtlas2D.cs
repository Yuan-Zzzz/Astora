using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Resources;

public class TextureAtlas2D
{
    public Texture2D SourceTexture { get; private set; }

    private Rectangle[] _frames;

    public TextureAtlas2D(Texture2D sourceTexture)
    {
        SourceTexture = sourceTexture;
        _frames = Array.Empty<Rectangle>();
    }

    public void Slice(int frameWidth, int frameHeight)
    {
        int columns = SourceTexture.Width / frameWidth;
        int rows = SourceTexture.Height / frameHeight;
        _frames = new Rectangle[columns * rows];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                _frames[y * columns + x] = new Rectangle(x * frameWidth, y * frameHeight, frameWidth, frameHeight);
            }
        }
    }

    public Rectangle GetFrame(int index)
    {
        if (index < 0 || index >= _frames.Length)
            throw new IndexOutOfRangeException("Frame index out of range.");

        return _frames[index];
    }
}