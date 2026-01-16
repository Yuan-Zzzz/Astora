namespace Astora.Editor.Project
{
    /// <summary>
    /// 项目模板类型
    /// </summary>
    public enum ProjectTemplateType
    {
        Minimal,
        SandBox,
        Empty
    }

    /// <summary>
    /// 项目模板 - 生成项目文件内容
    /// </summary>
    public static class ProjectTemplate
    {
        /// <summary>
        /// 生成 .csproj 文件内容
        /// </summary>
        public static string GenerateCsproj(string projectName, ProjectTemplateType templateType)
        {
            return $@"<Project Sdk=""Microsoft.NET.Sdk"">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net8.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <PublishAot>true</PublishAot>
        <InvariantGlobalization>true</InvariantGlobalization>
    </PropertyGroup>

    <ItemGroup>
      <ProjectReference Include=""..\\Astora.Core\\Astora.Core.csproj"" />
    </ItemGroup>

    <ItemGroup>
      <Folder Include=""Scenes\\"" />
    </ItemGroup>

</Project>";
        }

        /// <summary>
        /// 生成 Program.cs 文件内容
        /// </summary>
        public static string GenerateProgramCs(string projectName)
        {
            return $@"namespace {projectName};

class Program
{{
    static void Main(string[] args)
    {{
        Game1 game = new Game1();
        game.Run();
    }}
}}";
        }

        /// <summary>
        /// 生成 Game1.cs 文件内容
        /// </summary>
        public static string GenerateGame1Cs(string projectName, ProjectTemplateType templateType)
        {
            if (templateType == ProjectTemplateType.Empty)
            {
                return $@"using Astora.Core;
using Astora.Core.Project;
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace {projectName}
{{
    public class Game1 : Game
    {{
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SceneTree _sceneTree;

        public Game1()
        {{
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = ""Content"";
            IsMouseVisible = true;
            
            _sceneTree = new SceneTree();
            Engine.CurrentScene = _sceneTree;
        }}

        protected override void Initialize()
        {{
            base.Initialize();
        }}

        protected override void LoadContent()
        {{
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Engine.Initialize(Content, _graphics);
            Engine.CurrentScene = _sceneTree;
            
            // 加载项目配置并应用设计分辨率
            LoadAndApplyProjectConfig();
        }}
        
        /// <summary>
        /// 加载并应用项目配置
        /// </summary>
        private void LoadAndApplyProjectConfig()
        {{
            try
            {{
                // 尝试从当前目录或上级目录查找 project.yaml
                var configPath = ""project.yaml"";
                if (!File.Exists(configPath))
                {{
                    // 尝试在上级目录查找（适用于从 bin/Debug/net8.0 运行的情况）
                    configPath = Path.Combine("".."", "".."", "".."", "".."", ""project.yaml"");
                    if (!File.Exists(configPath))
                    {{
                        // 使用默认配置
                        System.Console.WriteLine(""未找到 project.yaml，使用默认设计分辨率"");
                        return;
                    }}
                }}
                
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                
                var yaml = File.ReadAllText(configPath);
                var config = deserializer.Deserialize<GameProjectConfig>(yaml);
                
                if (config != null)
                {{
                    Engine.SetDesignResolution(config);
                    System.Console.WriteLine($""已加载设计分辨率: {{config.DesignWidth}}x{{config.DesignHeight}}, 缩放模式: {{config.ScalingMode}}"");
                }}
            }}
            catch (Exception ex)
            {{
                System.Console.WriteLine($""加载项目配置失败: {{ex.Message}}，使用默认设计分辨率"");
            }}
        }}

        protected override void Update(GameTime gameTime)
        {{
            Engine.Update(gameTime);
            base.Update(gameTime);
        }}

        protected override void Draw(GameTime gameTime)
        {{
            Engine.Render();
            base.Draw(gameTime);
        }}
    }}
}}";
            }
            else
            {
                // Minimal template (same as Empty for now)
                return GenerateGame1Cs(projectName, ProjectTemplateType.Empty);
            }
        }
    }
}

