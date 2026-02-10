using Astora.Core;
using Astora.Core.Nodes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.Tools;

/// <summary>
/// Gizmo 渲染器 - Godot 风格，负责绘制所有 Gizmo 相关的图形
/// </summary>
public class GizmoRenderer
{
    private Texture2D? _whitePixelTexture;

    // Gizmo 尺寸常量
    private const float ArrowLength = 60f;
    private const float ArrowHeadSize = 10f;
    private const float ArrowLineWidth = 2.5f;
    private const float CenterSquareSize = 6f;
    private const float RotateRadius = 60f;
    private const float HandleDotSize = 5f;

    // Godot 风格颜色
    public static readonly Color AxisXColor = new Color(237, 75, 75);       // 红色 X 轴
    public static readonly Color AxisXHover = new Color(255, 120, 120);     // 红色悬停
    public static readonly Color AxisYColor = new Color(100, 190, 70);      // 绿色 Y 轴
    public static readonly Color AxisYHover = new Color(140, 220, 110);     // 绿色悬停
    public static readonly Color RotateColor = new Color(105, 156, 232);    // 蓝色旋转环
    public static readonly Color RotateHover = new Color(140, 185, 255);    // 蓝色悬停
    public static readonly Color CenterColor = Color.White;
    public static readonly Color SelectionBoxColor = new Color((byte)105, (byte)156, (byte)232, (byte)100);

    /// <summary>
    /// 创建白色像素纹理
    /// </summary>
    private Texture2D GetWhitePixelTexture()
    {
        if (_whitePixelTexture == null || _whitePixelTexture.IsDisposed)
        {
            _whitePixelTexture = new Texture2D(Engine.GDM.GraphicsDevice, 1, 1);
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
        if (length < 0.001f) return;
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
    /// 绘制填充三角形（用于箭头头部）
    /// </summary>
    public void DrawFilledTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        // 用多条线填充三角形（简单但有效的方法）
        var whitePixel = GetWhitePixelTexture();

        // 插值填充三角形
        int steps = 12;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            var left = Vector2.Lerp(p1, p2, t);
            var right = Vector2.Lerp(p1, p3, t);
            DrawLine(spriteBatch, left, right, color, 1.5f);
        }
    }

    /// <summary>
    /// 绘制箭头（Godot 风格：线段 + 三角形箭头头）
    /// </summary>
    public void DrawArrow(SpriteBatch spriteBatch, Vector2 origin, Vector2 direction, float length, float headSize, Color color, float lineWidth, float cameraZoom)
    {
        var scaledLength = length / cameraZoom;
        var scaledHeadSize = headSize / cameraZoom;
        var scaledLineWidth = lineWidth / cameraZoom;

        var dir = direction;
        if (dir.LengthSquared() > 0)
            dir.Normalize();

        var tip = origin + dir * scaledLength;
        var lineEnd = origin + dir * (scaledLength - scaledHeadSize);

        // 绘制线段（从原点到箭头底部）
        DrawLine(spriteBatch, origin, lineEnd, color, scaledLineWidth);

        // 绘制三角形箭头头
        var perp = new Vector2(-dir.Y, dir.X);
        var arrowBase1 = lineEnd + perp * scaledHeadSize * 0.5f;
        var arrowBase2 = lineEnd - perp * scaledHeadSize * 0.5f;
        DrawFilledTriangle(spriteBatch, tip, arrowBase1, arrowBase2, color);
    }

    /// <summary>
    /// 绘制填充正方形
    /// </summary>
    public void DrawFilledSquare(SpriteBatch spriteBatch, Vector2 center, float halfSize, Color color)
    {
        var whitePixel = GetWhitePixelTexture();
        spriteBatch.Draw(
            whitePixel,
            new Vector2(center.X - halfSize, center.Y - halfSize),
            null,
            color,
            0f,
            Vector2.Zero,
            new Vector2(halfSize * 2, halfSize * 2),
            SpriteEffects.None,
            0f
        );
    }

