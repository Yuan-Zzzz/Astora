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
using System.Runtime.InteropServices;

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

        // 1) 检测 DPI 缩放
        float uiScale = DetectUiScale();

        // 2) 加载字体（按缩放后的字号）
        var io = ImGui.GetIO();
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "msyh.ttc");
        float fontSize = _config.BaseFontSize * uiScale;

        if (File.Exists(fontPath))
        {
            io.Fonts.AddFontFromFileTTF(fontPath, fontSize, null, io.Fonts.GetGlyphRangesChineseFull());
        }
        else
        {
            System.Console.WriteLine($"警告：未找到字体文件 {fontPath}，将使用默认字体");
        }
        _imGuiRenderer.RebuildFontAtlas();

        // 3) 应用主题 + 缩放
        ImGuiStyleManager.ApplyModernDarkTheme();
        ImGuiStyleManager.ApplyScale(uiScale);

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
        // 轮询异步项目加载是否完成
        if (_actions is EditorActions editorActions)
            editorActions.PollAsyncLoad();

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

    /// <summary>
    /// 检测 UI 缩放因子：如果用户手动设置了 UiScale > 0 则使用用户值，
    /// 否则尝试通过 SDL 获取显示器 DPI 自动计算。
    /// </summary>
    private float DetectUiScale()
    {
        // 手动覆盖
        if (_config.UiScale > 0f)
            return _config.UiScale;

        try
        {
            // MonoGame DesktopGL 使用 SDL2 后端，尝试通过 P/Invoke 获取 DPI
            float dpi = GetDisplayDpi();
            float scale = dpi / 96f;

            // 限制在合理范围内
            scale = Math.Clamp(scale, 0.75f, 3.0f);

            System.Console.WriteLine($"[DPI] 检测到显示器 DPI: {dpi:F0}, 缩放因子: {scale:F2}");
            return scale;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DPI] 无法检测 DPI ({ex.Message})，使用默认缩放 1.0");
            return 1.0f;
        }
    }

    /// <summary>
    /// 通过 SDL2 获取主显示器 DPI
    /// </summary>
    private static float GetDisplayDpi()
    {
        // MonoGame DesktopGL 依赖 SDL2
        // SDL_GetDisplayDPI(int displayIndex, float* ddpi, float* hdpi, float* vdpi)
        int result = SDL_GetDisplayDPI(0, out float ddpi, out _, out _);
        if (result == 0 && ddpi > 0)
            return ddpi;

        return 96f; // 默认 96 DPI
    }

    [DllImport("SDL2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayDPI")]
    private static extern int SDL_GetDisplayDPI(int displayIndex, out float ddpi, out float hdpi, out float vdpi);
}
