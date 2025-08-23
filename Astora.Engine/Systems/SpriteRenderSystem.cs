using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Astora.Core;
using Astora.Core.Time;
using Astora.ECS;
using Astora.Engine.Components;
using Astora.Engine.Core;
using Astora.Engine.Graphics;

namespace Astora.Engine.Systems;

public struct SpriteRenderSystem : IRenderSystem
{
    private readonly Astora.ECS.World _world;
    private readonly SpriteBatch _sb;
    public int Order => 600;

    public SpriteRenderSystem(Astora.ECS.World world, GraphicsDevice gd)
    {
        _world = world;
        _sb = new SpriteBatch(gd);
    }

    public void TickRender(ITime t)
    {
        Camera2DComponent? camOpt = null;
        foreach (var e in _world.Query<Camera2DComponent>())
        {
            ref var c = ref _world.GetComponent<Camera2DComponent>(e);
            if (c.Primary) { camOpt = c; break; }
        }
        if (camOpt is null) return;
        var cam = camOpt.Value;

        RenderLayerPass(RenderLayer.Background, cam);
        RenderLayerPass(RenderLayer.World,      cam);
        RenderLayerPass(RenderLayer.Foreground, cam);
    }

    private void RenderLayerPass(RenderLayer layer, Camera2DComponent cam)
    {
        _sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp,
                  depthStencilState: null, rasterizerState: null, effect: null, transformMatrix: cam.View);

        foreach (var e in _world.Query<SpriteRendererComponent, Transform2D>())
        {
            ref var sr = ref _world.GetComponent<SpriteRendererComponent>(e);
            if (sr.Layer != layer || sr.Texture is null) continue;

            ref var tr = ref _world.GetComponent<Transform2D>(e);

            _sb.Draw(
                texture: sr.Texture,
                position: new Vector2(tr.WorldPosition.X, tr.WorldPosition.Y),
                sourceRectangle: sr.SourceRect,
                color: sr.Color,
                rotation: tr.WorldRotation,
                origin: sr.Origin,
                scale: new Vector2(tr.WorldScale.X, tr.WorldScale.Y),
                effects: sr.Effects,
                layerDepth: sr.SortKey
            );
        }

        _sb.End();
    }
}
