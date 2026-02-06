using System;
using System.Collections.Generic;

namespace Astora.Core.Tweener;

public static class Ease
{

    private static float Sin (float v) => (float)Math.Sin(v);
    private static float Cos (float v) => (float)Math.Cos(v);
    private static float Pow (float b, float e) => (float)Math.Pow(b, e);
    private static float Sqrt(float v) => (float)Math.Sqrt(v);
    private const float Pi  = (float)Math.PI;
    private const float _2PI = Pi * 2f;

    public static readonly Func<float, float> Linear = t => t;

    public static readonly Func<float, float> InQuad    = t => t * t;
    public static readonly Func<float, float> OutQuad   = t => t * (2f - t);
    public static readonly Func<float, float> InOutQuad = t => t < .5f ? 2f * t * t
                                                                      : -1f + (4f - 2f * t) * t;

    public static readonly Func<float, float> InCubic    = t => t * t * t;
    public static readonly Func<float, float> OutCubic   = t => --t * t * t + 1f;
    public static readonly Func<float, float> InOutCubic = t => t < .5f
                                                            ? 4f * t * t * t
                                                            : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

    public static readonly Func<float, float> InQuart    = t => t * t * t * t;
    public static readonly Func<float, float> OutQuart   = t => 1f - (--t) * t * t * t;
    public static readonly Func<float, float> InOutQuart = t => t < .5f
                                                            ? 8f * t * t * t * t
                                                            : 1f - 8f * (--t) * t * t * t;

    public static readonly Func<float, float> InQuint    = t => t * t * t * t * t;
    public static readonly Func<float, float> OutQuint   = t => 1f + (--t) * t * t * t * t;
    public static readonly Func<float, float> InOutQuint = t => t < .5f
                                                            ? 16f * t * t * t * t * t
                                                            : 1f + 16f * (--t) * t * t * t * t;

    public static readonly Func<float, float> InSine  = t => 1f - Cos(t * Pi * .5f);
    public static readonly Func<float, float> OutSine = t => Sin(t * Pi * .5f);
    public static readonly Func<float, float> InOutSine = t => -.5f * (Cos(Pi * t) - 1f);

    public static readonly Func<float, float> InExpo = t =>
        t == 0f ? 0f : Pow(2f, 10f * (t - 1f));

    public static readonly Func<float, float> OutExpo = t =>
        t == 1f ? 1f : 1f - Pow(2f, -10f * t);

    public static readonly Func<float, float> InOutExpo = t => t switch
    {
        0f => 0f,
        1f => 1f,
        _  => t < .5f
              ?  .5f * Pow(2f, 10f * (2f * t - 1f))
              :  .5f * (2f - Pow(2f, -10f * (2f * t - 1f)))
    };

    public static readonly Func<float, float> InCirc  = t => 1f - Sqrt(1f - t * t);
    public static readonly Func<float, float> OutCirc = t => Sqrt(1f - (--t) * t);
    public static readonly Func<float, float> InOutCirc = t =>
    {
        t *= 2f;
        return t < 1f
            ? -.5f * (Sqrt(1f - t * t) - 1f)
            :  .5f * (Sqrt(1f - (t -= 2f) * t) + 1f);
    };

    private const float c1 = 1.70158f;
    private const float c3 = c1 + 1f;
    public static readonly Func<float, float> InBack  = t => c3 * t * t * t - c1 * t * t;
    public static readonly Func<float, float> OutBack = t => 1f + c3 * Pow(--t, 3) + c1 * t * t;
    public static readonly Func<float, float> InOutBack = t =>
    {
        t *= 2f;
        const float c2 = c1 * 1.525f;
        return t < 1f
            ? .5f * (t * t * ((c2 + 1f) * t - c2))
            : .5f * ((t -= 2f) * t * ((c2 + 1f) * t + c2) + 2f);
    };

    private const float c4 = _2PI / 3f;
    private const float c5 = _2PI / 4.5f;

    public static readonly Func<float, float> InElastic = t =>
        t switch
        {
            0f => 0f,
            1f => 1f,
            _  => -Pow(2f, 10f * (t - 1f)) * Sin((t * 10f - 10.75f) * c4)
        };

    public static readonly Func<float, float> OutElastic = t =>
        t switch
        {
            0f => 0f,
            1f => 1f,
            _  =>  Pow(2f, -10f * t) * Sin((t * 10f - 0.75f) * c4) + 1f
        };

    public static readonly Func<float, float> InOutElastic = t =>
    {
        if (t == 0f || t == 1f) return t;
        t *= 2f;
        return t < 1f
            ? -.5f * Pow(2f, 10f * (t - 1f)) * Sin((t * 10f - 11.125f) * c5)
            :  .5f * Pow(2f, -10f * (t - 1f)) * Sin((t * 10f - 11.125f) * c5) + 1f;
    };

    private static float BounceOut(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        return t switch
        {
            < 1f / d1         => n1 * t * t,
            < 2f / d1         => n1 * (t -= 1.5f / d1) * t + 0.75f,
            < 2.5f / d1       => n1 * (t -= 2.25f / d1) * t + 0.9375f,
            _                 => n1 * (t -= 2.625f / d1) * t + 0.984375f
        };
    }

    public static readonly Func<float, float> OutBounce = BounceOut;
    public static readonly Func<float, float> InBounce  = t => 1f - BounceOut(1f - t);
    public static readonly Func<float, float> InOutBounce = t =>
        t < .5f
            ? (1f - BounceOut(1f - 2f * t)) * .5f
            : (1f + BounceOut(2f * t - 1f)) * .5f;
}
