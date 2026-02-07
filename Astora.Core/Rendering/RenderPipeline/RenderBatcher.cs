using System.Collections.Generic;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Rendering.RenderPipeline;

public class RenderBatcher : IRenderBatcher
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _device;
    
    private Matrix _currentTransform;
    private BlendState _currentBlendState;
    private SamplerState _currentSamplerState;
    private Effect _currentEffect;
    private RasterizerState _currentRasterizer;
    
    private bool _isBatching;
    private readonly Stack<Rectangle> _scissorStack = new Stack<Rectangle>();

    private static readonly RasterizerState ScissorEnabled = new RasterizerState
    {
        ScissorTestEnable = true,
        CullMode = CullMode.None
    };

    public RenderBatcher(GraphicsDevice device)
    {
        _device = device;
        _spriteBatch = new SpriteBatch(_device);
    }
    
    public void Begin(Matrix transformMatrix, SamplerState? sampler = null)
    {
        if (_isBatching) End();

        _currentTransform = transformMatrix;
        _currentSamplerState = sampler ?? SamplerState.PointClamp;
        _currentBlendState = BlendState.AlphaBlend;
        _currentEffect = null;
        _currentRasterizer = RasterizerState.CullNone;

        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            _currentBlendState,
            _currentSamplerState,
            DepthStencilState.None,
            _currentRasterizer,
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
        BlendState? blendState = null, Effect? effect = null)
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

    public void DrawString(SpriteFontBase font, string text, Vector2 position, Color color)
    {
        if (font == null || string.IsNullOrEmpty(text)) return;
        _spriteBatch.DrawString(font, text, position, color);
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
            _currentRasterizer,
            _currentEffect,
            _currentTransform
        );
    }

    public void PushScissorRect(Rectangle rect)
    {
        var vp = _device.Viewport.Bounds;
        var current = _scissorStack.Count > 0 ? _scissorStack.Peek() : new Rectangle(vp.X, vp.Y, vp.Width, vp.Height);
        var intersected = Rectangle.Intersect(current, rect);
        if (intersected.Width <= 0 || intersected.Height <= 0)
            intersected = new Rectangle(rect.X, rect.Y, 0, 0);
        _scissorStack.Push(intersected);
        ApplyScissorAndRestartBatch(ScissorEnabled, intersected);
    }

    public void PopScissorRect()
    {
        if (_scissorStack.Count == 0) return;
        _scissorStack.Pop();
        if (_scissorStack.Count == 0)
        {
            _spriteBatch.End();
            _isBatching = false;
            _currentRasterizer = RasterizerState.CullNone;
            _device.ScissorRectangle = _device.Viewport.Bounds;
            _spriteBatch.Begin(SpriteSortMode.Deferred, _currentBlendState, _currentSamplerState,
                DepthStencilState.None, _currentRasterizer, _currentEffect, _currentTransform);
            _isBatching = true;
        }
        else
            ApplyScissorAndRestartBatch(ScissorEnabled, _scissorStack.Peek());
    }

    private void ApplyScissorAndRestartBatch(RasterizerState rasterizer, Rectangle scissorRect)
    {
        _spriteBatch.End();
        _isBatching = false;
        _currentRasterizer = rasterizer;
        _device.ScissorRectangle = rasterizer.ScissorTestEnable ? scissorRect : _device.Viewport.Bounds;
        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            _currentBlendState,
            _currentSamplerState,
            DepthStencilState.None,
            _currentRasterizer,
            _currentEffect,
            _currentTransform
        );
        _isBatching = true;
    }
}
