using Astora.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes
{
    public class Sprite : Node2D
    {
        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
        public Color Modulate { get; set; } = Color.White;
        public Effect? Effect { get; set; }
        public Rectangle? Region { get; set; }
        public Vector2 Offset { get; set; } = Vector2.Zero;
        public BlendState BlendState { get; set; } = BlendState.AlphaBlend;

        // Default white texture for sprites without assigned texture
        private static Texture2D? _defaultWhiteTexture;
        private const int DefaultSize = 64;

        public Sprite(string name, Texture2D texture) : base(name)
        {
            Texture = texture;
            //The default origin is the center of the texture
            if (texture != null)
                Origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            else
                Origin = new Vector2(DefaultSize / 2f, DefaultSize / 2f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var transform = GlobalTransform;
            Vector3 pos, scale;
            Quaternion rotQ;
            transform.Decompose(out scale, out rotQ, out pos);
            float rotation = 2.0f * (float)Math.Atan2(rotQ.Z, rotQ.W);
            
            var textureToDraw = Texture;
            if (textureToDraw == null)
            {
                textureToDraw = GetDefaultWhiteTexture(spriteBatch.GraphicsDevice);
            }
            
            Rectangle srcRect = Region ?? new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height);
            
            Engine.SetRenderState(BlendState, Effect, null);

            // 4. 绘制
            spriteBatch.Draw(
                textureToDraw,
                new Vector2(pos.X, pos.Y) + Offset,
                srcRect,
                Modulate,
                rotation,
                Origin,
                new Vector2(scale.X, scale.Y),
                SpriteEffects.None,
                0f
            );
        }

        /// <summary>
        /// Default white texture generator
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        private static Texture2D GetDefaultWhiteTexture(GraphicsDevice graphicsDevice)
        {
            if (_defaultWhiteTexture == null || _defaultWhiteTexture.IsDisposed)
            {
                _defaultWhiteTexture = new Texture2D(graphicsDevice, DefaultSize, DefaultSize);
                var colorData = new Color[DefaultSize * DefaultSize];
                for (int i = 0; i < colorData.Length; i++)
                {
                    colorData[i] = Color.White;
                }

                _defaultWhiteTexture.SetData(colorData);
            }

            return _defaultWhiteTexture;
        }
    }
}