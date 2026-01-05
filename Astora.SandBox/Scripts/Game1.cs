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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // 统一初始化引擎
            Engine.Initialize(Content, GraphicsDevice, _spriteBatch);
            
            // 加载项目配置并应用设计分辨率
            LoadAndApplyProjectConfig();
            
            // 加载初始场景
            var initialScene = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Scenes", "NewScene.scene");
            if (File.Exists(initialScene))
            {
                Engine.LoadScene(initialScene);
            }
        }
        
        /// <summary>
        /// 加载并应用项目配置
        /// </summary>
        private void LoadAndApplyProjectConfig()
        {
            try
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
                    System.Console.WriteLine($"已加载设计分辨率: {config.DesignWidth}x{config.DesignHeight}, 缩放模式: {config.ScalingMode}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"加载项目配置失败: {ex.Message}，使用默认设计分辨率");
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