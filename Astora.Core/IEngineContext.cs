using Astora.Core.Project;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Scene;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core;

/// <summary>
/// Represents the current engine runtime environment. Allows dependency injection
/// and multiple instances for testing or multi-viewport scenarios.
/// </summary>
public interface IEngineContext
{
    /// <summary>Content manager for loading assets.</summary>
    ContentManager Content { get; }

    /// <summary>Graphics device manager.</summary>
    GraphicsDeviceManager GDM { get; }

    /// <summary>Design resolution (logical size).</summary>
    Point DesignResolution { get; set; }

    /// <summary>Scaling mode for adapting to screen size.</summary>
    ScalingMode ScalingMode { get; set; }

    /// <summary>Current scene tree.</summary>
    SceneTree CurrentScene { get; set; }

    /// <summary>Scene serializer for load/save.</summary>
    ISceneSerializer Serializer { get; set; }

    /// <summary>Rendering pipeline.</summary>
    RenderPipeline RenderPipeline { get; }

    /// <summary>Current view matrix (e.g. from active camera).</summary>
    Matrix CurrentViewMatrix { get; set; }

    /// <summary>Current global transform matrix.</summary>
    Matrix CurrentGlobalTransformMatrix { get; set; }

    /// <summary>Scale matrix for design resolution to viewport.</summary>
    Matrix GetScaleMatrix();

    /// <summary>Viewport scaled to design resolution and scaling mode.</summary>
    Viewport GetScaledViewport();

    /// <summary>Sets design resolution and updates render target.</summary>
    void SetDesignResolution(int width, int height, ScalingMode scalingMode = ScalingMode.Fit);

    /// <summary>Sets design resolution from project config.</summary>
    void SetDesignResolution(GameProjectConfig config);

    /// <summary>Sets window size.</summary>
    void SetWindowSize(int width, int height);
}
