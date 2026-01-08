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
        
        /// <summary>
        /// 可选的渲染效果。如果设置，Sprite将使用此Effect进行渲染。
        /// 注意：使用Effect时，Sprite会在独立的SpriteBatch批次中绘制。
        /// </summary>
        public Effect? Effect { get; set; }
        
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

            // 如果Sprite有Effect，使用独立的SpriteBatch批次进行绘制
            if (Effect != null)
            {
                // 计算变换矩阵（缩放矩阵 * 视图矩阵）
                Matrix scaleMatrix = Engine.GetScaleMatrix();
                Matrix viewMatrix = Matrix.Identity;
                if (Engine.CurrentScene?.ActiveCamera != null)
                {
                    viewMatrix = Engine.CurrentScene.ActiveCamera.GetViewMatrix();
                }
                Matrix transformMatrix = scaleMatrix * viewMatrix;

                // 保存当前批次状态：先结束当前批次（如果已经开始）
                // 注意：MonoGame的SpriteBatch不允许嵌套Begin()，所以我们需要先End
                // 由于MonoGame没有提供检查SpriteBatch状态的方法，我们使用try-catch来处理
                bool wasBatchActive = false;
                try
                {
                    // 尝试结束当前批次（如果已经开始）
                    spriteBatch.End();
                    wasBatchActive = true;
                }
                catch (InvalidOperationException)
                {
                    // 如果批次未开始，InvalidOperationException会被抛出
                    // 这是正常情况，说明SpriteBatch未开始，我们不需要恢复
                    wasBatchActive = false;
                }

                // 使用Effect开始新的批次
                spriteBatch.Begin(
                    effect: Effect,
                    samplerState: SamplerState.PointClamp,
                    transformMatrix: transformMatrix
                );

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

                spriteBatch.End();

                // 如果之前有批次在运行，重新开始原来的批次
                if (wasBatchActive)
                {
                    spriteBatch.Begin(
                        samplerState: SamplerState.PointClamp,
                        transformMatrix: transformMatrix
                    );
                }
            }
            else
            {
                // 没有Effect时，使用原有的绘制方式（在当前的SpriteBatch批次中绘制）
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