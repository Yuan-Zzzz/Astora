using Astora.Core.Project;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Astora.Core.Tweener;
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
    /// Current engine context. Set by <see cref="Initialize"/>.
    /// </summary>
    public static IEngineContext CurrentContext { get; private set; }

    /// <summary>
    /// Content Manager. Null when engine is not initialized.
    /// </summary>
    public static ContentManager Content => CurrentContext?.Content;

    /// <summary>
    /// Graphics Device. Null when engine is not initialized.
    /// </summary>
    public static GraphicsDeviceManager GDM => CurrentContext?.GDM;

    /// <summary>
    /// Current Scene Tree
    /// </summary>
    public static SceneTree CurrentScene
    {
        get => CurrentContext?.CurrentScene;
        set { if (CurrentContext != null) CurrentContext.CurrentScene = value; }
    }

    /// <summary>
    /// Scene Serializer
    /// </summary>
    public static ISceneSerializer Serializer
    {
        get => CurrentContext?.Serializer ?? new YamlSceneSerializer(new NodeTypeRegistry());
        set { if (CurrentContext != null) CurrentContext.Serializer = value; }
    }

    /// <summary>
    /// Design Resolution
    /// </summary>
    public static Point DesignResolution => CurrentContext?.DesignResolution ?? new Point(1920, 1080);

    /// <summary>
    /// Scaling Mode
    /// </summary>
    public static ScalingMode ScalingMode => CurrentContext?.ScalingMode ?? ScalingMode.Fit;

    /// <summary>
    /// Current View Matrix
    /// </summary>
    public static Matrix CurrentViewMatrix
    {
        get => CurrentContext?.CurrentViewMatrix ?? Matrix.Identity;
        set { if (CurrentContext != null) CurrentContext.CurrentViewMatrix = value; }
    }

    /// <summary>
    /// Current Global Transform Matrix
    /// </summary>
    public static Matrix CurrentGlobalTransformMatrix
    {
        get => CurrentContext?.CurrentGlobalTransformMatrix ?? Matrix.Identity;
        set { if (CurrentContext != null) CurrentContext.CurrentGlobalTransformMatrix = value; }
    }

    public static RenderPipeline RenderPipeline => CurrentContext?.RenderPipeline;

    /// <summary>
    /// Initialize the Engine. Optionally pass a custom node factory (e.g. editor's NodeTypeRegistry with priority assembly).
    /// </summary>
    public static void Initialize(ContentManager content, GraphicsDeviceManager gdm, INodeFactory? nodeFactory = null)
    {
        CurrentContext = new EngineContext(content, gdm, nodeFactory);
        ResourceLoader.Initialize(content);
    }

    /// <summary>
    /// Load project.yaml and apply design resolution, window size, and content root. Returns the config for use by IGameRuntime, or null if not found.
    /// </summary>
    public static GameProjectConfig? LoadProjectConfig()
    {
        var configPath = "project.yaml";
        if (!File.Exists(configPath))
        {
            configPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "project.yaml");
        }

        if (!File.Exists(configPath))
            return null;

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

        return config;
    }

    /// <summary>
    /// Set DesignResolution
    /// </summary>
    public static void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit)
    {
        CurrentContext?.SetDesignResolution(width, height, scalingMode);
    }

    /// <summary>
    /// SetDesignResolutionWith GameConfig
    /// </summary>
    public static void SetDesignResolution(GameProjectConfig config)
    {
        CurrentContext?.SetDesignResolution(config);
    }

    public static void SetWindowSize(int width, int height)
    {
        CurrentContext?.SetWindowSize(width, height);
    }

    /// <summary>
    /// Calculate the ScaleMatrix
    /// </summary>
    public static Matrix GetScaleMatrix()
    {
        return CurrentContext?.GetScaleMatrix() ?? Matrix.Identity;
    }

    /// <summary>
    /// Return the scaled viewport according to the design resolution and scaling mode
    /// </summary>
    public static Viewport GetScaledViewport()
    {
        if (CurrentContext == null)
            return new Viewport();
        return CurrentContext.GetScaledViewport();
    }

    /// <summary>
    /// GameLoop Update
    /// </summary>
    public static void Update(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        TweenCore.Update(delta);
        CurrentScene?.Update(gameTime);
    }

    public static void Render(GameTime gameTime, Color? clearColor = null)
    {
        RenderPipeline.Render(CurrentScene, gameTime, clearColor ?? Color.Black);
    }
}
