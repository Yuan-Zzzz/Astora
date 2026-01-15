using System;

namespace Astora.Core.Tweener;

public static class TweenExtension
{
    /// <summary>
    /// Single-line float Tween example:
    /// Tween.Float(() => x, v => x = v, to: 10f, duration: 0.5f, ease: Ease.InOutQuad);
    /// </summary>
    public static ValueTweener<float> Float(
        Func<float> getter,
        Action<float> setter,
        float to,
        float duration,
        Func<float, float> ease = null)
    {
        float from = getter();
        return ValueTweener<float>.Create(from, to, duration)
            .SetEase(ease ?? Ease.Linear)
            .OnUpdate(setter)
            .Start();
    }
}