    /// <summary>
    /// 绘制圆形（使用多个线段近似）
    /// </summary>
    public void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, float thickness = 1f)
    {
        const int segments = 48;
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
    /// 绘制弧线（用于旋转时显示角度）
    /// </summary>
    public void DrawArc(SpriteBatch spriteBatch, Vector2 center, float radius, float startAngle, float endAngle, Color color, float thickness = 1f)
    {
        const int segments = 32;
        float angleDiff = endAngle - startAngle;
        float angleStep = angleDiff / segments;

        for (int i = 0; i < segments; i++)
        {
            var a1 = startAngle + i * angleStep;
            var a2 = startAngle + (i + 1) * angleStep;

            var start = center + new Vector2((float)Math.Cos(a1) * radius, (float)Math.Sin(a1) * radius);
            var end = center + new Vector2((float)Math.Cos(a2) * radius, (float)Math.Sin(a2) * radius);

            DrawLine(spriteBatch, start, end, color, thickness);
        }
    }

    /// <summary>
    /// 绘制移动 Gizmo（Godot 风格：带箭头的 XY 轴 + 中心正方形）
    /// </summary>
    public void DrawMoveGizmo(SpriteBatch spriteBatch, Node2D node, float cameraZoom,
        bool hoverX = false, bool hoverY = false, bool hoverCenter = false)
    {
        var worldPos = node.GlobalPosition;
        var centerSize = CenterSquareSize / cameraZoom;

        // X 轴箭头（红色，向右）
        var xColor = hoverX ? AxisXHover : AxisXColor;
        var xLineWidth = hoverX ? ArrowLineWidth * 1.3f : ArrowLineWidth;
        DrawArrow(spriteBatch, worldPos, Vector2.UnitX, ArrowLength, ArrowHeadSize, xColor, xLineWidth, cameraZoom);

        // Y 轴箭头（绿色，向下）
        var yColor = hoverY ? AxisYHover : AxisYColor;
        var yLineWidth = hoverY ? ArrowLineWidth * 1.3f : ArrowLineWidth;
        DrawArrow(spriteBatch, worldPos, Vector2.UnitY, ArrowLength, ArrowHeadSize, yColor, yLineWidth, cameraZoom);

        // 中心正方形（白色，可自由拖拽）
        var cColor = hoverCenter ? new Color(255, 255, 255, 200) : new Color(255, 255, 255, 150);
        DrawFilledSquare(spriteBatch, worldPos, centerSize, cColor);
    }

    /// <summary>
    /// 绘制旋转 Gizmo（Godot 风格：蓝色圆环 + 方向标记 + 可选弧线）
    /// </summary>
    public void DrawRotateGizmo(SpriteBatch spriteBatch, Node2D node, float cameraZoom,
        bool hover = false, bool isDragging = false, float dragStartAngle = 0f, float currentAngle = 0f)
    {
        var worldPos = node.GlobalPosition;
        var radius = RotateRadius / cameraZoom;
        var thickness = (hover ? 3f : 2f) / cameraZoom;
        var dotSize = HandleDotSize / cameraZoom;

        var color = hover ? RotateHover : RotateColor;

        // 绘制圆环
        DrawCircle(spriteBatch, worldPos, radius, color, thickness);

        // 方向标记：在当前旋转角度处画一个小圆点
        var handleAngle = node.Rotation;
        if (node.Parent is Node2D parent2d)
            handleAngle += parent2d.Rotation;

        var handlePos = worldPos + new Vector2(
            (float)Math.Cos(handleAngle) * radius,
            (float)Math.Sin(handleAngle) * radius
        );
        DrawCircle(spriteBatch, handlePos, dotSize, color, thickness * 0.8f);

        // 拖动时显示角度弧线
        if (isDragging)
        {
            var arcColor = new Color((int)color.R, (int)color.G, (int)color.B, 80);
            DrawArc(spriteBatch, worldPos, radius * 0.7f, dragStartAngle, currentAngle, arcColor, thickness * 1.5f);
        }
    }

    /// <summary>
    /// 绘制选中节点的包围盒（淡蓝色虚线框）
    /// </summary>
    public void DrawSelectionBox(SpriteBatch spriteBatch, Node2D node, float cameraZoom)
    {
        // 如果是 Sprite，使用其纹理尺寸；否则画一个默认大小的框
        float width = 32f;
        float height = 32f;
        var origin = Vector2.Zero;

        if (node is Sprite sprite && sprite.Texture != null)
        {
            width = sprite.Texture.Width * Math.Abs(node.Scale.X);
            height = sprite.Texture.Height * Math.Abs(node.Scale.Y);
            origin = sprite.Origin * node.Scale;
        }

        var pos = node.GlobalPosition;
        var thickness = 1f / cameraZoom;

        var topLeft = new Vector2(pos.X - origin.X, pos.Y - origin.Y);
        var topRight = new Vector2(pos.X - origin.X + width, pos.Y - origin.Y);
        var bottomLeft = new Vector2(pos.X - origin.X, pos.Y - origin.Y + height);
        var bottomRight = new Vector2(pos.X - origin.X + width, pos.Y - origin.Y + height);

        DrawLine(spriteBatch, topLeft, topRight, SelectionBoxColor, thickness);
        DrawLine(spriteBatch, topRight, bottomRight, SelectionBoxColor, thickness);
        DrawLine(spriteBatch, bottomRight, bottomLeft, SelectionBoxColor, thickness);
        DrawLine(spriteBatch, bottomLeft, topLeft, SelectionBoxColor, thickness);
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

        DrawLine(spriteBatch, topLeft, topRight, color, thickness);
        DrawLine(spriteBatch, topRight, bottomRight, color, thickness);
        DrawLine(spriteBatch, bottomRight, bottomLeft, color, thickness);
        DrawLine(spriteBatch, bottomLeft, topLeft, color, thickness);
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
