using Astora.Core.Project;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

/// <summary>
/// Default implementation of the engine runtime context. Holds all mutable engine state
/// and provides scale matrix / viewport calculation.
/// </summary>
public class EngineContext : IEngineContext
{
    public ContentManager Content { get; }
    public GraphicsDeviceManager GDM { get; }
    public Point DesignResolution { get; set; }
    public ScalingMode ScalingMode { get; set; }
    public SceneTree CurrentScene { get; set; }
    public ISceneSerializer Serializer { get; set; }
    public RenderPipeline RenderPipeline { get; }
    public Matrix CurrentViewMatrix { get; set; } = Matrix.Identity;
    public Matrix CurrentGlobalTransformMatrix { get; set; } = Matrix.Identity;

    public EngineContext(ContentManager content, GraphicsDeviceManager gdm, INodeFactory? nodeFactory = null)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        GDM = gdm ?? throw new ArgumentNullException(nameof(gdm));
        DesignResolution = new Point(1920, 1080);
        ScalingMode = ScalingMode.Fit;
        Serializer = new YamlSceneSerializer(nodeFactory ?? new NodeTypeRegistry());
        CurrentScene = new SceneTree(Serializer);
        RenderPipeline = new RenderPipeline(gdm.GraphicsDevice, DesignResolution, GetScaleMatrix);
    }

    public Matrix GetScaleMatrix()
    {
        if (GDM?.GraphicsDevice == null)
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
                scaleX = scaleY = Math.Min((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
            case ScalingMode.Fill:
                scaleX = scaleY = Math.Max((float)actualWidth / designWidth, (float)actualHeight / designHeight);
                offsetX = (actualWidth - designWidth * scaleX) * 0.5f;
                offsetY = (actualHeight - designHeight * scaleY) * 0.5f;
                break;
            case ScalingMode.Stretch:
                scaleX = (float)actualWidth / designWidth;
                scaleY = (float)actualHeight / designHeight;
                break;
            case ScalingMode.PixelPerfect:
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

    public Viewport GetScaledViewport()
    {
        if (GDM?.GraphicsDevice == null)
            return new Viewport();

        var viewport = GDM.GraphicsDevice.Viewport;
        var scaleMatrix = GetScaleMatrix();
        var scale = scaleMatrix.M11;

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

    public void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit)
    {
        DesignResolution = new Point(width, height);
        ScalingMode = scalingMode;
        RenderPipeline.UpdateRenderTarget(DesignResolution);
    }

    public void SetDesignResolution(GameProjectConfig config)
    {
        if (config != null)
        {
            DesignResolution = new Point(config.DesignWidth, config.DesignHeight);
            ScalingMode = config.ScalingMode;
            RenderPipeline.UpdateRenderTarget(DesignResolution);
        }
    }

    public void SetWindowSize(int width, int height)
    {
        if (GDM == null) return;
        GDM.PreferredBackBufferWidth = width;
        GDM.PreferredBackBufferHeight = height;
        GDM.ApplyChanges();
    }
}
