using Astora.Core;
using Astora.Core.Attributes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes
{
    public class Sprite : Node2D
    {
        [SerializeField]
        [ContentRelativePath]
        private string _texturePath = "";
        
        [SerializeField]
        private Vector2 _origin;
        
        [SerializeField]
        private Color _modulate = Color.White;
        
        [SerializeField]
        private Vector2 _offset = Vector2.Zero;
        
        [SerializeField]
        private Rectangle? _region;
        
        // Non-serialized runtime fields
        private Texture2D _texture;
        private Effect? _effect;
        private BlendState _blendState = BlendState.AlphaBlend;
        
        public Texture2D Texture 
        { 
            get => _texture; 
            set => _texture = value; 
        }
        
        public Vector2 Origin 
        { 
            get => _origin; 
            set => _origin = value; 
        }
        
        public Color Modulate 
        { 
            get => _modulate; 
            set => _modulate = value; 
        }
        
        public Effect? Effect 
        { 
            get => _effect; 
            set => _effect = value; 
        }
        
        public Rectangle? Region 
        { 
            get => _region; 
            set => _region = value; 
        }
        
        public Vector2 Offset 
        { 
            get => _offset; 
            set => _offset = value; 
        }
        
        public BlendState BlendState 
        { 
            get => _blendState; 
            set => _blendState = value; 
        }
        
        public string TexturePath
        {
            get => _texturePath;
            set => _texturePath = value;
        }

        // Default white texture for sprites without assigned texture
        private static Texture2D? _defaultWhiteTexture;
        private const int DefaultSize = 64;

        public Sprite() : base()
        {
            _origin = new Vector2(DefaultSize / 2f, DefaultSize / 2f);
        }

        public Sprite(string name, Texture2D texture) : base(name)
        {
            _texture = texture;
            //The default origin is the center of the texture
            if (texture != null)
                _origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            else
                _origin = new Vector2(DefaultSize / 2f, DefaultSize / 2f);
        }

        public override void Draw(RenderBatcher renderBatcher)
        {
            var transform = GlobalTransform;
            Vector3 pos, scale;
            Quaternion rotQ;
            transform.Decompose(out scale, out rotQ, out pos);
            float rotation = 2.0f * (float)Math.Atan2(rotQ.Z, rotQ.W);
            
            var textureToDraw = Texture;
            if (textureToDraw == null)
            {
                textureToDraw = GetDefaultWhiteTexture(Engine.GDM.GraphicsDevice);
            }
            
            Rectangle srcRect = Region ?? new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height);

            // 4. 绘制
            renderBatcher.Draw(
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
