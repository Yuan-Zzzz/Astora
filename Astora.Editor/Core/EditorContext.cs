using Astora.Editor.Core.Actions;
using Astora.Editor.Core.Commands;
using Astora.Editor.Core.Events;
using Astora.Editor.Services;

namespace Astora.Editor.Core;

public sealed class EditorContext : IEditorContext
{
    public EditorConfig Config { get; }
    public ProjectService ProjectService { get; }
    public EditorService EditorService { get; }
    public RenderService RenderService { get; }

    public CommandManager Commands { get; }
    public IEventBus Events { get; }
    public IEditorActions Actions { get; }

    public EditorContext(
        EditorConfig config,
        ProjectService projectService,
        EditorService editorService,
        RenderService renderService,
        CommandManager commands,
        IEventBus events,
        IEditorActions actions)
    {
        Config = config;
        ProjectService = projectService;
        EditorService = editorService;
        RenderService = renderService;
        Commands = commands;
        Events = events;
        Actions = actions;
    }
}

