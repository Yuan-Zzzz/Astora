using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Renderer;

public class RenderBatcher
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _device;
    
    private Matrix _currentTransform;
    private BlendState _currentBlendState;
    private SamplerState _currentSamplerState;
    private Effect _currentEffect;
    
    // 标记 Batch 是否处于 Begin 状态
    private bool _isBatching;

    public RenderBatcher(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        _device = device;
        _spriteBatch = spriteBatch;
    }

    /// <summary>
    /// 开始新的一帧或一个新的渲染通道
    /// </summary>
    public void Begin(Matrix transformMatrix, SamplerState sampler = null)
    {
        if (_isBatching) End();

        _currentTransform = transformMatrix;
        _currentSamplerState = sampler ?? SamplerState.PointClamp; // 默认像素风
        _currentBlendState = BlendState.AlphaBlend; // 默认混合
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

    /// <summary>
    /// 结束当前的绘制
    /// </summary>
    public void End()
    {
        if (_isBatching)
        {
            _spriteBatch.End();
            _isBatching = false;
        }
    }

    /// <summary>
    /// 核心方法：带状态的绘制
    /// </summary>
    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, 
                     Color color, float rotation, Vector2 origin, Vector2 scale, 
                     SpriteEffects effects, float layerDepth, 
                     BlendState blendState = null, Effect effect = null)
    {
        // 1. 确定目标状态（如果是 null，就用默认的 AlphaBlend）
        var targetBlend = blendState ?? BlendState.AlphaBlend;
        var targetEffect = effect; // null 也是一种状态

        // 2. 检查状态是否发生变化
        bool stateChanged = (targetBlend != _currentBlendState) || (targetEffect != _currentEffect);

        if (stateChanged)
        {
            // 3. 如果变了，打断当前 Batch，应用新状态重启
            FlushAndChangeState(targetBlend, targetEffect);
        }

        // 4. 执行绘制
        _spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
    }

    // 处理状态切换的私有方法
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
            _currentTransform // 保持摄像机矩阵不变
        );
    }
}