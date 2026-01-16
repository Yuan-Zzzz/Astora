using Astora.Core.Scene;
using Microsoft.Xna.Framework.Graphics;

namespace Astora.Editor.UI;

/// <summary>
/// 场景视图覆盖层接口，用于在场景视图上绘制辅助内容（网格、坐标轴、Gizmo等）
/// </summary>
public interface ISceneViewOverlay
{
    /// <summary>
    /// 是否启用此覆盖层
    /// </summary>
    bool Enabled { get; set; }
    
    /// <summary>
    /// 渲染顺序，数值越小越先渲染（在场景内容之下）
    /// </summary>
    int RenderOrder { get; }
    
    /// <summary>
    /// 绘制覆盖层内容
    /// </summary>
    /// <param name="spriteBatch">用于绘制的 SpriteBatch</param>
    /// <param name="camera">场景视图相机</param>
    /// <param name="sceneTree">场景树</param>
    /// <param name="viewportWidth">视口宽度</param>
    /// <param name="viewportHeight">视口高度</param>
    void Draw(SpriteBatch spriteBatch, SceneViewCamera camera, SceneTree sceneTree, int viewportWidth, int viewportHeight);
}
