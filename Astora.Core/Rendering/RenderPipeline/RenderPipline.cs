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

    public RenderPipeline(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _renderBatcher = new RenderBatcher(graphicsDevice);
        _passes = new List<IRenderPass>();
        _context = new RenderContext
        {
            GraphicsDevice = graphicsDevice,
            RenderBatcher = _renderBatcher
        };
        
        UpdateRenderTarget();
        AddPass(new SceneRenderPass());
        AddPass(new FinalCompositionPass());
    }

    public void AddPass(IRenderPass pass) => _passes.Add(pass);
    public void RemovePass(IRenderPass pass) => _passes.Remove(pass);
    public void ClearPasses() => _passes.Clear();
    
    public void UpdateRenderTarget()
    {
        _mainTarget?.Dispose();
        _mainTarget = new RenderTarget2D(
            _graphicsDevice,
            Engine.DesignResolution.X,
            Engine.DesignResolution.Y,
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
