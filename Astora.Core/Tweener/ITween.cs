namespace Astora.Core.Tweener;

/// <summary>
/// Internal interface used by TweenCore. Update returns true if the Tween has finished.
/// </summary>
public interface ITween
{
    bool Update(float deltaTime);
}
