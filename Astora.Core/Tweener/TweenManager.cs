using System.Collections.Generic;

namespace Astora.Core.Tweener;

/// <summary>
/// Tweener Updater
/// </summary>
public static class TweenCore
{
    private static readonly List<ITween> _tweens = new();
    private static readonly object _lock = new object();

    /// <summary>
    /// Add a new Tween
    /// </summary>
    public static void Add(ITween tween)
    {
        if (tween != null)
        {
            lock (_lock)
            {
                _tweens.Add(tween);
            }
        }
    }

    /// <summary>
    /// Drives all Tweens, deltaTime is the elapsed time for the current frame.
    /// </summary>
    public static void Update(float deltaTime)
    {
        lock (_lock)
        {
            for (int i = _tweens.Count - 1; i >= 0; i--)
            {
                if (_tweens[i] == null)
                {
                    _tweens.RemoveAt(i);
                    continue;
                }
                
                if (_tweens[i].Update(deltaTime))
                    _tweens.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Clears all Tweens (can be called on scene transition).
    /// </summary>
    public static void Clear()
    {
        lock (_lock)
        {
            _tweens.Clear();
        }
    }
}
