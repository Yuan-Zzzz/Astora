using Astora.Core;
using Astora.Editor.Core;
using Astora.Editor.Core.Actions;
using Astora.Editor.Core.Commands;
using Astora.Editor.Core.Events;
using Astora.Editor.Services;
using Astora.Editor.UI;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Astora.Editor;

/// <summary>
/// Editor 宿主（MonoGame Game）：只负责生命周期与渲染循环。
/// 业务能力通过 Context/Actions/Commands/Events 注入给 UI，避免 UI 直接依赖 Game。
/// </summary>
public sealed class Editor : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private ImGuiRenderer _imGuiRenderer = null!;

    private readonly EditorConfig _config;
    private readonly ProjectService _projectService;
    private readonly EditorService _editorService;
    private readonly RenderService _renderService;

    private readonly CommandManager _commands;
    private readonly IEventBus _events;
    private readonly IEditorActions _actions;
    private readonly IEditorContext _ctx;

    private EditorUi _ui = null!;

    public Editor()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _config = new EditorConfig();
        _projectService = new ProjectService();
        _editorService = new EditorService(_projectService);
        _renderService = new RenderService();

        _commands = new CommandManager();
        _events = new EventBus();
        _actions = new EditorActions(_projectService, _editorService);
        _ctx = new EditorContext(_config, _projectService, _editorService, _renderService, _commands, _events, _actions);

        _graphics.PreferredBackBufferWidth = _config.DefaultWindowWidth;
        _graphics.PreferredBackBufferHeight = _config.DefaultWindowHeight;
    }

    protected override void Initialize()
    {
        base.Initialize();

        Window.AllowUserResizing = _config.AllowWindowResizing;
        Window.ClientSizeChanged += OnClientSizeChanged;

        _imGuiRenderer = new ImGuiRenderer(this);

        // 字体：优先尝试加载 Fonts/msyh.ttc（微软雅黑）用于中文。
        var io = ImGui.GetIO();
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "msyh.ttc");
        if (File.Exists(fontPath))
        {
            io.Fonts.AddFontFromFileTTF(fontPath, 18.0f, null, io.Fonts.GetGlyphRangesChineseFull());
        }
        else
        {
            System.Console.WriteLine($"警告：未找到字体文件 {fontPath}，将使用默认字体");
        }
        _imGuiRenderer.RebuildFontAtlas();

        _ui = new EditorUi(_ctx, _imGuiRenderer);
    }

    protected override void LoadContent()
    {
        // 关键：传入 NodeTypeRegistry 作为 NodeFactory，确保序列化/反序列化可创建项目自定义节点。
        Engine.Initialize(Content, _graphics, _projectService.NodeTypeRegistry);
        Engine.CurrentScene = _editorService.SceneTree;
    }

    protected override void Update(GameTime gameTime)
    {
        if (_editorService.State.IsPlaying)
            Engine.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_config.BackgroundColor);

        // 视口渲染（RenderTarget 更新），在 ImGui 之前进行。
        var spriteBatch = _renderService.GetSpriteBatch();
        _ui.DrawViewport(spriteBatch);

        _imGuiRenderer.BeforeLayout(gameTime);
        _ui.Render();
        _imGuiRenderer.AfterLayout();

        base.Draw(gameTime);
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        if (Window.ClientBounds.Width <= 0 || Window.ClientBounds.Height <= 0)
            return;

        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();
    }
}
