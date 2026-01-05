using Astora.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.SandBox.Scripts
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // 统一初始化引擎
            Engine.Initialize(Content, GraphicsDevice, _spriteBatch);
            
            // 加载初始场景
            var initialScene = "Scenes/Root.scene";
            if (File.Exists(initialScene))
            {
                Engine.LoadScene(initialScene);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Engine.Render();
            base.Draw(gameTime);
        }
    }
}