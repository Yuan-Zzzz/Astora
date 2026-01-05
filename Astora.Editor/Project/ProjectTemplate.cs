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
using Astora.Core.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            Engine.Content = Content;
            Engine.GraphicsDevice = GraphicsDevice;
            Engine.SpriteBatch = _spriteBatch;
        }}

        protected override void Update(GameTime gameTime)
        {{
            _sceneTree.Update(gameTime);
            base.Update(gameTime);
        }}

        protected override void Draw(GameTime gameTime)
        {{
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix viewMatrix = Matrix.Identity;
            if (_sceneTree.ActiveCamera != null)
            {{
                viewMatrix = _sceneTree.ActiveCamera.GetViewMatrix();
            }}

            _spriteBatch.Begin(
                samplerState: SamplerState.PointClamp,
                transformMatrix: viewMatrix
            );
            _sceneTree.Draw(_spriteBatch);
            _spriteBatch.End();

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

