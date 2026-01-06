using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

public static class Engine
{
    public static ContentManager Content { get; private set; }
    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static SceneTree CurrentScene { get; set; }
    public static ISceneSerializer Serializer { get; set; } = new YamlSceneSerializer();
    
    /// <summary>
    /// 设计分辨率
    /// </summary>
    public static Point DesignResolution { get; private set; } = new Point(1920, 1080);
    
    /// <summary>
    /// 缩放模式
    /// </summary>
    public static ScalingMode ScalingMode { get; private set; } = ScalingMode.Fit;
    
    /// <summary>
    /// 统一初始化引擎
    /// </summary>
    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        Content = content;
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        CurrentScene = new SceneTree();
    }
    
    /// <summary>
    /// 设置设计分辨率和缩放模式
    /// </summary>
    public static void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit)
    {
        DesignResolution = new Point(width, height);
        ScalingMode = scalingMode;
    }
    
    /// <summary>
    /// 从配置设置设计分辨率
    /// </summary>
    public static void SetDesignResolution(GameProjectConfig config)
    {
        if (config != null)
        {
            DesignResolution = new Point(config.DesignWidth, config.DesignHeight);
            ScalingMode = config.ScalingMode;
        }
    }
    
    /// <summary>
    /// 计算缩放矩阵
    /// </summary>
    public static Matrix GetScaleMatrix()
    {
        if (GraphicsDevice == null)
            return Matrix.Identity;
        
        var viewport = GraphicsDevice.Viewport;
        var actualWidth = viewport.Width;
        var actualHeight = viewport.Height;
        var designWidth = DesignResolution.X;
        var designHeight = DesignResolution.Y;
        
        if (designWidth <= 0 || designHeight <= 0)
            return Matrix.Identity;
        
        float scaleX, scaleY;
        float offsetX = 0, offsetY = 0;
        
        switch (ScalingMode)
        {
            case ScalingMode.None:
                scaleX = 1.0f;
                scaleY = 1.0f;
                break;
                
            case ScalingMode.Fit:
                // 保持宽高比，完整显示（可能有黑边）
                scaleX = scaleY = Math.Min((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
                
            case ScalingMode.Fill:
                // 保持宽高比，填满屏幕（可能裁剪）
                scaleX = scaleY = Math.Max((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
                
            case ScalingMode.Stretch:
                // 拉伸填满屏幕
                scaleX = (float)actualWidth / designWidth;
                scaleY = (float)actualHeight / designHeight;
                break;
                
            case ScalingMode.PixelPerfect:
                // 像素完美缩放（整数倍缩放）
                var scale = Math.Min((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                var pixelScale = Math.Max(1, (int)Math.Floor(scale));
                scaleX = scaleY = pixelScale;
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
                
            default:
                scaleX = scaleY = 1.0f;
                break;
        }
        
        return Matrix.CreateScale(scaleX, scaleY, 1.0f) * Matrix.CreateTranslation(offsetX, offsetY, 0);
    }
    
    /// <summary>
    /// 获取缩放后的视口
    /// </summary>
    public static Viewport GetScaledViewport()
    {
        if (GraphicsDevice == null)
            return new Viewport();
        
        var viewport = GraphicsDevice.Viewport;
        var scaleMatrix = GetScaleMatrix();
        var scale = scaleMatrix.M11; // 获取X缩放
        
        return new Viewport
        {
            X = viewport.X,
            Y = viewport.Y,
            Width = (int)(DesignResolution.X * scale),
            Height = (int)(DesignResolution.Y * scale),
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        };
    }
    
    /// <summary>
    /// 便捷的场景加载方法
    /// </summary>
    public static void LoadScene(string scenePath)
    {
        if (CurrentScene == null)
            throw new InvalidOperationException("Engine not initialized. Call Engine.Initialize() first.");
        
        if (Serializer == null)
            throw new InvalidOperationException("Scene serializer not set.");
        
        var scene = Serializer.Load(scenePath);
        CurrentScene.ChangeScene(scene);
    }
    
    /// <summary>
    /// 游戏循环更新方法
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }
    
    /// <summary>
    /// 游戏循环渲染方法
    /// </summary>
    public static void Render(Color? clearColor = null)
    {
        if (GraphicsDevice == null || SpriteBatch == null || CurrentScene == null)
            return;
        
        GraphicsDevice.Clear(clearColor ?? Color.Black);
        
        // 计算变换矩阵：先应用缩放，再应用相机视图
        Matrix scaleMatrix = GetScaleMatrix();
        Matrix viewMatrix = Matrix.Identity;
        if (CurrentScene.ActiveCamera != null)
        {
            viewMatrix = CurrentScene.ActiveCamera.GetViewMatrix();
        }
        else
        {
            return;
        }
        
        // 组合变换矩阵：缩放 * 视图
        Matrix transformMatrix = scaleMatrix * viewMatrix;
        
        SpriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: transformMatrix
        );
        
        CurrentScene.Draw(SpriteBatch);
        
        SpriteBatch.End();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static T Load<T>(string path) where T : class
    {
        if (Content == null)
            throw new InvalidOperationException("Engine not initialized. Call Engine.Initialize() first.");
        return Content.Load<T>(path);
    }
}