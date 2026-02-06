using Astora.Core;
using Astora.Core.Rendering.RenderPipeline.RenderPass;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline;

public class RenderPipeline
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly RenderBatcher _renderBatcher;
    private RenderTarget2D _mainTarget;
    private List<IRenderPass> _passes;
    private RenderContext _context;
    private Point _designResolution;
    private readonly Func<Matrix>? _getScaleMatrix;

    public RenderPipeline(GraphicsDevice graphicsDevice, Point designResolution, Func<Matrix>? getScaleMatrix = null)
    {
        _graphicsDevice = graphicsDevice;
        _designResolution = designResolution;
        _getScaleMatrix = getScaleMatrix;
        _renderBatcher = new RenderBatcher(graphicsDevice);
        _passes = new List<IRenderPass>();
        _context = new RenderContext
        {
            GraphicsDevice = graphicsDevice,
            RenderBatcher = _renderBatcher
        };
        
        UpdateRenderTarget(_designResolution);
        AddPass(new SceneRenderPass());
        AddPass(new FinalCompositionPass(_getScaleMatrix ?? (() => Engine.GetScaleMatrix())));
    }

    public void AddPass(IRenderPass pass) => _passes.Add(pass);
    public void RemovePass(IRenderPass pass) => _passes.Remove(pass);
    public void ClearPasses() => _passes.Clear();
    
    /// <summary>
    /// Updates the main render target to match the given design resolution.
    /// </summary>
    public void UpdateRenderTarget(Point designResolution)
    {
        _designResolution = designResolution;
        _mainTarget?.Dispose();
        _mainTarget = new RenderTarget2D(
            _graphicsDevice,
            _designResolution.X,
            _designResolution.Y,
            false,
            _graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);
    }

    public void Render(SceneTree scene, GameTime gameTime, Color clearColor)
    {
        _context.CurrentScene = scene;
        _context.ActiveCamera = scene.ActiveCamera;
        _context.GameTime = gameTime;
        _context.SourceBuffer = _mainTarget;
        _graphicsDevice.SetRenderTarget(_mainTarget);
        _graphicsDevice.Clear(clearColor);
        
        for (int i = 0; i < _passes.Count; i++)
        {
            var pass = _passes[i];
            if (!pass.Enabled) continue;
            if (pass is FinalCompositionPass)
            {
                _graphicsDevice.SetRenderTarget(null);
                _graphicsDevice.Clear(Color.Black);
            }
            pass.Execute(_context);
        }
    }
}
