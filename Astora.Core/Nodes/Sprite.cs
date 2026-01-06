using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes
{
    public class Sprite : Node2D
    {
        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
        public Color Modulate { get; set; } = Color.White;
        
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

            // 分解矩阵
            transform.Decompose(out scale, out rotQ, out pos);

            // 将四元数转换为 Z 轴旋转 (弧度)
            // 这是一个简化的转换，假设我们只在 2D 平面旋转
            float rotation = 2.0f * (float)System.Math.Atan2(rotQ.Z, rotQ.W);

            // 如果没有纹理，使用默认的64x64白色矩形
            var textureToDraw = Texture;
            if (textureToDraw == null)
            {
                textureToDraw = GetDefaultWhiteTexture(spriteBatch.GraphicsDevice);
            }

            spriteBatch.Draw(
                textureToDraw,
                new Vector2(pos.X, pos.Y),
                null,
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