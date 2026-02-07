using System.Collections.Generic;
using System.Linq;
using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI;
using Astora.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Core.Scene;

public class SceneTree
{
    private readonly ISceneSerializer? _serializer;
    private Control? _focusedControl;

    /// <summary>
    /// Optional serializer for load/save. When null, uses Engine.CurrentContext.Serializer.
    /// </summary>
    public SceneTree(ISceneSerializer? serializer = null)
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Root Node of the Scene
    /// </summary>
    public Node Root { get; private set; }
    /// <summary>
    /// Currently Active Camera
    /// </summary>
    public Camera2D ActiveCamera { get; set; }

    private ISceneSerializer GetSerializer()
    {
        return _serializer ?? Engine.CurrentContext?.Serializer ?? throw new InvalidOperationException("Scene serializer not set. Set Engine.CurrentContext or pass a serializer to SceneTree.");
    }

    /// <summary>
    /// Change the current scene root (alias for AttachScene)
    /// </summary>
    public void ChangeScene(Node newSceneRoot)
    {
        AttachScene(newSceneRoot);
    }

    public void AttachScene(Node newSceneRoot)
    {
        // Set new root
        Root = newSceneRoot;
        
        // Call Ready on the new root
        if (Root != null)
        {
            Root.Ready();
            
            // Try to find a Camera2D in the new scene
            var camera = GetNode<Camera2D>();
            if (camera != null)
            {
                ActiveCamera = camera;
            }
        }
        else
        {
            ActiveCamera = null;
        }
    }

    /// <summary>
    /// Load Scene from file
    /// </summary>
    public void AttachScene(string scenePath)
    {
        var scene = GetSerializer().Load(scenePath);
        AttachScene(scene);
    }

    public void SaveScene(string scenePath)
    {
        GetSerializer().Save(Root, scenePath);
        Logger.Info($"Scene saved to {scenePath}");
    }

    /// <summary>
    /// Update Nodes
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Update Input
        Input.Update();

        // Update Nodes
        if (Root != null)
        {
            Root.InternalUpdate(gameTime);
            ProcessUILayoutAndInput();
        }
    }

    /// <summary>
    /// Currently focused control (global). Tab cycles within the same UI tree.
    /// </summary>
    public Control? GetFocusedControl() => _focusedControl;

    /// <summary>
    /// Set the focused control. Only has effect if the control has FocusMode != None.
    /// </summary>
    public void SetFocusedControl(Control? control)
    {
        if (control != null && control.FocusMode == FocusMode.None) return;
        if (_focusedControl == control) return;
        var old = _focusedControl;
        _focusedControl = control;
        if (old != null)
        {
            old.IsFocused = false;
            old.OnFocusExit();
        }
        if (control != null)
        {
            control.IsFocused = true;
            control.OnFocusEnter();
        }
    }

    /// <summary>
    /// Collect all UI roots: direct Control children of Root (layer 0), and direct Control children of each CanvasLayer (that layer).
    /// Sorted by layer ascending (draw/layout order). Hit test uses reverse order (top layer first).
    /// </summary>
    public IEnumerable<(int Layer, Control Root)> GetUIRoots()
    {
        if (Root == null) yield break;
        var list = new List<(int, Control)>();
        foreach (var child in Root.Children)
        {
            if (child is CanvasLayer cl)
            {
                foreach (var c in cl.Children.OfType<Control>())
                    list.Add((cl.Layer, c));
            }
            else if (child is Control c)
            {
                list.Add((0, c));
            }
        }
        foreach (var item in list.OrderBy(x => x.Item1))
            yield return item;
    }

    /// <summary>
    /// Run layout for each UI root, then hit-test and route input (Godot-style driver).
    /// </summary>
    private void ProcessUILayoutAndInput()
    {
        var roots = GetUIRoots().ToList();
        if (roots.Count == 0) return;

        var canvasRect = new Rectangle(0, 0, Engine.DesignResolution.X, Engine.DesignResolution.Y);
        foreach (var (_, root) in roots)
            root.DoLayout(canvasRect);

        var pos = Input.MouseScreenPosition;
        Control? hitControl = null;
        Control? hitRoot = null;
        foreach (var (_, root) in roots.OrderByDescending(x => x.Layer))
        {
            var hit = root.HitTest(pos);
            if (hit != null)
            {
                hitControl = hit;
                hitRoot = root;
                break;
            }
        }

        var focusRoot = GetRootContaining(_focusedControl, roots);
        var driverRoot = hitRoot ?? focusRoot;
        if (driverRoot != null)
            driverRoot.ProcessInputEvents(hitControl);
    }

    private static Control? GetRootContaining(Control? control, List<(int Layer, Control Root)> roots)
    {
        if (control == null) return null;
        for (var n = control.Parent; n != null; n = n.Parent)
        {
            if (n is Control c && roots.Any(r => r.Root == c))
                return c;
        }
        return null;
    }

    /// <summary>
    /// Draw Nodes
    /// </summary>
    public void Draw(IRenderBatcher renderBatcher)
    {
        if (Root != null)
        {
            Root.InternalDraw(renderBatcher);
        }
    }
    
    /// <summary>
    /// Set the current active camera
    /// </summary>
    public void SetCurrentCamera(Camera2D camera)
    {
        ActiveCamera = camera;
    }
    
    /// <summary>
    /// 清理标记删除的节点（用于编辑器模式）
    /// </summary>
    public void CleanupQueuedNodes()
    {
        if (Root != null)
        {
            CleanupNode(Root);
        }
    }
    
    private void CleanupNode(Node node)
    {
        // 先清理子节点
        for (int i = node.Children.Count - 1; i >= 0; i--)
        {
            var child = node.Children[i];
            CleanupNode(child);
            
            if (child.IsQueuedForDeletion)
            {
                node.RemoveChildAt(i);
            }
        }
    }
    
    /// <summary>
    /// Get Node by Type
    /// </summary>
    public T GetNode<T>() where T : Node
    {
        return FindNode<T>(Root);
    }
    
    /// <summary>
    /// Find Node by Name
    /// </summary>
    public Node FindNode(string name)
    {
        return FindNodeByName(Root, name);
    }
    
    /// <summary>
    /// Find Node by Type (recursive)
    /// </summary>
    private T FindNode<T>(Node node) where T : Node
    {
        if (node == null) return null;
        if (node is T result) return result;
        
        foreach (var child in node.Children)
        {
            var found = FindNode<T>(child);
            if (found != null) return found;
        }
        
        return null;
    }
    
    /// <summary>
    /// Find Node by Name (recursive)
    /// </summary>
    private Node FindNodeByName(Node node, string name)
    {
        if (node == null) return null;
        if (node.Name == name) return node;
        
        foreach (var child in node.Children)
        {
            var found = FindNodeByName(child, name);
            if (found != null) return found;
        }
        
        return null;
    }
}
