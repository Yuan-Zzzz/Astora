using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Project;
using Astora.Core.Rendering;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;

namespace Astora.SandBox.Scripts
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Engine.Initialize(Content, _graphics);
            Engine.LoadProjectConfig();
            var sceneRoot = SceneBootstrap.LoadOrBuildScene();
            Engine.CurrentScene.AttachScene(sceneRoot);
            SandboxUIDemo.BuildDemoUI(Engine.CurrentScene.Root);
        }
        
        protected override void Update(GameTime gameTime)
        {
            Engine.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
           Engine.Render(gameTime);
            base.Draw(gameTime);
        }
    }
}
