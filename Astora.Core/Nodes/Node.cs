using Astora.Core.Attributes;
using Astora.Core.Rendering.RenderPipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Nodes;

public class Node
{
    /// <summary>
    /// Name of the node for identification
    /// </summary>
    [SerializeField]
    private string _name = "Node";

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Parent node in the hierarchy
    /// </summary>
    public Node Parent { get; private set; }

    /// <summary>
    /// Node children in the hierarchy (read-only view)
    /// </summary>
    private readonly List<Node> _children = new List<Node>();
    public IReadOnlyList<Node> Children => _children;

    // Mark if this node is queued for deletion
    public bool IsQueuedForDeletion { get; private set; } = false;
    public event Action OnExitTree;

    public Node() : this("Node") { }

    public Node(string name)
    {
        _name = name;
    }

    #region Child Management
    /// <summary>
    /// Adds a child node to this node.
    /// </summary>
    /// <param name="child"></param>
    public void AddChild(Node child)
    {
        if (child.Parent != null)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        _children.Add(child);
        child.Ready();
    }

    /// <summary>
    /// Removes a child node from this node.
    /// </summary>
    /// <param name="child"></param>
    public void RemoveChild(Node child)
    {
        if (_children.Contains(child))
        {
            child.ExitTree();
            child.Parent = null;
            _children.Remove(child);
        }
    }

    /// <summary>
    /// Marks this node and its children for deletion.
    /// </summary>
    public void QueueFree()
    {
        IsQueuedForDeletion = true;
        foreach (var child in Children)
        {
            child.QueueFree();
        }

        ExitTree();
    }
    #endregion

    #region Lifecycle Methods
    public virtual void Ready() { }
    public virtual void Update(float delta) { }
    public virtual void Draw(IRenderBatcher renderBatcher) { }
    public virtual void ExitTree()
    {
        OnExitTree?.Invoke();
        OnExitTree = null;
    }
    #endregion

    #region Node Search Methods
    /// <summary>
    /// 按类型查找子节点（递归）
    /// </summary>
    public T GetNode<T>() where T : Node
    {
        foreach (var child in Children)
        {
            if (child is T result) return result;
            var found = child.GetNode<T>();
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// 按名称查找子节点（递归）
    /// </summary>
    public Node FindNode(string name)
    {
        foreach (var child in Children)
        {
            if (child.Name == name) return child;
            var found = child.FindNode(name);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// 获取所有指定类型的子节点（递归）
    /// </summary>
    public IEnumerable<T> GetChildren<T>() where T : Node
    {
        foreach (var child in Children)
        {
            if (child is T result) yield return result;
            foreach (var grandchild in child.GetChildren<T>())
                yield return grandchild;
        }
    }
    #endregion

    #region Internal Methods
    /// <summary>
    /// Remove child at index (internal use for cleanup)
    /// </summary>
    internal void RemoveChildAt(int index)
    {
        _children.RemoveAt(index);
    }

    internal void InternalUpdate(GameTime gameTime)
    {
        if (IsQueuedForDeletion) return;
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Update(delta);

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            Children[i].InternalUpdate(gameTime);
        }

        _children.RemoveAll(c => c.IsQueuedForDeletion);
    }
    /// <summary>
    /// Override to customize how children are drawn (e.g. Control sorts by ZIndex).
    /// </summary>
    protected virtual void DrawChildren(IRenderBatcher renderBatcher)
    {
        foreach (var child in Children)
        {
            child.InternalDraw(renderBatcher);
        }
    }

    internal void InternalDraw(IRenderBatcher renderBatcher)
    {
        if (IsQueuedForDeletion) return;

        Draw(renderBatcher);
        DrawChildren(renderBatcher);
    }
    #endregion
}
