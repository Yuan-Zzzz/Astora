using System.Linq;
using Astora.Core.Nodes;
using Astora.Core.Rendering.RenderPipeline;
using Astora.Core.UI.Events;
using Astora.Core.UI.Layout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    /// True when this control has focus. Set by UI focus manager.
    /// </summary>
    public bool IsFocused { get; protected set; }

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
        // Base does not arrange children; containers override and call child.ArrangeChildren(rect) per child.
    }

    #endregion

    #region Node lifecycle

    public override void Update(float delta)
    {
        // Layout is driven by UIRoot.DoLayout(), not here. Control.Update can be used for animation etc.
    }

    public override void Draw(IRenderBatcher renderBatcher)
    {
        // Placeholder: no drawing by default. Subclasses and leaf widgets draw.
        // ClipContent is applied by renderer when drawing children (e.g. UIRoot or a future InvalidationBox).
    }

    #endregion

    #region Event virtual methods (routing)

    public virtual void OnPreviewMouseButtonDown(MouseButtonEventArgs e) { }

    public virtual void OnMouseButtonDown(MouseButtonEventArgs e) { }

    public virtual void OnPreviewMouseButtonUp(MouseButtonEventArgs e) { }

    public virtual void OnMouseButtonUp(MouseButtonEventArgs e) { }

    public virtual void OnPreviewMouseMove(MouseMoveEventArgs e) { }

    public virtual void OnMouseMove(MouseMoveEventArgs e) { }

    #endregion
}
