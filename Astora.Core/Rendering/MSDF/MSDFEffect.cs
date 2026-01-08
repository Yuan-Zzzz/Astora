using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.MSDF;

/// <summary>
/// MSDF (Multi-channel Signed Distance Field) 渲染效果
/// 提供MSDF shader的加载和参数管理
/// </summary>
public static class MSDFEffect
{
    private static Effect? _effect;
    private static GraphicsDevice? _graphicsDevice;
    
    private static float _pixelRange = 5.0f;
    private static Vector2 _textureSize = Vector2.One;
    private static Vector4 _bgColor = new Vector4(0, 0, 0, 0);
    private static Vector4 _fgColor = new Vector4(1, 1, 1, 1);
    
    /// <summary>
    /// 初始化MSDF Effect，从嵌入资源加载shader
    /// </summary>
    /// <param name="graphicsDevice">图形设备</param>
    /// <exception cref="InvalidOperationException">如果shader资源未找到</exception>
    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        if (_effect != null && !_effect.IsDisposed)
        {
            return; // 已经初始化
        }
        
        _graphicsDevice = graphicsDevice;
        
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Astora.Core.Shaders.MSDF.xnb";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"无法找到嵌入资源: {resourceName}");
        }
        
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, (int)stream.Length);
        
        _effect = new Effect(graphicsDevice, bytes);
        
        // 应用默认参数
        ApplyParameters();
    }
    
    /// <summary>
    /// 获取Effect实例，用于SpriteBatch.Begin
    /// </summary>
    /// <returns>Effect实例</returns>
    /// <exception cref="InvalidOperationException">如果Effect未初始化</exception>
    public static Effect GetEffect()
    {
        if (_effect == null || _effect.IsDisposed)
        {
            throw new InvalidOperationException("MSDFEffect未初始化。请先调用Initialize()方法。");
        }
        
        return _effect;
    }
    
    /// <summary>
    /// 设置像素范围（推荐值：5）
    /// </summary>
    /// <param name="pxRange">像素范围</param>
    public static void SetPixelRange(float pxRange)
    {
        _pixelRange = pxRange;
        if (_effect != null && !_effect.IsDisposed)
        {
            _effect.Parameters["pxRange"]?.SetValue(pxRange);
        }
    }
    
    /// <summary>
    /// 设置纹理尺寸
    /// </summary>
    /// <param name="textureSize">纹理尺寸（Vector2）</param>
    public static void SetTextureSize(Vector2 textureSize)
    {
        _textureSize = textureSize;
        if (_effect != null && !_effect.IsDisposed)
        {
            _effect.Parameters["textureSize"]?.SetValue(textureSize);
        }
    }
    
    /// <summary>
    /// 设置纹理尺寸（使用单个值，假设为正方形纹理）
    /// </summary>
    /// <param name="size">纹理尺寸</param>
    public static void SetTextureSize(float size)
    {
        SetTextureSize(new Vector2(size, size));
    }
    
    /// <summary>
    /// 设置背景色（用于lerp）
    /// </summary>
    /// <param name="bgColor">背景色（RGBA）</param>
    public static void SetBackgroundColor(Vector4 bgColor)
    {
        _bgColor = bgColor;
        if (_effect != null && !_effect.IsDisposed)
        {
            _effect.Parameters["bgColor"]?.SetValue(bgColor);
        }
    }
    
    /// <summary>
    /// 设置背景色（使用Color）
    /// </summary>
    /// <param name="color">背景色</param>
    public static void SetBackgroundColor(Color color)
    {
        SetBackgroundColor(color.ToVector4());
    }
    
    /// <summary>
    /// 设置前景色（用于lerp）
    /// </summary>
    /// <param name="fgColor">前景色（RGBA）</param>
    public static void SetForegroundColor(Vector4 fgColor)
    {
        _fgColor = fgColor;
        if (_effect != null && !_effect.IsDisposed)
        {
            _effect.Parameters["fgColor"]?.SetValue(fgColor);
        }
    }
    
    /// <summary>
    /// 设置前景色（使用Color）
    /// </summary>
    /// <param name="color">前景色</param>
    public static void SetForegroundColor(Color color)
    {
        SetForegroundColor(color.ToVector4());
    }
    
    /// <summary>
    /// 应用所有参数到Effect
    /// </summary>
    private static void ApplyParameters()
    {
        if (_effect == null || _effect.IsDisposed)
            return;
        
        _effect.Parameters["pxRange"]?.SetValue(_pixelRange);
        _effect.Parameters["textureSize"]?.SetValue(_textureSize);
        _effect.Parameters["bgColor"]?.SetValue(_bgColor);
        _effect.Parameters["fgColor"]?.SetValue(_fgColor);
    }
    
    /// <summary>
    /// 检查Effect是否已初始化
    /// </summary>
    public static bool IsInitialized => _effect != null && !_effect.IsDisposed;
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public static void Dispose()
    {
        if (_effect != null && !_effect.IsDisposed)
        {
            _effect.Dispose();
            _effect = null;
        }
        _graphicsDevice = null;
    }
}

