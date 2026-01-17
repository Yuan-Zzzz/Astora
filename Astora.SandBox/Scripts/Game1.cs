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
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            Engine.Initialize(Content, _graphics);
            Engine.LoadProjectConfig();
            Console.WriteLine("Conent目录："+ Content.RootDirectory);
            var rootNode = new Node("Main");
            var xygg = ResourceLoader.Load<Texture2DResource>("Test.png"); 
            var spr = new Sprite("xygg",xygg.Texture);
            rootNode.AddChild(spr);
            spr.TexturePath = xygg.ResourcePath;
            var cam = new Camera2D();
            rootNode.AddChild(cam);
            Engine.CurrentScene.AttachScene(rootNode);
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
