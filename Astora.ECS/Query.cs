using System;
using System.Collections.Generic;

namespace Astora.ECS;
using Entity = Int32;

public readonly struct Query<T>
{
    private readonly ComponentPool<T> _p1;

    public Query(ComponentPool<T> p1) => _p1 = p1;

    public Enumerator GetEnumerator() => new Enumerator(_p1.Set.Dense);

    public struct Enumerator
    {
        private readonly IReadOnlyList<int> _dense;
        private int _i;

        public Enumerator(IReadOnlyList<int> dense)
        {
            _dense = dense;
            _i = -1;
        }

        public bool MoveNext()
        {
            _i++;
            return _i < _dense.Count;
        }

        public Entity Current => _dense[_i];
    }
}

/// <summary>
/// 双组件查询：遍历同时拥有 T1、T2 的实体。
/// 以更小的池作为 pivot 迭代，其余用 Contains 过滤。
/// </summary>
public readonly struct Query<T1, T2>
{
    private readonly ComponentPool<T1> _p1;
    private readonly ComponentPool<T2> _p2;
    private readonly IReadOnlyList<int> _pivot;
    private readonly byte _pivotId; // 1: p1, 2: p2

    public Query(ComponentPool<T1> p1, ComponentPool<T2> p2)
    {
        _p1 = p1;
        _p2 = p2;

        if (p1.Set.Count <= p2.Set.Count)
        {
            _pivot = p1.Set.Dense;
            _pivotId = 1;
        }
        else
        {
            _pivot = p2.Set.Dense;
            _pivotId = 2;
        }
    }

    public Enumerator GetEnumerator() => new Enumerator(_pivot, _p1, _p2, _pivotId);

    public struct Enumerator
    {
        private readonly IReadOnlyList<int> _pivot;
        private readonly ComponentPool<T1> _p1;
        private readonly ComponentPool<T2> _p2;
        private readonly byte _pid;
        private int _i;
        private int _current;

        public Enumerator(IReadOnlyList<int> pivot, ComponentPool<T1> p1, ComponentPool<T2> p2, byte pid)
        {
            _pivot = pivot;
            _p1 = p1;
            _p2 = p2;
            _pid = pid;
            _i = -1;
            _current = default;
        }

        public bool MoveNext()
        {
            while (true)
            {
                _i++;
                if (_i >= _pivot.Count) return false;

                int e = _pivot[_i];
                bool ok = (_pid == 1) ? _p2.Contains(e) : _p1.Contains(e);
                if (ok)
                {
                    _current = e;
                    return true;
                }
            }
        }

        public Entity Current => _current;
    }
}
public readonly struct Query<T1, T2, T3>
{
    private readonly ComponentPool<T1> _p1;
    private readonly ComponentPool<T2> _p2;
    private readonly ComponentPool<T3> _p3;
    private readonly IReadOnlyList<int> _pivot;
    private readonly byte _pivotId;

    public Query(ComponentPool<T1> p1, ComponentPool<T2> p2, ComponentPool<T3> p3)
    {
        _p1 = p1; _p2 = p2; _p3 = p3;
        
        var c1 = p1.Set.Count;
        var c2 = p2.Set.Count;
        var c3 = p3.Set.Count;

        if (c1 <= c2 && c1 <= c3) { _pivot = p1.Set.Dense; _pivotId = 1; }
        else if (c2 <= c1 && c2 <= c3) { _pivot = p2.Set.Dense; _pivotId = 2; }
        else { _pivot = p3.Set.Dense; _pivotId = 3; }
    }

    public Enumerator GetEnumerator() => new Enumerator(_pivot, _p1, _p2, _p3, _pivotId);

    public struct Enumerator
    {
        private readonly IReadOnlyList<int> _pivot;
        private readonly ComponentPool<T1> _p1;
        private readonly ComponentPool<T2> _p2;
        private readonly ComponentPool<T3> _p3;
        private readonly byte _pid;
        private int _i;
        private int _current;

        public Enumerator(IReadOnlyList<int> pivot, ComponentPool<T1> p1, ComponentPool<T2> p2, ComponentPool<T3> p3, byte pid)
        {
            _pivot = pivot;
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _pid = pid;
            _i = -1;
            _current = default;
        }

        public bool MoveNext()
        {
            while (true)
            {
                _i++;
                if (_i >= _pivot.Count) return false;

                int e = _pivot[_i];
                bool ok = _pid switch
                {
                    1 => _p2.Contains(e) && _p3.Contains(e),
                    2 => _p1.Contains(e) && _p3.Contains(e),
                    _ => _p1.Contains(e) && _p2.Contains(e),
                };

                if (ok)
                {
                    _current = e;
                    return true;
                }
            }
        }

        public Entity Current => _current;
    }
}
