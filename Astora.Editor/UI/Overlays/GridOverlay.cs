using Astora.Core.Scene;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.UI.Overlays;

/// <summary>
/// 网格覆盖层，在场景视图上绘制辅助网格
/// </summary>
public class GridOverlay : ISceneViewOverlay
{
    private readonly GizmoRenderer _gizmoRenderer;
    private const float MinLineThickness = 1f;
    
    public bool Enabled { get; set; } = true;
    public int RenderOrder => 0; // 最先渲染，在场景内容之下
    
    public GridOverlay(GizmoRenderer gizmoRenderer)
    {
        _gizmoRenderer = gizmoRenderer;
    }
    
    public void Draw(SpriteBatch spriteBatch, SceneViewCamera camera, SceneTree sceneTree, int viewportWidth, int viewportHeight)
    {
        if (!Enabled) return;
        
        // 获取视口在世界坐标系中的边界
        var bounds = camera.GetViewportBounds(viewportWidth, viewportHeight);
        
        // 扩展范围以确保网格覆盖整个视口
        var padding = 100f;
        var worldMinX = bounds.X - padding;
        var worldMaxX = bounds.X + bounds.Width + padding;
        var worldMinY = bounds.Y - padding;
        var worldMaxY = bounds.Y + bounds.Height + padding;
        
        // 网格间距
        const float minorGridSpacing = 50f;
        const float majorGridSpacing = 100f;
        
        // 根据缩放级别决定是否绘制次网格
        bool drawMinorGrid = camera.Zoom > 0.3f;
        
        // 计算网格线的起始位置（对齐到网格）
        var startX = (float)(Math.Floor(worldMinX / minorGridSpacing) * minorGridSpacing);
        var startY = (float)(Math.Floor(worldMinY / minorGridSpacing) * minorGridSpacing);
        
        // 绘制次网格线（50像素间距）
        if (drawMinorGrid)
        {
            var minorColor = new Color(128, 128, 128, 100);
            var minorThickness = Math.Max(1f / camera.Zoom, MinLineThickness);
            
            // 垂直线
            for (float x = startX; x <= worldMaxX; x += minorGridSpacing)
            {
                // 跳过主网格线位置
                if (Math.Abs(x % majorGridSpacing) < 0.1f)
                    continue;
                
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(x, worldMinY),
                    new XnaVector2(x, worldMaxY),
                    minorColor,
                    minorThickness
                );
            }
            
            // 水平线
            for (float y = startY; y <= worldMaxY; y += minorGridSpacing)
            {
                // 跳过主网格线位置
                if (Math.Abs(y % majorGridSpacing) < 0.1f)
                    continue;
                
                _gizmoRenderer.DrawLine(
                    spriteBatch,
                    new XnaVector2(worldMinX, y),
                    new XnaVector2(worldMaxX, y),
                    minorColor,
                    minorThickness
                );
            }
        }
        
        // 绘制主网格线（100像素间距）
        var majorColor = new Color(100, 100, 100, 150);
        var majorThickness = Math.Max(1f / camera.Zoom, MinLineThickness);
        
        var majorStartX = (float)(Math.Floor(worldMinX / majorGridSpacing) * majorGridSpacing);
        var majorStartY = (float)(Math.Floor(worldMinY / majorGridSpacing) * majorGridSpacing);
        
        // 垂直线
        for (float x = majorStartX; x <= worldMaxX; x += majorGridSpacing)
        {
            _gizmoRenderer.DrawLine(
                spriteBatch,
                new XnaVector2(x, worldMinY),
                new XnaVector2(x, worldMaxY),
                majorColor,
                majorThickness
            );
        }
        
        // 水平线
        for (float y = majorStartY; y <= worldMaxY; y += majorGridSpacing)
        {
            _gizmoRenderer.DrawLine(
                spriteBatch,
                new XnaVector2(worldMinX, y),
                new XnaVector2(worldMaxX, y),
                majorColor,
                majorThickness
            );
        }
    }
}
