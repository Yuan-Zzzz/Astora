using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes
{
    public class Sprite : Node2D
    {
        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
        public Color Modulate { get; set; } = Color.White;

        public Sprite(string name, Texture2D texture) : base(name)
        {
            Texture = texture;
            //The default origin is the center of the texture
            if (texture != null)
                Origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null) return;
            
            var transform = GlobalTransform;

            Vector3 pos, scale;
            Quaternion rotQ;

            // 分解矩阵
            transform.Decompose(out scale, out rotQ, out pos);

            // 将四元数转换为 Z 轴旋转 (弧度)
            // 这是一个简化的转换，假设我们只在 2D 平面旋转
            float rotation = 2.0f * (float)System.Math.Atan2(rotQ.Z, rotQ.W);

            spriteBatch.Draw(
                Texture,
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
    }
}