using Astora.Core.Project;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Rendering.RenderPipeline.RenderPass;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

public static class Engine
{
    /// <summary>
    /// Default Sampler State
    /// </summary>
    public static SamplerState DefaultSamplerState { get; } = SamplerState.PointClamp;
    
    /// <summary>
    /// Content Manager
    /// </summary>
    public static ContentManager Content { get; private set; }
    /// <summary>
    /// Graphics Device
    /// </summary>
    public static GraphicsDevice GraphicsDevice { get; private set; }
    /// <summary>
    /// Default Sprite Batch
    /// </summary>
    public static SpriteBatch SpriteBatch { get; private set; }
    /// <summary>
    /// Current Scene Tree
    /// </summary>
    public static SceneTree CurrentScene { get; set; }
    /// <summary>
    /// Scene Serializer
    /// </summary>
    public static ISceneSerializer Serializer { get; set; } = new YamlSceneSerializer();
    
    /// <summary>
    /// Design Resolution
    /// </summary>
    public static Point DesignResolution { get; private set; } = new Point(1920, 1080);
    
    /// <summary>
    /// Scaling Mode
    /// </summary>
    public static ScalingMode ScalingMode { get; private set; } = ScalingMode.Fit;
    
    /// <summary>
    /// Current View Matrix
    /// </summary>
    public static Matrix CurrentViewMatrix { get; private set; } = Matrix.Identity;
    
    /// <summary>
    /// Current Global Transform Matrix
    /// </summary>
    public static Matrix CurrentGlobalTransformMatrix { get; private set; } = Matrix.Identity;
    
    public static RenderPipeline RenderPipeline { get; private set; }
    
    
    /// <summary>
    /// Initialize the Engine
    /// </summary>
    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        Content = content;
        GraphicsDevice = graphicsDevice;
        SpriteBatch = spriteBatch;
        CurrentScene = new SceneTree();
        RenderPipeline = new RenderPipeline(GraphicsDevice, SpriteBatch);
    }
    
    /// <summary>
    /// 设置设计分辨率和缩放模式
    /// </summary>
    public static void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit)
    {
        DesignResolution = new Point(width, height);
        ScalingMode = scalingMode;
        RenderPipeline.UpdateRenderTarget();
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
            RenderPipeline.UpdateRenderTarget();
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
                // Hold aspect ratio, fit within the screen
                scaleX = scaleY = Math.Min((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
                
            case ScalingMode.Fill:
                // Hold aspect ratio, fill the screen (may crop)
                scaleX = scaleY = Math.Max((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
                
            case ScalingMode.Stretch:
                // Stretch to fill the screen (ignore aspect ratio)
                scaleX = (float)actualWidth / designWidth;
                scaleY = (float)actualHeight / designHeight;
                break;
                
            case ScalingMode.PixelPerfect:
                // Scale by integer multiples only
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
    /// return the scaled viewport according to the design resolution and scaling mode
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
    /// Load scene from file
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
    /// GameLoop Update
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }
    
    public static void Render(GameTime gameTime, Color? clearColor = null)
    {
        if (GraphicsDevice == null || SpriteBatch == null || CurrentScene == null)
            return;
        
        RenderPipeline.Render(CurrentScene, gameTime, clearColor ?? Color.Black);
    }
}