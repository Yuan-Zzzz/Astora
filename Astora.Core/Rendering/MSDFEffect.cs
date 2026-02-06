using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering;

public class MSDFEffect : Effect
{
    private EffectParameter? _pxRangeParameter;
    private EffectParameter? _textureSizeParameter;
    private EffectParameter? _bgColorParameter;
    private EffectParameter? _fgColorParameter;

    private float _pxRange = 5.0f;
    private Vector2 _textureSize = Vector2.One;
    private Vector4 _bgColor = new Vector4(0, 0, 0, 0);
    private Vector4 _fgColor = new Vector4(1, 1, 1, 1);
    
    private static MSDFEffect _instance;

    public static MSDFEffect Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Astora.Core.Rendering.Shaders.Build.MSDF.fxc";
    
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
            }
    
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
    
            _instance = new MSDFEffect(Engine.GDM.GraphicsDevice, bytes);
            return _instance;
        }
    }

    /// <summary>
    /// Pixel range for MSDF rendering
    /// </summary>
    public float PxRange
    {
        get => _pxRange;
        set
        {
            _pxRange = value;
            _pxRangeParameter?.SetValue(value);
        }
    }

    /// <summary>
    /// Texture size (width, height)
    /// </summary>
    public Vector2 TextureSize
    {
        get => _textureSize;
        set
        {
            _textureSize = value;
            _textureSizeParameter?.SetValue(value);
        }
    }

    /// <summary>
    /// Background color (RGBA)
    /// </summary>
    public Vector4 BackgroundColor
    {
        get => _bgColor;
        set
        {
            _bgColor = value;
            _bgColorParameter?.SetValue(value);
        }
    }

    /// <summary>
    /// Foreground color (RGBA)
    /// </summary>
    public Vector4 ForegroundColor
    {
        get => _fgColor;
        set
        {
            _fgColor = value;
            _fgColorParameter?.SetValue(value);
        }
    }

    /// <summary>
    /// Uses compiled effect code to create MSDFEffect
    /// </summary>
    public MSDFEffect(GraphicsDevice graphicsDevice, byte[] effectCode)
        : base(graphicsDevice, effectCode)
    {
        Initialize();
    }

    /// <summary>
    /// Clone constructor
    /// </summary>
    public MSDFEffect(Effect cloneSource)
        : base(cloneSource)
    {
        Initialize();
    }

    /// <summary>
    /// Initialize effect parameters
    /// </summary>
    private void Initialize()
    {
        _pxRangeParameter = Parameters["pxRange"];
        _textureSizeParameter = Parameters["textureSize"];
        _bgColorParameter = Parameters["bgColor"];
        _fgColorParameter = Parameters["fgColor"];

        if (_pxRangeParameter != null)
            _pxRangeParameter.SetValue(_pxRange);
        if (_textureSizeParameter != null)
            _textureSizeParameter.SetValue(_textureSize);
        if (_bgColorParameter != null)
            _bgColorParameter.SetValue(_bgColor);
        if (_fgColorParameter != null)
            _fgColorParameter.SetValue(_fgColor);
    }

    /// <summary>
    /// Sets texture size based on the provided texture
    /// </summary>
    public void SetTextureSize(Texture2D texture)
    {
        if (texture != null)
        {
            TextureSize = new Vector2(texture.Width, texture.Height);
        }
    }

    /// <summary>
    /// Sets texture size assuming square texture
    /// </summary>
    public void SetTextureSize(int textureHeight)
    {
        TextureSize = new Vector2(textureHeight, textureHeight);
    }
}
