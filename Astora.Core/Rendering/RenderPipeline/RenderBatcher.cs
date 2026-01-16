using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline;

public class RenderBatcher
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _device;
    
    private Matrix _currentTransform;
    private BlendState _currentBlendState;
    private SamplerState _currentSamplerState;
    private Effect _currentEffect;
    
    private bool _isBatching;

    public RenderBatcher(GraphicsDevice device)
    {
        _device = device;
        _spriteBatch = new SpriteBatch(_device);
    }
    
    public void Begin(Matrix transformMatrix, SamplerState sampler = null)
    {
        if (_isBatching) End();

        _currentTransform = transformMatrix;
        _currentSamplerState = sampler ?? SamplerState.PointClamp;
        _currentBlendState = BlendState.AlphaBlend; 
        _currentEffect = null;

        // 真正的开启
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            _currentBlendState,
            _currentSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            _currentEffect,
            _currentTransform
        );
        
        _isBatching = true;
    }
    
    public void End()
    {
        if (_isBatching)
        {
            _spriteBatch.End();
            _isBatching = false;
        }
    }
    
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, 
                     Color color, float rotation, Vector2 origin, Vector2 scale, 
                     SpriteEffects effects, float layerDepth, 
                     BlendState blendState = null, Effect effect = null)
    {
        var targetBlend = blendState ?? BlendState.AlphaBlend;
        var targetEffect = effect;
        
        bool stateChanged = (targetBlend != _currentBlendState) || (targetEffect != _currentEffect);

        if (stateChanged)
        {
            FlushAndChangeState(targetBlend, targetEffect);
        }
        
        _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }
    
    private void FlushAndChangeState(BlendState newBlend, Effect newEffect)
    {
        _spriteBatch.End();

        _currentBlendState = newBlend;
        _currentEffect = newEffect;

        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            _currentBlendState,
            _currentSamplerState,
            DepthStencilState.None,
            RasterizerState.CullNone,
            _currentEffect,
            _currentTransform
        );
    }
}
