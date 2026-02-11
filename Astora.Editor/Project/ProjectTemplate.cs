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
using Astora.Core.Game;
using Astora.Core.Project;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace {projectName}
{{
    public class Game1 : Game
    {{
        private readonly GraphicsDeviceManager _graphics;
        private IGameRuntime _runtime = null!;

        public Game1()
        {{
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = ""Content"";
            IsMouseVisible = true;
        }}

        protected override void Initialize()
        {{
            base.Initialize();
            Engine.Initialize(Content, _graphics);
            var config = Engine.LoadProjectConfig() ?? GameProjectConfig.CreateDefault();
            _runtime = new DefaultGameRuntime();
            _runtime.Initialize(Engine.Content, config, Engine.CurrentScene, skipInitialSceneLoad: false);
        }}

        protected override void LoadContent()
        {{
            base.LoadContent();
        }}

        protected override void Update(GameTime gameTime)
        {{
            _runtime.Update(gameTime);
            Engine.Update(gameTime);
            base.Update(gameTime);
        }}

        protected override void Draw(GameTime gameTime)
        {{
            Engine.Render(gameTime, Color.Black);
            base.Draw(gameTime);
        }}
    }}

    /// <summary>
    /// 默认游戏逻辑入口，与 Editor 播放共用。可在此添加场景加载与输入逻辑。
    /// </summary>
    public class DefaultGameRuntime : IGameRuntime
    {{
        public void Initialize(Microsoft.Xna.Framework.Content.ContentManager content, GameProjectConfig config, Astora.Core.Scene.SceneTree sceneTree, bool skipInitialSceneLoad)
        {{
            if (!skipInitialSceneLoad)
            {{
                // 可在此加载默认场景，例如: sceneTree.AttachScene(myRoot);
            }}
        }}

        public void Update(GameTime gameTime)
        {{
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

