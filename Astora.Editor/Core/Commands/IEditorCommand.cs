namespace Astora.Editor.Core.Commands;

/// <summary>
/// 编辑器命令：用于统一封装可撤销的编辑操作。
/// </summary>
public interface IEditorCommand
{
    string Name { get; }

    /// <summary>
    /// 是否需要写入 Undo/Redo 历史。
    /// 用于“选择变化”等临时操作：可以命令化，但不污染历史栈。
    /// </summary>
    bool RecordInHistory { get; }

    /// <summary>
    /// 是否允许执行（用于菜单禁用/快捷键提示）。
    /// </summary>
    bool CanExecute();

    /// <summary>
    /// 执行命令。
    /// </summary>
    void Execute();

    /// <summary>
    /// 撤销命令。若命令不可撤销，可抛出 NotSupportedException。
    /// </summary>
    void Undo();
}

