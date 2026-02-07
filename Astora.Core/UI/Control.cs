using System.Collections.Generic;
using System.Linq;
using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.Scene;
using Astora.Core.UI.Events;
using Astora.Core.UI.Layout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Astora.Core.UI;

/// <summary>
/// Base class for all UI elements. Godot Control-like hierarchy with two-pass layout and event routing.
/// Inherits Node but not Node2D; uses rect-based layout and FinalRect for positioning.
/// </summary>
public class Control : Node, ILayoutable
{
    #region Geometry and layout

    private Vector2 _size;
    private Vector2 _position;
    private Vector2 _pivotOffset = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Desired size from Pass 1 (ComputeDesiredSize). Read-only; set by layout or self in ComputeDesiredSize.
    /// </summary>
    public Vector2 DesiredSize { get; internal set; }

    /// <summary>
    /// Final rectangle assigned in Pass 2 (ArrangeChildren). Read-only; in parent/screen space.
    /// </summary>
    public Rectangle FinalRect { get; internal set; }

    /// <summary>
    /// Explicit size. When set, used as desired size for non-container controls; otherwise layout drives size.
    /// </summary>
    public Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value) return;
            _size = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Local position. Used when no layout container positions this control; otherwise overwritten by ArrangeChildren.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set
        {
            if (_position == value) return;
            _position = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Pivot offset (0–1). Default (0.5, 0.5) for rotation/scale center.
    /// </summary>
    public Vector2 PivotOffset
    {
        get => _pivotOffset;
        set => _pivotOffset = value;
    }

    /// <summary>
    /// When true, clip children to this control's FinalRect when drawing.
    /// </summary>
    public bool ClipContent { get; set; }

    /// <summary>
    /// Minimum size constraint for layout. Containers (e.g. BoxContainer) use this as lower bound when allocating space.
    /// </summary>
    public Vector2 MinSize { get; set; }

    /// <summary>
    /// Stretch ratio in container layout. When parent has extra space, children with StretchRatio &gt; 0 share it proportionally.
    /// </summary>
    public float StretchRatio { get; set; }

    /// <summary>
    /// When true, this control's FinalRect is computed from anchor and offset relative to parent (Godot-style).
    /// Used by UI root when arranging direct children.
    /// </summary>
    public bool UseAnchorLayout { get; set; }

    private float _anchorLeft = 0f, _anchorTop = 0f, _anchorRight = 0f, _anchorBottom = 0f;
    private int _offsetLeft, _offsetTop, _offsetRight, _offsetBottom;

    public float AnchorLeft { get => _anchorLeft; set { _anchorLeft = value; InvalidateLayout(); } }
    public float AnchorTop { get => _anchorTop; set { _anchorTop = value; InvalidateLayout(); } }
    public float AnchorRight { get => _anchorRight; set { _anchorRight = value; InvalidateLayout(); } }
    public float AnchorBottom { get => _anchorBottom; set { _anchorBottom = value; InvalidateLayout(); } }
    public int OffsetLeft { get => _offsetLeft; set { _offsetLeft = value; InvalidateLayout(); } }
    public int OffsetTop { get => _offsetTop; set { _offsetTop = value; InvalidateLayout(); } }
    public int OffsetRight { get => _offsetRight; set { _offsetRight = value; InvalidateLayout(); } }
    public int OffsetBottom { get => _offsetBottom; set { _offsetBottom = value; InvalidateLayout(); } }

    /// <summary>
    /// Compute this control's rectangle from parent rect using anchor (0–1) and offset. Used when UseAnchorLayout is true.
    /// </summary>
    public Rectangle GetRectFromAnchorAndOffset(Rectangle parentRect)
    {
        int x = (int)(parentRect.X + _anchorLeft * parentRect.Width) + _offsetLeft;
        int y = (int)(parentRect.Y + _anchorTop * parentRect.Height) + _offsetTop;
        int right = (int)(parentRect.X + _anchorRight * parentRect.Width) + _offsetRight;
        int bottom = (int)(parentRect.Y + _anchorBottom * parentRect.Height) + _offsetBottom;
        int w = right - x;
        int h = bottom - y;
        if (w < 0) w = 0;
        if (h < 0) h = 0;
        return new Rectangle(x, y, w, h);
    }

    #endregion

    #region Theme

    private Theme? _theme;
    private Dictionary<string, Color>? _themeColorOverrides;
    private Dictionary<string, int>? _themeConstantOverrides;

    /// <summary>
    /// Theme resource for this control. When null, inherited from parent. Set on UI root or a container to provide defaults.
    /// </summary>
    public Theme? Theme
    {
        get => _theme;
        set => _theme = value;
    }

    /// <summary>
    /// Override a theme color by name. Takes precedence over inherited theme.
    /// </summary>
    public void AddThemeColorOverride(string name, Color color)
    {
        _themeColorOverrides ??= new Dictionary<string, Color>();
        _themeColorOverrides[name] = color;
    }

    /// <summary>
    /// Override a theme constant (int) by name.
    /// </summary>
    public void AddThemeConstantOverride(string name, int value)
    {
        _themeConstantOverrides ??= new Dictionary<string, int>();
        _themeConstantOverrides[name] = value;
    }

    /// <summary>
    /// Resolve a theme color: local override, then parent chain, then theme resource, then default.
    /// </summary>
    public Color GetThemeColor(string name, Color defaultColor = default)
    {
        if (defaultColor == default) defaultColor = Color.White;
        if (_themeColorOverrides != null && _themeColorOverrides.TryGetValue(name, out var over))
            return over;
        if (Parent is Control parent)
            return parent.GetThemeColor(name, defaultColor);
        if (_theme != null && _theme.GetColor(name, out var fromTheme))
            return fromTheme;
        return defaultColor;
    }

    /// <summary>
    /// Resolve a theme constant: local override, then parent chain, then theme resource, then default.
    /// </summary>
    public int GetThemeConstant(string name, int defaultValue = 0)
    {
        if (_themeConstantOverrides != null && _themeConstantOverrides.TryGetValue(name, out var over))
            return over;
        if (Parent is Control parent)
            return parent.GetThemeConstant(name, defaultValue);
        if (_theme != null && _theme.GetConstant(name, out var fromTheme))
            return fromTheme;
        return defaultValue;
    }

    #endregion

    #region Visibility and interaction

    private bool _visible = true;
    private MouseFilter _mouseFilter = MouseFilter.Pass;
    private FocusMode _focusMode = FocusMode.None;

    /// <summary>
    /// When false, control is not laid out, drawn, or hit-tested.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Whether this control participates in hit test and whether input passes to children.
    /// </summary>
    public MouseFilter MouseFilter
    {
        get => _mouseFilter;
        set => _mouseFilter = value;
    }

    /// <summary>
    /// Whether this control can receive focus.
    /// </summary>
    public FocusMode FocusMode
    {
        get => _focusMode;
        set => _focusMode = value;
    }

    /// <summary>
    /// True when this control has focus. Set by SceneTree (global focus).
    /// </summary>
    public bool IsFocused { get; internal set; }

    /// <summary>
    /// Request focus for this control. Uses SceneTree global focus. Only has effect when FocusMode is not None.
    /// </summary>
    public void GrabFocus()
    {
        if (FocusMode == FocusMode.None) return;
        Engine.CurrentScene?.SetFocusedControl(this);
    }

    #endregion

    #region Visual

    private Color _modulate = Color.White;
    private int _zIndex;

    /// <summary>
    /// Multiply color for rendering. Default White.
    /// </summary>
    public Color Modulate
    {
        get => _modulate;
        set
        {
            _modulate = value;
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Order within same parent: higher drawn and hit-tested last (on top).
    /// </summary>
    public int ZIndex
    {
        get => _zIndex;
        set
        {
            if (_zIndex == value) return;
            _zIndex = value;
            InvalidateLayout();
        }
    }

    #endregion

    #region Dirty state

    private bool _isLayoutDirty = true;
    private bool _isVisualDirty = true;

    /// <summary>
    /// This node or subtree needs a layout pass (two-pass).
    /// </summary>
    public bool IsLayoutDirty => _isLayoutDirty;

    /// <summary>
    /// This node or subtree needs repaint (for future Invalidation Box).
    /// </summary>
    public bool IsVisualDirty => _isVisualDirty;

    #endregion

    public Control() : this("Control") { }

    public Control(string name) : base(name) { }

    #region Child management (invalidate layout)

    public new void AddChild(Node child)
    {
        base.AddChild(child);
        InvalidateLayout();
    }

    public new void RemoveChild(Node child)
    {
        base.RemoveChild(child);
        InvalidateLayout();
    }

    /// <summary>
    /// Add a Control child. Convenience that marks layout dirty.
    /// </summary>
    public void AddChild(Control child)
    {
        AddChild((Node)child);
    }

    /// <summary>
    /// Remove a Control child.
    /// </summary>
    public void RemoveChild(Control child)
    {
        RemoveChild((Node)child);
    }

    #endregion

    #region Invalidation

    /// <summary>
    /// Mark this node and ancestors as layout dirty (so next DoLayout runs).
    /// </summary>
    public void InvalidateLayout()
    {
        if (_isLayoutDirty) return;
        _isLayoutDirty = true;
        _isVisualDirty = true;
        if (Parent is Control parentControl)
            parentControl.InvalidateLayout();
    }

    /// <summary>
    /// Mark this node and optionally subtree as visual dirty (for future invalidation box).
    /// </summary>
    public void InvalidateVisual()
    {
        _isVisualDirty = true;
    }

    /// <summary>
    /// Clear layout dirty on this node and all descendants. Called after DoLayout.
    /// </summary>
    public void ClearLayoutDirty()
    {
        _isLayoutDirty = false;
        foreach (var child in Children)
        {
            if (child is Control c)
                c.ClearLayoutDirty();
        }
    }

    /// <summary>
    /// Clear visual dirty. Called when cached render is updated.
    /// </summary>
    internal void ClearVisualDirty()
    {
        _isVisualDirty = false;
    }

    #endregion

    #region ILayoutable

    /// <inheritdoc />
    public virtual Vector2 ComputeDesiredSize()
    {
        var controlChildren = Children.OfType<Control>().ToList();
        if (controlChildren.Count == 0)
        {
            DesiredSize = _size.X >= 0 && _size.Y >= 0 ? _size : Vector2.Zero;
            return DesiredSize;
        }
        // Base implementation for non-containers: desired size is explicit Size or zero.
        DesiredSize = _size.X >= 0 && _size.Y >= 0 ? _size : Vector2.Zero;
        return DesiredSize;
    }

    /// <inheritdoc />
    public virtual void ArrangeChildren(Rectangle finalRect)
    {
        FinalRect = finalRect;
        if (IsUIRoot && GetType() == typeof(Control) && Children.OfType<Control>().Any())
        {
            foreach (var child in Children.OfType<Control>())
            {
                if (child.UseAnchorLayout)
                {
                    var rect = child.GetRectFromAnchorAndOffset(finalRect);
                    child.ArrangeChildren(rect);
                }
                else
                    child.ArrangeChildren(finalRect);
            }
        }
    }

    /// <summary>
    /// True when this control acts as a UI root (parent is not a Control).
    /// </summary>
    private bool IsUIRoot => Parent is not Control;

    /// <summary>
    /// Run layout if dirty: Pass 1 bottom-up ComputeDesiredSize, Pass 2 top-down ArrangeChildren. Called by SceneTree for each UI root with design resolution rect.
    /// </summary>
    public void DoLayout(Rectangle viewportRect)
    {
        if (!IsLayoutDirty) return;
        ComputeDesiredSizeRecursive(this);
        ArrangeChildren(viewportRect);
        ClearLayoutDirty();
    }

    private static Vector2 ComputeDesiredSizeRecursive(Control c)
    {
        foreach (var child in c.Children.OfType<Control>())
            ComputeDesiredSizeRecursive(child);
        return c.ComputeDesiredSize();
    }

    /// <summary>
    /// Find the topmost control that contains the point (in design resolution). Uses MouseFilter and Visible.
    /// </summary>
    public Control? HitTest(Vector2 pointInScreenSpace)
    {
        return HitTestRecursive(this, pointInScreenSpace);
    }

    private static Control? HitTestRecursive(Control c, Vector2 pointInLocalSpace)
    {
        if (!c.Visible || c.MouseFilter == MouseFilter.Ignore)
            return null;
        var localBounds = new Rectangle(0, 0, c.FinalRect.Width, c.FinalRect.Height);
        if (!localBounds.Contains(pointInLocalSpace.X, pointInLocalSpace.Y))
            return null;

        var withIndex = c.Children
            .Select((node, index) => (Control: node as Control, Index: index))
            .Where(x => x.Control != null)
            .OrderByDescending(x => x.Control!.ZIndex)
            .ThenByDescending(x => x.Index)
            .Select(x => x.Control!)
            .ToList();

        foreach (var child in withIndex)
        {
            if (!child.Visible || child.MouseFilter == MouseFilter.Ignore) continue;
            if (!child.FinalRect.Contains(pointInLocalSpace.X, pointInLocalSpace.Y)) continue;
            if (child.MouseFilter == MouseFilter.Stop)
                return child;
            var childLocalPoint = new Vector2(
                pointInLocalSpace.X - child.FinalRect.X,
                pointInLocalSpace.Y - child.FinalRect.Y);
            var hit = HitTestRecursive(child, childLocalPoint);
            if (hit != null) return hit;
            return child;
        }

        return c;
    }

    #endregion

    #region Event routing and input (for UI root)

    private Vector2 _lastMousePosition;

    private static List<Control> GetPathToTarget(Control root, Control? target)
    {
        if (target == null) return new List<Control> { root };
        var path = new List<Control>();
        for (var n = target; n != null; n = n.Parent as Control)
            path.Add(n);
        path.Reverse();
        return path;
    }

    private void RouteMouseButtonDown(Control? hitTarget, MouseButtonEventArgs args)
    {
        var path = GetPathToTarget(this, hitTarget ?? this);
        for (int i = 0; i < path.Count && !args.Handled; i++)
            path[i].OnPreviewMouseButtonDown(args);
        if (args.Handled) return;
        for (int i = path.Count - 1; i >= 0 && !args.Handled; i--)
            path[i].OnMouseButtonDown(args);
    }

    private void RouteMouseButtonUp(Control? hitTarget, MouseButtonEventArgs args)
    {
        var path = GetPathToTarget(this, hitTarget ?? this);
        for (int i = 0; i < path.Count && !args.Handled; i++)
            path[i].OnPreviewMouseButtonUp(args);
        if (args.Handled) return;
        for (int i = path.Count - 1; i >= 0 && !args.Handled; i--)
            path[i].OnMouseButtonUp(args);
    }

    private void RouteMouseMove(Control? hitTarget, MouseMoveEventArgs args)
    {
        var path = GetPathToTarget(this, hitTarget ?? this);
        for (int i = 0; i < path.Count && !args.Handled; i++)
            path[i].OnPreviewMouseMove(args);
        if (args.Handled) return;
        for (int i = path.Count - 1; i >= 0 && !args.Handled; i--)
            path[i].OnMouseMove(args);
    }

    private void RouteKeyDown(Control? focus, KeyEventArgs args)
    {
        if (focus == null) return;
        var path = GetPathToTarget(this, focus);
        for (int i = 0; i < path.Count && !args.Handled; i++)
            path[i].OnPreviewKeyDown(args);
        if (args.Handled) return;
        for (int i = path.Count - 1; i >= 0 && !args.Handled; i--)
            path[i].OnKeyDown(args);
    }

    private void RouteKeyUp(Control? focus, KeyEventArgs args)
    {
        if (focus == null) return;
        var path = GetPathToTarget(this, focus);
        for (int i = 0; i < path.Count && !args.Handled; i++)
            path[i].OnPreviewKeyUp(args);
        if (args.Handled) return;
        for (int i = path.Count - 1; i >= 0 && !args.Handled; i--)
            path[i].OnKeyUp(args);
    }

    private static void CollectFocusable(Control c, List<Control> list)
    {
        if (!c.Visible || c.FocusMode == FocusMode.None) return;
        list.Add(c);
        foreach (var child in c.Children.OfType<Control>())
            CollectFocusable(child, list);
    }

    private Control? FindNextValidFocus()
    {
        var list = new List<Control>();
        CollectFocusable(this, list);
        if (list.Count == 0) return null;
        var focus = Engine.CurrentScene?.GetFocusedControl();
        int idx = focus != null ? list.IndexOf(focus) : -1;
        return list[(idx + 1) % list.Count];
    }

    private Control? FindPrevValidFocus()
    {
        var list = new List<Control>();
        CollectFocusable(this, list);
        if (list.Count == 0) return null;
        var focus = Engine.CurrentScene?.GetFocusedControl();
        int idx = focus != null ? list.IndexOf(focus) : 0;
        idx = idx <= 0 ? list.Count - 1 : idx - 1;
        return list[idx];
    }

    /// <summary>
    /// Process mouse and keyboard for this UI tree. Called by SceneTree with the hit target (or null).
    /// Uses global focus from SceneTree for keyboard and Tab.
    /// </summary>
    internal void ProcessInputEvents(Control? hitTarget)
    {
        var pos = Input.MouseScreenPosition;

        if (Input.IsLeftMouseButtonPressed())
        {
            if (hitTarget != null && hitTarget.FocusMode != FocusMode.None)
                hitTarget.GrabFocus();
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Left,
                Pressed = true
            };
            RouteMouseButtonDown(hitTarget, args);
        }
        if (Input.IsRightMouseButtonPressed())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Right,
                Pressed = true
            };
            RouteMouseButtonDown(hitTarget, args);
        }

        if (Input.IsLeftMouseButtonReleased())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Left,
                Pressed = false
            };
            RouteMouseButtonUp(hitTarget, args);
        }
        if (Input.IsRightMouseButtonReleased())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Right,
                Pressed = false
            };
            RouteMouseButtonUp(hitTarget, args);
        }

        if (pos != _lastMousePosition)
        {
            var moveArgs = new MouseMoveEventArgs
            {
                Position = pos,
                PreviousPosition = _lastMousePosition
            };
            RouteMouseMove(hitTarget, moveArgs);
            _lastMousePosition = pos;
        }

        var focus = Engine.CurrentScene?.GetFocusedControl();
        if (focus == null) return;

        if (Input.IsKeyPressed(Keys.Tab))
        {
            var next = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift)
                ? FindPrevValidFocus()
                : FindNextValidFocus();
            if (next != null)
                Engine.CurrentScene?.SetFocusedControl(next);
        }
        else
        {
            foreach (var key in Input.GetKeysPressedThisFrame())
            {
                var args = new KeyEventArgs { Key = key, Pressed = true };
                RouteKeyDown(focus, args);
            }
            foreach (var key in Input.GetKeysReleasedThisFrame())
            {
                var args = new KeyEventArgs { Key = key, Pressed = false };
                RouteKeyUp(focus, args);
            }
        }
    }

    #endregion

    #region Node lifecycle

    public override void Update(float delta)
    {
        // Layout is driven by SceneTree (ProcessUILayoutAndInput calls DoLayout on each UI root). Control.Update can be used for animation etc.
    }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        // Placeholder: no drawing by default. Subclasses and leaf widgets draw.
    }

    /// <summary>
    /// Draw children in ZIndex order (ascending; same as Godot). Non-Control children use ZIndex 0.
    /// When ClipContent is true, children are clipped to this control's FinalRect.
    /// </summary>
    protected override void DrawChildren(IRenderBatcher renderBatcher)
    {
        if (Children.Count == 0) return;

        if (ClipContent)
            renderBatcher.PushScissorRect(FinalRect);

        var indices = new List<int>(Children.Count);
        for (int i = 0; i < Children.Count; i++) indices.Add(i);
        indices.Sort((i, j) =>
        {
            int zi = Children[i] is Control ci ? ci.ZIndex : 0;
            int zj = Children[j] is Control cj ? cj.ZIndex : 0;
            if (zi != zj) return zi.CompareTo(zj);
            return i.CompareTo(j);
        });
        foreach (int i in indices)
            Children[i].InternalDraw(renderBatcher);

        if (ClipContent)
            renderBatcher.PopScissorRect();
    }

    #endregion

    #region Event virtual methods (routing)

    public virtual void OnPreviewMouseButtonDown(MouseButtonEventArgs e) { }

    public virtual void OnMouseButtonDown(MouseButtonEventArgs e) { }

    public virtual void OnPreviewMouseButtonUp(MouseButtonEventArgs e) { }

    public virtual void OnMouseButtonUp(MouseButtonEventArgs e) { }

    public virtual void OnPreviewMouseMove(MouseMoveEventArgs e) { }

    public virtual void OnMouseMove(MouseMoveEventArgs e) { }

    public virtual void OnPreviewKeyDown(KeyEventArgs e) { }

    public virtual void OnKeyDown(KeyEventArgs e) { }

    public virtual void OnPreviewKeyUp(KeyEventArgs e) { }

    public virtual void OnKeyUp(KeyEventArgs e) { }

    /// <summary>Called when this control gains focus.</summary>
    public virtual void OnFocusEnter() { }

    /// <summary>Called when this control loses focus.</summary>
    public virtual void OnFocusExit() { }

    #endregion
}
