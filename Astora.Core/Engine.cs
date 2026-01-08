using Astora.Core.Project;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

public static class Engine
{
    public static SamplerState DefaultSamplerState { get; } = SamplerState.PointClamp;
    public static ContentManager Content { get; private set; }
    public static GraphicsDevice GraphicsDevice { get; private set; }
    public static SpriteBatch SpriteBatch { get; private set; }
    public static SceneTree CurrentScene { get; set; }
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
    
    private static BlendState _currentBlendState = BlendState.AlphaBlend;
    private static Effect _currentEffect = null;
    private static Matrix _currentTransformMatrix = Matrix.Identity;
    
    /// <summary>
    /// Initialize the Engine
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
    /// 游戏循环更新方法
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }
    
    public static void Render(Color? clearColor = null)
    {
        if (GraphicsDevice == null || SpriteBatch == null || CurrentScene == null)
            return;
        
        GraphicsDevice.Clear(clearColor ?? Color.Black);
        
        Matrix scaleMatrix = GetScaleMatrix();
        CurrentViewMatrix = Matrix.Identity;
        
        if (CurrentScene.ActiveCamera != null)
        {
            CurrentViewMatrix = CurrentScene.ActiveCamera.GetViewMatrix();
        }
        
        CurrentGlobalTransformMatrix = scaleMatrix * CurrentViewMatrix;
        
        _currentBlendState = BlendState.AlphaBlend;
        _currentEffect = null;
        _currentTransformMatrix = CurrentGlobalTransformMatrix;
       
        // Begin SpriteBatch with proper settings
        SpriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred, 
            blendState: BlendState.AlphaBlend, 
            samplerState: DefaultSamplerState,
            depthStencilState: null, 
            rasterizerState: null, 
            effect: null, 
            transformMatrix: CurrentGlobalTransformMatrix
        );
        
        CurrentScene.Draw(SpriteBatch);
        
        SpriteBatch.End();
    }
    
    public static void SetRenderState(BlendState blendState = null, Effect effect = null, Matrix? transformMatrix = null)
    {
        var targetBlend = blendState ?? BlendState.AlphaBlend;
        var targetEffect = effect;
        var targetMatrix = transformMatrix ?? CurrentGlobalTransformMatrix;
        
        bool stateChanged = false;

        if (_currentBlendState != targetBlend) stateChanged = true;
        else if (_currentEffect != targetEffect) stateChanged = true;
        else if (_currentTransformMatrix != targetMatrix) stateChanged = true;
        
        if (!stateChanged) return;
        
        SpriteBatch.End();

        SpriteBatch.Begin(
            sortMode: SpriteSortMode.Deferred,
            blendState: targetBlend,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: targetEffect,
            transformMatrix: targetMatrix
        );
        
        _currentBlendState = targetBlend;
        _currentEffect = targetEffect;
        _currentTransformMatrix = targetMatrix;
    }
}