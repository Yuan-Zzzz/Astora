using Astora.Core;
using Astora.Core.Project;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

        protected override void Initialize()
        {
            base.Initialize();
            
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.Initialize(Content, GraphicsDevice, _spriteBatch);
            LoadAndApplyProjectConfig();
            
             var initialScene = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Scenes", "NewScene.scene");
             if (File.Exists(initialScene))
             {
                 Console.WriteLine($"加载初始场景: {initialScene}");
                 Engine.LoadScene(initialScene);
             }
            
        }
        
        /// <summary>
        /// 加载并应用项目配置
        /// </summary>
        private void LoadAndApplyProjectConfig()
        {
                // 尝试从当前目录或上级目录查找 project.yaml
                var configPath = "project.yaml";
                if (!File.Exists(configPath))
                {
                    // 尝试在上级目录查找（适用于从 bin/Debug/net8.0 运行的情况）
                    configPath =  Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "project.yaml");
                    if (!File.Exists(configPath))
                    {
                        // 使用默认配置
                        System.Console.WriteLine("未找到 project.yaml，使用默认设计分辨率");
                        return;
                    }
                }
                
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var yaml = File.ReadAllText(configPath);
                var config = deserializer.Deserialize<GameProjectConfig>(yaml);
                
                if (config != null)
                {
                    Engine.SetDesignResolution(config);
                    _graphics.PreferredBackBufferWidth = config.DesignWidth;
                    _graphics.PreferredBackBufferHeight = config.DesignHeight;
                    _graphics.ApplyChanges();
                }
        }

        protected override void Update(GameTime gameTime)
        {
            Engine.Update(gameTime);
            
            //根据按键调整Scale
            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                scale += new Vector2(1, 1);
            }
            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                scale -= new Vector2(1, 1);
            }
            base.Update(gameTime);
        }

        Texture2D msdfTex;
        Effect MSDFshader;
        Vector2 scale = new Vector2(1, 1) * 100;

        protected override void LoadContent()
        {
            base.LoadContent();
            var bytes = File.ReadAllBytes("../../../Content/Shaders/MSDF.xnb");
            MSDFshader = new Effect(this.GraphicsDevice, bytes);
            
            msdfTex = Texture2D.FromFile(GraphicsDevice, "../../../Content/MSDFTEST.png");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            var screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height / 2f);
            
           // Engine.Render();
            _spriteBatch.Begin(effect:MSDFshader);
            
            MSDFshader.Parameters["pxRange"].SetValue(5);                             //best value is 5.
            MSDFshader.Parameters["textureSize"].SetValue(msdfTex.Height);                 //MSDF shader needs texture size.

            //MSDF shader will lerping with this colors.
            MSDFshader.Parameters["bgColor"].SetValue(new Vector4(0, 0, 0, 0));
            MSDFshader.Parameters["fgColor"].SetValue(new Vector4(0, 0.5f, 0, 1));

            _spriteBatch.Draw(msdfTex,
                new Rectangle((int)(screenCenter.X - scale.X / 2),
                    (int)(screenCenter.Y - scale.Y / 2),
                    (int)scale.X,
                    (int)scale.Y),
                Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}