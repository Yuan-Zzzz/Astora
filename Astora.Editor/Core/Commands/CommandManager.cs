using System.Collections.Generic;

namespace Astora.Editor.Core.Commands;

/// <summary>
/// 命令管理器：提供 Execute/Undo/Redo 栈。
/// </summary>
public sealed class CommandManager
{
    private readonly Stack<IEditorCommand> _undo = new();
    private readonly Stack<IEditorCommand> _redo = new();

    public int UndoCount => _undo.Count;
    public int RedoCount => _redo.Count;

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public event Action<IEditorCommand>? Executed;
    public event Action<IEditorCommand>? Undone;
    public event Action<IEditorCommand>? Redone;

    public bool TryExecute(IEditorCommand command)
    {
        if (!command.CanExecute())
            return false;

        command.Execute();
        if (command.RecordInHistory)
        {
            _undo.Push(command);
            _redo.Clear();
        }

        Executed?.Invoke(command);
        return true;
    }

    public bool TryUndo()
    {
        if (_undo.Count == 0)
            return false;

        var cmd = _undo.Pop();
        cmd.Undo();
        _redo.Push(cmd);

        Undone?.Invoke(cmd);
        return true;
    }

    public bool TryRedo()
    {
        if (_redo.Count == 0)
            return false;

        var cmd = _redo.Pop();
        cmd.Execute();
        _undo.Push(cmd);

        Redone?.Invoke(cmd);
        return true;
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }
}

