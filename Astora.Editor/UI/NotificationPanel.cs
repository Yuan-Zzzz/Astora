using Astora.Editor.Core;
using ImGuiNET;
using System.Numerics;

namespace Astora.Editor.UI;

/// <summary>
/// 通知面板 - 显示编辑器通知消息
/// </summary>
public class NotificationPanel
{
    private readonly NotificationManager _notificationManager;
    private const float NotificationWidth = 400f;
    private const float NotificationHeight = 60f;
    private const float Padding = 10f;
    
    public NotificationPanel(NotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }
    
    /// <summary>
    /// 渲染通知面板
    /// </summary>
    public void Render()
    {
        // 更新通知管理器（清理过期通知）
        _notificationManager.Update();
        
        var notifications = _notificationManager.ActiveNotifications;
        if (notifications.Count == 0)
        {
            return;
        }
        
        // 获取主视口大小
        var viewport = ImGui.GetMainViewport();
        var viewportSize = viewport.Size;
        
        // 在右下角显示通知
        var yOffset = viewportSize.Y - Padding;
        
        for (int i = notifications.Count - 1; i >= 0; i--)
        {
            var notification = notifications[i];
            
            // 计算通知位置
            var posX = viewportSize.X - NotificationWidth - Padding;
            var posY = yOffset - NotificationHeight;
            
            // 设置窗口位置和大小
            ImGui.SetNextWindowPos(new Vector2(posX, posY));
            ImGui.SetNextWindowSize(new Vector2(NotificationWidth, NotificationHeight));
            
            // 根据通知类型设置颜色
            var bgColor = GetBackgroundColor(notification.Type);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);
            
            // 创建唯一的窗口ID
            var windowName = $"##Notification_{i}";
            
            // 无标题栏、无边框、无滚动条、无调整大小
            var flags = ImGuiWindowFlags.NoTitleBar | 
                       ImGuiWindowFlags.NoResize | 
                       ImGuiWindowFlags.NoMove | 
                       ImGuiWindowFlags.NoScrollbar |
                       ImGuiWindowFlags.NoSavedSettings |
                       ImGuiWindowFlags.NoFocusOnAppearing |
                       ImGuiWindowFlags.NoNav;
            
            if (ImGui.Begin(windowName, flags))
            {
                // 显示图标和消息
                var icon = GetIcon(notification.Type);
                ImGui.Text(icon);
                ImGui.SameLine();
                
                // 文字换行显示
                ImGui.PushTextWrapPos(ImGui.GetContentRegionAvail().X);
                ImGui.TextWrapped(notification.Message);
                ImGui.PopTextWrapPos();
                
                ImGui.End();
            }
            
            ImGui.PopStyleColor();
            
            // 更新 Y 偏移，为下一个通知留出空间
            yOffset -= NotificationHeight + Padding;
        }
    }
    
    /// <summary>
    /// 根据通知类型获取背景颜色
    /// </summary>
    private Vector4 GetBackgroundColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => new Vector4(0.2f, 0.4f, 0.8f, 0.95f),      // 蓝色
            NotificationType.Success => new Vector4(0.2f, 0.7f, 0.3f, 0.95f),   // 绿色
            NotificationType.Warning => new Vector4(0.9f, 0.7f, 0.2f, 0.95f),   // 黄色
            NotificationType.Error => new Vector4(0.8f, 0.2f, 0.2f, 0.95f),     // 红色
            _ => new Vector4(0.3f, 0.3f, 0.3f, 0.95f)
        };
    }
    
    /// <summary>
    /// 根据通知类型获取图标
    /// </summary>
    private string GetIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "ℹ️",
            NotificationType.Success => "✓",
            NotificationType.Warning => "⚠️",
            NotificationType.Error => "✗",
            _ => "•"
        };
    }
}
