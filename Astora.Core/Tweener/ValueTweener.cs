using System;

namespace Astora.Core.Tweener;

/// <summary>
/// Generic Tween: Interpolates any type via custom lerp method.
/// </summary>
/// <typeparam name="T">The type to interpolate.</typeparam>
public sealed class ValueTweener<T> : ITween
{
    public T From { get; }
    public T To   { get; }
    public float Duration { get; }

    private readonly Func<T, T, float, T> _lerp;      // Interpolation function
    private readonly Func<float, float>   _ease;      // Easing curve
    private readonly Action<T>            _onUpdate;  // Update callback
    private readonly Action               _onComplete;// Completion callback

    private float _elapsed;   // Elapsed time
    private bool  _completed; // Whether completed

    private ValueTweener(
        T from,
        T to,
        float duration,
        Func<T, T, float, T> lerp,
        Func<float, float> ease,
        Action<T> onUpdate,
        Action onComplete)
    {
        if (duration <= 0)
            throw new ArgumentException("Duration must be greater than 0", nameof(duration));

        From       = from;
        To         = to;
        Duration   = duration;
        _lerp      = lerp  ?? throw new ArgumentNullException(nameof(lerp));
        _ease      = ease  ?? Ease.Linear;
        _onUpdate  = onUpdate  ?? (_ => { });
        _onComplete = onComplete ?? (() => { });
    }

    /// <summary>
    /// Creates a Builder object for chained configuration.
    /// </summary>
    public static Builder Create(T from, T to, float duration) => new Builder(from, to, duration);

    /// <summary>
    /// Called by TweenManager every frame; returns true if the Tween should be removed.
    /// </summary>
    public bool Update(float deltaTime)
    {
        if (_completed) return true;

        _elapsed += deltaTime;
        float t      = Math.Clamp(_elapsed / Duration, 0f, 1f);
        float eased  = _ease(t);
        T     value = _lerp(From, To, eased);
        _onUpdate(value);

        if (_elapsed >= Duration)
        {
            _completed = true;
            _onComplete();
        }

        return _completed;
    }

    #region Chained Builder

    public sealed class Builder
    {
        private readonly T _from;
        private readonly T _to;
        private readonly float _duration;
        private Func<T, T, float, T> _lerp;
        private Func<float, float>   _ease = Ease.Linear;
        private Action<T>            _onUpdate  = _ => { };
        private Action               _onComplete = () => { };

        internal Builder(T from, T to, float duration)
        {
            _from     = from;
            _to       = to;
            _duration = duration;
        }

        /// <summary>
        /// Specifies the interpolation function (required).
        /// </summary>
        public Builder Using(Func<T, T, float, T> lerp)
        {
            _lerp = lerp;
            return this;
        }

        /// <summary>
        /// Specifies the easing curve (optional, defaults to linear).
        /// </summary>
        public Builder SetEase(Func<float, float> easing)
        {
            _ease = easing ?? Ease.Linear;
            return this;
        }

        /// <summary>
        /// Callback for each frame update.
        /// </summary>
        public Builder OnUpdate(Action<T> action)
        {
            _onUpdate = action ?? (_ => { });
            return this;
        }

        /// <summary>
        /// Callback upon completion.
        /// </summary>
        public Builder OnComplete(Action action)
        {
            _onComplete = action ?? (() => { });
            return this;
        }

        /// <summary>
        /// Creates a Tweener instance.
        /// </summary>
        public ValueTweener<T> Build()
        {
            if (_lerp == null)
            {
                if (typeof(T) == typeof(float))
                {
                    // For float, provide default linear interpolation
                    _lerp = (Func<T, T, float, T>)(object)((Func<float, float, float, float>)((a, b, t) => a + (b - a) * t));
                }
                else
                {
                    throw new InvalidOperationException("Interpolation function must be provided (Using)");
                }
            }

            return new ValueTweener<T>(_from, _to, _duration, _lerp, _ease, _onUpdate, _onComplete);
        }

        /// <summary>
        /// Creates and immediately submits to TweenManager.
        /// </summary>
        public ValueTweener<T> Start()
        {
            var tween = Build();
            TweenManager.Add(tween);
            return tween;
        }
    }
    #endregion
}
