using System.Collections.Generic;
using System.Linq;
using Astora.Core;
using Astora.Core.Inputs;
using Astora.Core.Nodes;
using Astora.Core.UI.Events;
using Astora.Core.UI.Layout;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Astora.Core.UI;

/// <summary>
/// Root of the UI tree. Drives two-pass layout, hit testing, and event routing. Use design resolution as canvas.
/// </summary>
public class UIRoot : Control
{
    private int _viewportWidth;
    private int _viewportHeight;
    private Control? _focusedControl;

    /// <summary>
    /// Canvas width for layout (default from Engine.DesignResolution).
    /// </summary>
    public int ViewportWidth
    {
        get => _viewportWidth;
        set
        {
            if (_viewportWidth == value) return;
            _viewportWidth = value;
            InvalidateLayout();
        }
    }

    /// <summary>
    /// Canvas height for layout.
    /// </summary>
    public int ViewportHeight
    {
        get => _viewportHeight;
        set
        {
            if (_viewportHeight == value) return;
            _viewportHeight = value;
            InvalidateLayout();
        }
    }

    public UIRoot() : base("UIRoot")
    {
        _viewportWidth = 0;
        _viewportHeight = 0;
    }

    /// <summary>
    /// Initialize viewport from engine design resolution. Call once after Engine is initialized.
    /// </summary>
    public void SetViewportFromDesignResolution()
    {
        var pt = Engine.DesignResolution;
        ViewportWidth = pt.X;
        ViewportHeight = pt.Y;
    }

    public override void Ready()
    {
        base.Ready();
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
            SetViewportFromDesignResolution();
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        DoLayout();
        ProcessInputEvents();
    }

    #region Two-pass layout

    /// <summary>
    /// Run layout if dirty: Pass 1 bottom-up ComputeDesiredSize, Pass 2 top-down ArrangeChildren.
    /// </summary>
    public void DoLayout()
    {
        if (!IsLayoutDirty) return;
        int w = _viewportWidth > 0 ? _viewportWidth : Engine.DesignResolution.X;
        int h = _viewportHeight > 0 ? _viewportHeight : Engine.DesignResolution.Y;
        var canvasRect = new Rectangle(0, 0, w, h);

        ComputeDesiredSizeRecursive(this);
        ArrangeChildren(canvasRect);
        ClearLayoutDirty();
    }

    private static Vector2 ComputeDesiredSizeRecursive(Control c)
    {
        foreach (var child in c.Children.OfType<Control>())
            ComputeDesiredSizeRecursive(child);
        return c.ComputeDesiredSize();
    }

    public override void ArrangeChildren(Rectangle finalRect)
    {
        base.ArrangeChildren(finalRect);
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

    #endregion

    #region Hit test

    /// <summary>
    /// Find the topmost control that contains the point (in design resolution). Uses MouseFilter and Visible.
    /// </summary>
    public Control? HitTest(Vector2 pointInScreenSpace)
    {
        return HitTestRecursive(this, pointInScreenSpace);
    }

    /// <param name="pointInLocalSpace">Point in this control's local space (origin at top-left of this control).</param>
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

    #region Focus

    /// <summary>
    /// Currently focused control, or null. Set by GrabFocus(Control).
    /// </summary>
    public Control? GetFocused() => _focusedControl;

    /// <summary>
    /// Set focus to the given control. Only has effect if the control is in this tree and has FocusMode != None.
    /// </summary>
    public void GrabFocus(Control? control)
    {
        if (control != null && control.FocusMode == FocusMode.None) return;
        if (control != null && !IsDescendantOfThis(control)) return;
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

    private bool IsDescendantOfThis(Control control)
    {
        for (var n = control.Parent; n != null; n = n.Parent)
        {
            if (n == this) return true;
        }
        return false;
    }

    /// <summary>
    /// Collect all focusable controls in depth-first order (same as tree order).
    /// </summary>
    private static void CollectFocusable(Control c, List<Control> list)
    {
        if (!c.Visible || c.FocusMode == FocusMode.None) return;
        list.Add(c);
        foreach (var child in c.Children.OfType<Control>())
            CollectFocusable(child, list);
    }

    public Control? FindNextValidFocus()
    {
        var list = new List<Control>();
        CollectFocusable(this, list);
        if (list.Count == 0) return null;
        int idx = _focusedControl != null ? list.IndexOf(_focusedControl) : -1;
        return list[(idx + 1) % list.Count];
    }

    public Control? FindPrevValidFocus()
    {
        var list = new List<Control>();
        CollectFocusable(this, list);
        if (list.Count == 0) return null;
        int idx = _focusedControl != null ? list.IndexOf(_focusedControl) : 0;
        idx = idx <= 0 ? list.Count - 1 : idx - 1;
        return list[idx];
    }

    #endregion

    #region Event routing (tunnel then bubble)

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

    #endregion

    #region Input processing

    private Vector2 _lastMousePosition;

    private void ProcessInputEvents()
    {
        var pos = Input.MouseScreenPosition;
        var hit = HitTest(pos);

        if (Input.IsLeftMouseButtonPressed())
        {
            if (hit != null && hit.FocusMode != FocusMode.None)
                hit.GrabFocus();
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Left,
                Pressed = true
            };
            RouteMouseButtonDown(hit, args);
        }
        if (Input.IsRightMouseButtonPressed())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Right,
                Pressed = true
            };
            RouteMouseButtonDown(hit, args);
        }

        if (Input.IsLeftMouseButtonReleased())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Left,
                Pressed = false
            };
            RouteMouseButtonUp(hit, args);
        }
        if (Input.IsRightMouseButtonReleased())
        {
            var args = new MouseButtonEventArgs
            {
                Position = pos,
                Button = MouseButton.Right,
                Pressed = false
            };
            RouteMouseButtonUp(hit, args);
        }

        if (pos != _lastMousePosition)
        {
            var moveArgs = new MouseMoveEventArgs
            {
                Position = pos,
                PreviousPosition = _lastMousePosition
            };
            RouteMouseMove(hit, moveArgs);
            _lastMousePosition = pos;
        }

        ProcessKeyboardEvents();
    }

    private void ProcessKeyboardEvents()
    {
        var focus = _focusedControl;
        if (focus == null) return;

        if (Input.IsKeyPressed(Keys.Tab))
        {
            var next = Input.IsKeyDown(Keys.LeftShift) || Input.IsKeyDown(Keys.RightShift)
                ? FindPrevValidFocus()
                : FindNextValidFocus();
            if (next != null)
                GrabFocus(next);
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
}
