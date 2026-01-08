using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Project;
using Astora.Core.Rendering;
using Astora.Core.Resources;
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
             
     
             TextureAtlas2D atlas = new TextureAtlas2D(ani);
             atlas.Slice(16, 16); 
             
             SpriteFrames frames = new SpriteFrames(ani);
             
             frames.AddAnimation("idle", fps: 8f, loop: true);
             frames.AddFramesFromAtlas("idle", atlas, new[] { "0", "1", "2", "3", "4", "5", "6", "7" });
             AnimatedSprite player = new AnimatedSprite("Player", frames);
             player.Scale = new Vector2(4f, 4f);
             player.Play("idle");
                Engine.CurrentScene.Root.AddChild(player);
            
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
            base.Update(gameTime);
        }

        Texture2D ani;

        protected override void LoadContent()
        {
            base.LoadContent();
            ani = Texture2D.FromFile(GraphicsDevice, "../../../Content/Animated.png");
        }

        protected override void Draw(GameTime gameTime)
        {
           Engine.Render();
            base.Draw(gameTime);
        }
    }
}