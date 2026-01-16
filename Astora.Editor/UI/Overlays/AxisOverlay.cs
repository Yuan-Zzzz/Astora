using Astora.Core.Scene;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.UI.Overlays;

/// <summary>
/// 坐标轴覆盖层，绘制X轴（红色）和Y轴（绿色）
/// </summary>
public class AxisOverlay : ISceneViewOverlay
{
    private readonly GizmoRenderer _gizmoRenderer;
    private const float MinLineThickness = 1f;
    
    public bool Enabled { get; set; } = true;
    public int RenderOrder => 1; // 在网格之上，场景内容之下
    
    public AxisOverlay(GizmoRenderer gizmoRenderer)
    {
        _gizmoRenderer = gizmoRenderer;
    }
    
    public void Draw(SpriteBatch spriteBatch, SceneViewCamera camera, SceneTree sceneTree, int viewportWidth, int viewportHeight)
    {
        if (!Enabled) return;
        
        // 获取视口在世界坐标系中的边界
        var bounds = camera.GetViewportBounds(viewportWidth, viewportHeight);
        
        // 扩展范围以确保坐标轴覆盖整个视口
        var padding = 100f;
        var worldMinX = bounds.X - padding;
        var worldMaxX = bounds.X + bounds.Width + padding;
        var worldMinY = bounds.Y - padding;
        var worldMaxY = bounds.Y + bounds.Height + padding;
        
        // 坐标轴线宽
        var axisThickness = Math.Max(1.5f / camera.Zoom, MinLineThickness);
        var originThickness = Math.Max(4f / camera.Zoom, MinLineThickness * 2f); // 原点处加粗
        
        // 绘制X轴（红色）
        if (worldMinY <= 0 && worldMaxY >= 0)
        {
            // 检查原点是否在视口内
            bool originVisible = (worldMinX <= 0 && worldMaxX >= 0);
            
            if (originVisible)
            {
                // 原点左侧
                if (worldMinX < 0)
                {
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(worldMinX, 0),
                        new XnaVector2(0, 0),
                        Color.Red,
                        axisThickness
                    );
                }
                
                // 原点处加粗
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(-5f / camera.Zoom, 0),
                    new XnaVector2(5f / camera.Zoom, 0),
                    Color.Red,
                    originThickness
                );
                
                // 原点右侧
                if (worldMaxX > 0)
                {
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(0, 0),
                        new XnaVector2(worldMaxX, 0),
                        Color.Red,
                        axisThickness
                    );
                }
            }
            else
            {
                // 原点不在视口内，绘制整条线
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(worldMinX, 0),
                    new XnaVector2(worldMaxX, 0),
                    Color.Red,
                    axisThickness
                );
            }
        }
        
        // 绘制Y轴（绿色）
        if (worldMinX <= 0 && worldMaxX >= 0)
        {
            // 检查原点是否在视口内
            bool originVisible = (worldMinY <= 0 && worldMaxY >= 0);
            
            if (originVisible)
            {
                // 原点下方
                if (worldMinY < 0)
                {
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(0, worldMinY),
                        new XnaVector2(0, 0),
                        Color.Green,
                        axisThickness
                    );
                }
                
                // 原点处加粗
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(0, -5f / camera.Zoom),
                    new XnaVector2(0, 5f / camera.Zoom),
                    Color.Green,
                    originThickness
                );
                
                // 原点上方的Y轴
                if (worldMaxY > 0)
                {
                    _gizmoRenderer.DrawLine(
                        spriteBatch,
                        new XnaVector2(0, 0),
                        new XnaVector2(0, worldMaxY),
                        Color.Green,
                        axisThickness
                    );
                }
            }
            else
            {
                // 原点不在视口内，绘制整条线
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(0, worldMinY),
                    new XnaVector2(0, worldMaxY),
                    Color.Green,
                    axisThickness
                );
            }
        }
    }
}
