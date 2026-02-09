using Astora.Editor.Core.Actions;
using Astora.Editor.Core.Commands;
using Astora.Editor.Core.Events;
using Astora.Editor.Services;

namespace Astora.Editor.Core;

/// <summary>
/// 编辑器上下文：提供统一依赖访问入口，避免 UI 直接依赖宿主 Game。
/// </summary>
public interface IEditorContext
{
    EditorConfig Config { get; }
    ProjectService ProjectService { get; }
    EditorService EditorService { get; }
    RenderService RenderService { get; }

    CommandManager Commands { get; }
    IEventBus Events { get; }
    IEditorActions Actions { get; }
}

