using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Project;
using Astora.Core.Rendering;
using Astora.Core.Resources;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace Astora.SandBox.Scripts

{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            Engine.Initialize(Content, _graphics);
            Console.WriteLine("Conent目录："+ Content.RootDirectory);
            Engine.CurrentScene.AttachScene(new Node("Main"));
            var xygg = ResourceLoader.Load<Texture2DResource>("Test.png");
            
            var spr = new Sprite("xygg",xygg.Texture);
            Engine.CurrentScene.Root.AddChild(spr);
            spr.TexturePath = xygg.ResourcePath;
            Engine.CurrentScene.SaveScene("Scenes/test.scene");
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
