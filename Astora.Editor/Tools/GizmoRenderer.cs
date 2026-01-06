using Astora.Core;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// Gizmo 渲染器，负责绘制所有 Gizmo 相关的图形
/// </summary>
public class GizmoRenderer
{
    private Texture2D? _whitePixelTexture;
    private const float GizmoHandleSize = 8f;
    private const float GizmoLineLength = 50f;
    
    /// <summary>
    /// 创建白色像素纹理用于绘制Gizmo
    /// </summary>
    private Texture2D GetWhitePixelTexture()
    {
        if (_whitePixelTexture == null || _whitePixelTexture.IsDisposed)
        {
            _whitePixelTexture = new Texture2D(Engine.GraphicsDevice, 1, 1);
            _whitePixelTexture.SetData(new[] { Color.White });
        }
        return _whitePixelTexture;
    }
    
    /// <summary>
    /// 绘制线条
    /// </summary>
    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var whitePixel = GetWhitePixelTexture();
        var direction = end - start;
        var length = direction.Length();
        var angle = (float)Math.Atan2(direction.Y, direction.X);
        
        spriteBatch.Draw(
            whitePixel,
            start,
            null,
            color,
            angle,
            new Vector2(0, 0.5f),
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f
        );
    }
    
    /// <summary>
    /// 绘制圆形（使用多个线段近似）
    /// </summary>
    public void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float thickness = 1f)
    {
        const int segments = 32;
        var angleStep = MathHelper.TwoPi / segments;
        
        for (int i = 0; i < segments; i++)
        {
            var angle1 = i * angleStep;
            var angle2 = (i + 1) * angleStep;
            
            var start = center + new Vector2(
                (float)Math.Cos(angle1) * radius,
                (float)Math.Sin(angle1) * radius
            );
            var end = center + new Vector2(
                (float)Math.Cos(angle2) * radius,
                (float)Math.Sin(angle2) * radius
            );
            
            DrawLine(spriteBatch, start, end, color, thickness);
        }
    }
    
    /// <summary>
    /// 绘制移动 Gizmo（十字线）
    /// </summary>
    public void DrawMoveGizmo(SpriteBatch spriteBatch, Node2D node, float cameraZoom)
    {
        var worldPos = node.GlobalPosition;
        var lineLength = GizmoLineLength / cameraZoom;
        var handleSize = GizmoHandleSize / cameraZoom;
        
        // 水平线（红色）
        DrawLine(spriteBatch,
            new Vector2(worldPos.X - lineLength, worldPos.Y),
            new Vector2(worldPos.X + lineLength, worldPos.Y),
            Color.Red, 2f / cameraZoom);
        
        // 垂直线（绿色）
        DrawLine(spriteBatch,
            new Vector2(worldPos.X, worldPos.Y - lineLength),
            new Vector2(worldPos.X, worldPos.Y + lineLength),
            Color.Green, 2f / cameraZoom);
        
        // 中心点（白色）
        DrawCircle(spriteBatch, worldPos, handleSize, Color.White);
    }
    
    /// <summary>
    /// 绘制旋转 Gizmo（圆形和手柄）
    /// </summary>
    public void DrawRotateGizmo(SpriteBatch spriteBatch, Node2D node, float cameraZoom)
    {
        var worldPos = node.GlobalPosition;
        var radius = GizmoLineLength / cameraZoom;
        var handleSize = GizmoHandleSize / cameraZoom;
        
        // 绘制旋转圆形（黄色）
        DrawCircle(spriteBatch, worldPos, radius, Color.Yellow, 2f / cameraZoom);
        
        // 绘制旋转手柄
        var handleAngle = node.Rotation;
        if (node.Parent is Node2D parent2d)
        {
            handleAngle += parent2d.Rotation;
        }
        var handlePos = worldPos + new Vector2(
            (float)Math.Cos(handleAngle) * radius,
            (float)Math.Sin(handleAngle) * radius
        );
        DrawCircle(spriteBatch, handlePos, handleSize, Color.Yellow);
    }
    
    /// <summary>
    /// 绘制矩形边框
    /// </summary>
    public void DrawRectangle(SpriteBatch spriteBatch, RectangleF bounds, Color color, float thickness)
    {
        var topLeft = new Vector2(bounds.X, bounds.Y);
        var topRight = new Vector2(bounds.X + bounds.Width, bounds.Y);
        var bottomLeft = new Vector2(bounds.X, bounds.Y + bounds.Height);
        var bottomRight = new Vector2(bounds.X + bounds.Width, bounds.Y + bounds.Height);
        
        // 绘制四条边
        DrawLine(spriteBatch, topLeft, topRight, color, thickness);      // 上边
        DrawLine(spriteBatch, topRight, bottomRight, color, thickness);  // 右边
        DrawLine(spriteBatch, bottomRight, bottomLeft, color, thickness); // 下边
        DrawLine(spriteBatch, bottomLeft, topLeft, color, thickness);     // 左边
    }
}

/// <summary>
/// 简单的矩形结构用于碰撞检测和绘制
/// </summary>
public struct RectangleF
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    
    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public bool Contains(Vector2 point)
    {
        return point.X >= X && point.X <= X + Width &&
               point.Y >= Y && point.Y <= Y + Height;
    }
}
