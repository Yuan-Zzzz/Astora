using Astora.Core.Nodes;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using ImGuiNET;
using System.Numerics;
using Vector2 = System.Numerics.Vector2;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace Astora.Editor.UI;

/// <summary>
/// 场景视图相机，管理编辑器视图的平移和缩放
/// </summary>
public class SceneViewCamera
{
    private XnaVector2 _position;
    private float _zoom = 1.0f;
    
    /// <summary>
    /// 相机位置（世界坐标）
    /// </summary>
    public XnaVector2 Position
    {
        get => _position;
        set => _position = value;
    }
    
    /// <summary>
    /// 相机缩放级别
    /// </summary>
    public float Zoom
    {
        get => _zoom;
        set => _zoom = MathHelper.Clamp(value, 0.1f, 10f);
    }
    
    /// <summary>
    /// 获取视图变换矩阵
    /// </summary>
    public Matrix GetViewMatrix()
    {
        return Matrix.CreateTranslation(new XnaVector3(-_position, 0)) *
               Matrix.CreateScale(_zoom);
    }
    
    /// <summary>
    /// 将屏幕坐标转换为世界坐标
    /// 注意：此方法只能在ImGui窗口打开时调用（即在RenderUI中，ImGui.Begin()之后）
    /// </summary>
    public XnaVector2 ScreenToWorld(Vector2 screenPos)
    {
        // 获取窗口内容区域的位置
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        
        // 计算相对于内容区域的坐标
        var localPos = new XnaVector2(
            screenPos.X - cursorScreenPos.X,
            screenPos.Y - cursorScreenPos.Y
        );
        
        // 应用相机变换的逆变换
        Matrix viewMatrix = GetViewMatrix();
        Matrix.Invert(ref viewMatrix, out var invViewMatrix);
        
        return XnaVector2.Transform(localPos, invViewMatrix);
    }
    
    /// <summary>
    /// 将世界坐标转换为屏幕坐标
    /// 注意：此方法只能在ImGui窗口打开时调用（即在RenderUI中，ImGui.Begin()之后）
    /// </summary>
    public Vector2 WorldToScreen(XnaVector2 worldPos)
    {
        // 应用相机变换
        Matrix viewMatrix = GetViewMatrix();
        var screenPos = XnaVector2.Transform(worldPos, viewMatrix);
        
        // 转换为窗口坐标
        var cursorScreenPos = ImGui.GetCursorScreenPos();
        
        return new Vector2(
            screenPos.X + cursorScreenPos.X,
            screenPos.Y + cursorScreenPos.Y
        );
    }
    
    /// <summary>
    /// 平移相机
    /// </summary>
    public void Pan(XnaVector2 delta)
    {
        _position -= delta / _zoom;
    }
    
    /// <summary>
    /// 缩放相机
    /// </summary>
    public void ZoomIn(float factor = 1.1f)
    {
        _zoom *= factor;
        _zoom = MathHelper.Clamp(_zoom, 0.1f, 10f);
    }
    
    /// <summary>
    /// 缩小相机
    /// </summary>
    public void ZoomOut(float factor = 1.1f)
    {
        _zoom /= factor;
        _zoom = MathHelper.Clamp(_zoom, 0.1f, 10f);
    }
    
    /// <summary>
    /// 获取当前视口在世界坐标系中的边界
    /// </summary>
    public RectangleF GetViewportBounds(int viewportWidth, int viewportHeight)
    {
        // 屏幕(0,0)对应的世界坐标
        var worldTopLeft = new XnaVector2(
            0 / _zoom + _position.X,
            0 / _zoom + _position.Y
        );
        // 屏幕(viewportWidth, viewportHeight)对应的世界坐标
        var worldBottomRight = new XnaVector2(
            viewportWidth / _zoom + _position.X,
            viewportHeight / _zoom + _position.Y
        );
        
        var worldMinX = Math.Min(worldTopLeft.X, worldBottomRight.X);
        var worldMaxX = Math.Max(worldTopLeft.X, worldBottomRight.X);
        var worldMinY = Math.Min(worldTopLeft.Y, worldBottomRight.Y);
        var worldMaxY = Math.Max(worldTopLeft.Y, worldBottomRight.Y);
        
        return new RectangleF(
            worldMinX,
            worldMinY,
            worldMaxX - worldMinX,
            worldMaxY - worldMinY
        );
    }
}
