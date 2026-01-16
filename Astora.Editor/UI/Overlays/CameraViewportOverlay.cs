using Astora.Core;
using Astora.Core.Nodes;
using Astora.Core.Scene;
using Astora.Editor;
using Astora.Editor.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Astora.Editor.UI.Overlays;

/// <summary>
/// 相机视口覆盖层，显示场景中所有相机的视口区域
/// </summary>
public class CameraViewportOverlay : ISceneViewOverlay
{
    private readonly GizmoRenderer _gizmoRenderer;
    private readonly Editor _editor;
    
    public bool Enabled { get; set; } = true;
    public int RenderOrder => 3; // 在场景内容之上
    
    public CameraViewportOverlay(GizmoRenderer gizmoRenderer, Editor editor)
    {
        _gizmoRenderer = gizmoRenderer;
        _editor = editor;
    }
    
    public void Draw(SpriteBatch spriteBatch, SceneViewCamera camera, SceneTree sceneTree, int viewportWidth, int viewportHeight)
    {
        if (!Enabled) return;
        
        var cameras = GetAllCameras(sceneTree);
        if (cameras.Count == 0) return;
        
        var activeCamera = sceneTree.ActiveCamera;
        
        foreach (var cam in cameras)
        {
            var bounds = GetCameraViewportBounds(cam, _editor);
            var isActive = cam == activeCamera;
            
            // ActiveCamera 用绿色，其他用黄色
            var color = isActive ? new Color(0, 255, 0, 200) : new Color(255, 255, 0, 150);
            var thickness = isActive ? 3f / camera.Zoom : 2f / camera.Zoom;
            
            // 绘制视口边框
            _gizmoRenderer.DrawRectangle(spriteBatch, bounds, color, thickness);
            
            // 在相机位置绘制一个小标记
            var cameraPos = cam.GlobalPosition;
            var markerSize = 8f / camera.Zoom;
            _gizmoRenderer.DrawCircle(spriteBatch, cameraPos, markerSize, color, thickness);
        }
    }
    
    /// <summary>
    /// 递归查找场景中所有的 Camera2D 节点
    /// </summary>
    private List<Camera2D> GetAllCameras(SceneTree sceneTree)
    {
        var cameras = new List<Camera2D>();
        FindCameras(sceneTree.Root, cameras);
        return cameras;
    }
    
    /// <summary>
    /// 递归查找相机节点
    /// </summary>
    private void FindCameras(Node? node, List<Camera2D> cameras)
    {
        if (node == null) return;
        
        if (node is Camera2D camera)
        {
            cameras.Add(camera);
        }
        
        foreach (var child in node.Children)
        {
            FindCameras(child, cameras);
        }
    }
    
    /// <summary>
    /// 计算相机的世界坐标视口边界
    /// </summary>
    private RectangleF GetCameraViewportBounds(Camera2D camera, Editor editor)
    {
        // 获取设计分辨率（从项目配置）
        int designWidth = 1920;
        int designHeight = 1080;
        
        var projectManager = editor.ProjectManager;
        if (projectManager?.CurrentProject?.GameConfig != null)
        {
            var config = projectManager.CurrentProject.GameConfig;
            designWidth = config.DesignWidth;
            designHeight = config.DesignHeight;
        }
        else if (Engine.GDM?.GraphicsDevice != null)
        {
            // 如果没有项目配置，使用 GraphicsDevice 的视口大小
            var viewport = Engine.GDM.GraphicsDevice.Viewport;
            designWidth = viewport.Width;
            designHeight = viewport.Height;
        }
        
        // 计算视口边界：
        // 相机的Origin表示相机Position在屏幕上的显示位置
        // 为了正确显示游戏视口，我们应该使用设计分辨率的中心作为Origin
        var expectedOrigin = new XnaVector2(designWidth / 2f, designHeight / 2f);
        
        // 手动计算视口边界，使用正确的Origin值
        // 视口大小（考虑Zoom）
        var viewportWidth = designWidth / camera.Zoom;
        var viewportHeight = designHeight / camera.Zoom;
        
        // 视口左上角 = 相机位置 - (期望的Origin / Zoom)
        var topLeft = camera.GlobalPosition - expectedOrigin / camera.Zoom;
        
        return new RectangleF(
            topLeft.X,
            topLeft.Y,
            viewportWidth,
            viewportHeight
        );
    }
}
