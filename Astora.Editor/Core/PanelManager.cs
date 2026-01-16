using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Astora.Editor.Core;

/// <summary>
/// 面板管理器 - 统一管理所有UI面板
/// </summary>
public class PanelManager
{
    private readonly Dictionary<string, IPanel> _panels = new Dictionary<string, IPanel>();
    
    /// <summary>
    /// 注册面板
    /// </summary>
    public void RegisterPanel(IPanel panel)
    {
        _panels[panel.Name] = panel;
    }
    
    /// <summary>
    /// 注销面板
    /// </summary>
    public void UnregisterPanel(string name)
    {
        _panels.Remove(name);
    }
    
    /// <summary>
    /// 获取面板
    /// </summary>
    public T? GetPanel<T>(string name) where T : class, IPanel
    {
        if (_panels.TryGetValue(name, out var panel))
        {
            return panel as T;
        }
        return null;
    }
    
    /// <summary>
    /// 更新所有可见的面板
    /// </summary>
    public void Update(GameTime gameTime)
    {
        foreach (var panel in _panels.Values)
        {
            if (panel.IsVisible)
            {
                panel.Update(gameTime);
            }
        }
    }
    
    /// <summary>
    /// 渲染所有可见的面板
    /// </summary>
    public void Render()
    {
        foreach (var panel in _panels.Values)
        {
            if (panel.IsVisible)
            {
                panel.Render();
            }
        }
    }
    
    /// <summary>
    /// 获取所有面板
    /// </summary>
    public IEnumerable<IPanel> GetAllPanels()
    {
        return _panels.Values;
    }
}
