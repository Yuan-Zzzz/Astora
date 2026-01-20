using Astora.Core.Project;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Rendering.RenderPipeline.RenderPass;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    public static GraphicsDeviceManager GDM{ get; private set; }
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
    public static void Initialize(ContentManager content, GraphicsDeviceManager gdm)
    {
        Content = content;
        GDM = gdm;
        CurrentScene = new SceneTree();
        RenderPipeline = new RenderPipeline(GDM.GraphicsDevice);
        ResourceLoader.Initialize(content);
    }

    public static void LoadProjectConfig()
    {
            var configPath = "project.yaml";
            if (!File.Exists(configPath))
            {
                configPath =  Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "project.yaml");
            }
                
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
                
            var yaml = File.ReadAllText(configPath);
            var config = deserializer.Deserialize<GameProjectConfig>(yaml);
                
            if (config != null)
            {
                SetDesignResolution(config);
                SetWindowSize(config.DesignWidth, config.DesignHeight);
                GDM.ApplyChanges();
                Content.RootDirectory = config.ContentRootDirectory; 
            }
  
    }
    
    /// <summary>
    /// Set DesignResolution
    /// </summary>
    public static void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit)
    {
        DesignResolution = new Point(width, height);
        ScalingMode = scalingMode;
        RenderPipeline.UpdateRenderTarget();
    }
    
    /// <summary>
    /// SetDesignResolutionWith GameConfig
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

    public static void SetWindowSize(int width, int height)
    {
        if (GDM == null) return;
        GDM.PreferredBackBufferWidth = width;
        GDM.PreferredBackBufferHeight = height;
        GDM.ApplyChanges();
    }
    
    /// <summary>
    /// Caculate the ScaleMatrix
    /// </summary>
    public static Matrix GetScaleMatrix()
    {
        if (GDM == null)
            return Matrix.Identity;
        
        var viewport = GDM.GraphicsDevice.Viewport;
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
        if (GDM == null)
            return new Viewport();
        
        var viewport = GDM.GraphicsDevice.Viewport;
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
    /// GameLoop Update
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }
    
    public static void Render(GameTime gameTime, Color? clearColor = null)
    { 
        RenderPipeline.Render(CurrentScene, gameTime, clearColor ?? Color.Black);
    }
}
