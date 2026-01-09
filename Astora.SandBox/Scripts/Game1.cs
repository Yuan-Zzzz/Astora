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
             
             var magicVortex = new CPUParticles2D("MagicVortex", 500);
           //  magicVortex.Texture = Content.Load<Texture2D>("star_particle");
             magicVortex.Position = new Vector2(400, 300);

// 1. 设置发射形状为圆形 (半径 100)
             magicVortex.EmissionShape = CPUParticles2D.ParticleEmissionShape.Sphere;
             magicVortex.EmissionBoxExtents = new Vector2(100, 0); // Y 在 Sphere 模式下忽略

// 2. 设置物理：无重力，无初速度，纯靠切线加速度旋转
             magicVortex.Gravity = Vector2.Zero;
             magicVortex.InitialVelocityMin = 0f;
             magicVortex.InitialVelocityMax = 0f;
             magicVortex.TangentialAccel = 150f; // 正值逆时针旋转，负值顺时针
             
             magicVortex.ScaleStart = 1.5f; // 曲线最大值时的倍率

// 4. 颜色
             magicVortex.ColorStart = Color.Cyan;
             magicVortex.ColorEnd = Color.Transparent;
             magicVortex.Lifetime = 2.0f;

             Engine.CurrentScene.Root.AddChild(magicVortex);
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
            ani = Texture2D.FromFile(GraphicsDevice, "../../../Content/Test.png");
        }

        protected override void Draw(GameTime gameTime)
        {
           Engine.Render(gameTime);
            base.Draw(gameTime);
        }
    }
}