using System;
using System.Collections.Generic;
using System.Linq;

namespace Astora.Editor.Core;

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// 通知消息
/// </summary>
public class Notification
{
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public float Duration { get; set; } // 显示持续时间（秒）
    
    public Notification(string message, NotificationType type, float duration = 3.0f)
    {
        Message = message;
        Type = type;
        Timestamp = DateTime.Now;
        Duration = duration;
    }
    
    /// <summary>
    /// 检查通知是否已过期
    /// </summary>
    public bool IsExpired()
    {
        return (DateTime.Now - Timestamp).TotalSeconds > Duration;
    }
}

/// <summary>
/// 通知管理器 - 管理编辑器中的所有通知消息
/// </summary>
public class NotificationManager
{
    private readonly List<Notification> _notifications = new();
    private readonly int _maxNotifications = 5; // 最多显示的通知数量
    
    /// <summary>
    /// 获取所有活动的通知
    /// </summary>
    public IReadOnlyList<Notification> ActiveNotifications => _notifications;
    
    /// <summary>
    /// 添加信息通知
    /// </summary>
    public void ShowInfo(string message, float duration = 3.0f)
    {
        AddNotification(message, NotificationType.Info, duration);
    }
    
    /// <summary>
    /// 添加成功通知
    /// </summary>
    public void ShowSuccess(string message, float duration = 3.0f)
    {
        AddNotification(message, NotificationType.Success, duration);
    }
    
    /// <summary>
    /// 添加警告通知
    /// </summary>
    public void ShowWarning(string message, float duration = 4.0f)
    {
        AddNotification(message, NotificationType.Warning, duration);
    }
    
    /// <summary>
    /// 添加错误通知
    /// </summary>
    public void ShowError(string message, float duration = 5.0f)
    {
        AddNotification(message, NotificationType.Error, duration);
    }
    
    /// <summary>
    /// 添加通知
    /// </summary>
    private void AddNotification(string message, NotificationType type, float duration)
    {
        var notification = new Notification(message, type, duration);
        _notifications.Add(notification);
        
        // 如果通知数量超过最大值，移除最旧的
        while (_notifications.Count > _maxNotifications)
        {
            _notifications.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 更新通知（清除过期的通知）
    /// </summary>
    public void Update()
    {
        // 移除所有过期的通知
        _notifications.RemoveAll(n => n.IsExpired());
    }
    
    /// <summary>
    /// 清除所有通知
    /// </summary>
    public void Clear()
    {
        _notifications.Clear();
    }
}
