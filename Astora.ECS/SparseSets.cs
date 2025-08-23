using System.Runtime.CompilerServices;

namespace Astora.ECS;

public sealed class SparseSets
{
    private readonly List<int> _dense = [];
    private readonly List<int[]?> _sparse = [];

    private int _counts;
    
    public int Count => _counts;

    public int PageSize { get; }
    public int PageShift { get; }
    public int PageMask { get; }
    
    public const int Invalid = -1;

    public IReadOnlyList<int> Dense => _dense;
    
    public SparseSets(int pageSize = 4096)
    {
        if (pageSize <= 0 || (pageSize & (pageSize - 1)) != 0)
            throw new ArgumentException("PageSize must be a positive power-of-two.", nameof(pageSize));
        PageSize  = pageSize;
        PageMask  = pageSize - 1;
        PageShift = Log2(pageSize);
    }

    public void Add(int t)
    {
        if (t < 0) throw new ArgumentOutOfRangeException(nameof(t));
        var page   = ToPage(t);
        var offset = ToOffset(t);
        var bucket = EnsurePage(page);

        if (bucket[offset] != Invalid)
            throw new InvalidOperationException("Key already exists in SparseSets.");

        int di = _counts;
        bucket[offset] = di;
        _dense.Add(t);
        _counts++;
    }
    
    public void Remove(int t)
    {
        if (t < 0) return;

        var page = ToPage(t);
        var bucket = TryPage(page);
        if (bucket is null) return;

        var offset = ToOffset(t);
        int di = bucket[offset];
        if (di == Invalid) return;

        int last = _counts - 1;
        int moved = _dense[last];
        
        _dense[di] = moved;
        
        var mp = ToPage(moved);
        var mo = ToOffset(moved);
        var movedBucket = _sparse[mp]!;
        movedBucket[mo] = di;
        
        bucket[offset] = Invalid;
        _dense.RemoveAt(last);
        _counts--;
    }
    
    public bool Contains(int t)
    {
        if (t < 0) return false;
        var page = ToPage(t);
        var bucket = TryPage(page);
        if (bucket is null) return false;
        return bucket[ToOffset(t)] != Invalid;
    }
    
    public int IndexOf(int t)
    {
        if (t < 0) return Invalid;
        var page = ToPage(t);
        var bucket = TryPage(page);
        if (bucket is null) return Invalid;
        return bucket[ToOffset(t)];
    }

    public bool TryIndex(int t, out int index)
    {
        index = IndexOf(t);
        return index != Invalid;
    }

    public void Clear()
    {
        for (var i = 0; i < _sparse.Count; i++)
        {
            var p = _sparse[i];
            if (p != null) Array.Fill(p, Invalid);
        }
        _dense.Clear();
        _counts = 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToPage(int key) => key >> PageShift;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToOffset(int key) => key & PageMask;

    private int[] EnsurePage(int page)
    {
        while (_sparse.Count <= page) _sparse.Add(null);
        var bucket = _sparse[page];
        if (bucket == null)
        {
            bucket = new int[PageSize];
            Array.Fill(bucket, Invalid);
            _sparse[page] = bucket;
        }
        return bucket;
    }

    private int[]? TryPage(int page)
        => (page >= 0 && page < _sparse.Count) ? _sparse[page] : null;
    
    private static int Log2(int v)
    {
        int r = 0;
        while ((v >>= 1) != 0) r++;
        return r;
    }
}