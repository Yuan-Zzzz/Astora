using Astora.Core.Nodes;
using Astora.Editor.Core.Actions;

namespace Astora.Editor.Core.Commands;

/// <summary>
/// 选择节点命令（默认不进入 Undo/Redo 历史）。
/// </summary>
public sealed class SelectNodeCommand : IEditorCommand
{
    private readonly IEditorActions _actions;
    private readonly Node? _prev;
    private readonly Node? _next;

    public string Name => "Select Node";
    public bool RecordInHistory { get; } = false;

    public SelectNodeCommand(IEditorActions actions, Node? prev, Node? next, bool recordInHistory = false)
    {
        _actions = actions;
        _prev = prev;
        _next = next;
        RecordInHistory = recordInHistory;
    }

    public bool CanExecute() => true;

    public void Execute()
    {
        _actions.SetSelectedNode(_next);
    }

    public void Undo()
    {
        _actions.SetSelectedNode(_prev);
    }
}

