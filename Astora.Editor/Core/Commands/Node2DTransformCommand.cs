using Astora.Core.Nodes;
using Microsoft.Xna.Framework;

namespace Astora.Editor.Core.Commands;

/// <summary>
/// 2D 节点变换命令：用于 Move/Rotate 等工具的一次拖拽提交。
/// </summary>
public sealed class Node2DTransformCommand : IEditorCommand
{
    private readonly Node2D _node;
    private readonly Vector2 _beforePos;
    private readonly float _beforeRot;
    private readonly Vector2 _afterPos;
    private readonly float _afterRot;

    public string Name { get; }
    public bool RecordInHistory { get; } = true;

    public Node2DTransformCommand(
        string name,
        Node2D node,
        Vector2 beforePos,
        float beforeRot,
        Vector2 afterPos,
        float afterRot)
    {
        Name = name;
        _node = node;
        _beforePos = beforePos;
        _beforeRot = beforeRot;
        _afterPos = afterPos;
        _afterRot = afterRot;
    }

    public bool CanExecute() => _node != null;

    public void Execute()
    {
        _node.Position = _afterPos;
        _node.Rotation = _afterRot;
    }

    public void Undo()
    {
        _node.Position = _beforePos;
        _node.Rotation = _beforeRot;
    }
}

