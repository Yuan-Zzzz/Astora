namespace Astora.Core.Util;

/// <summary>
/// 收纳多个 IDisposable，常用于成对订阅/注册的集中释放。
/// </summary>
public sealed class DisposableBag : IDisposable
{
    private readonly List<IDisposable> _items = new();
    private bool _disposed;

    public T Add<T>(T item) where T : IDisposable
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DisposableBag));
        _items.Add(item);
        return item;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // 逆序释放更安全（后添加的往往依赖先添加的）
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            try { _items[i].Dispose(); }
            catch { /* 忽略释放异常 */ }
        }
        _items.Clear();
    }
}