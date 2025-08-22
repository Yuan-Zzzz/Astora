namespace Astora.Benchmark;

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Astora.ECS;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class DataSource
{
    [Params(100_000, 1_000_000)]
    public int N;

    [Params(1, 8, 64)]
    public int Gap;

    [Params(1024, 4096, 16384)]
    public int PageSize;
    
    public int[] KeysExisting = default!;
    public int[] KeysExistingShuffled = default!;
    public int[] KeysMissing = default!;
    
    public SparseSets SparseForQuery = default!;
    public Dictionary<int, byte> DictForQuery = default!;

    private static int[] MakeRange(int n, int gap)
    {
        var arr = new int[n];
        long v = 0;
        for (int i = 0; i < n; i++) { arr[i] = (int)v; v += gap; }
        return arr;
    }

    private static int[] MakeShuffled(int[] src, int seed)
    {
        var arr = (int[])src.Clone();
        var rng = new Random(seed);
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }

    [GlobalSetup]
    public void Setup()
    {
        KeysExisting = MakeRange(N, Gap);
        KeysExistingShuffled = MakeShuffled(KeysExisting, seed: 12345);
        
        KeysMissing = new int[N];
        if (Gap > 1)
        {
            for (int i = 0; i < N; i++) KeysMissing[i] = KeysExisting[i] + 1;
        }
        else
        {
            for (int i = 0; i < N; i++) KeysMissing[i] = KeysExisting[i] + N;
        }
        
        SparseForQuery = new SparseSets(PageSize);
        foreach (var k in KeysExisting) SparseForQuery.Add(k);

        DictForQuery = new Dictionary<int, byte>(capacity: N * 2);
        foreach (var k in KeysExisting) DictForQuery[k] = 1;
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AddBench
{
    private DataSource _ds = default!;

    [ParamsSource(nameof(DataSources))]
    public DataSource DS { get => _ds; set => _ds = value; }
    public IEnumerable<DataSource> DataSources()
    {
        var ds = new DataSource { N = 100_000, Gap = 8, PageSize = 4096 };
        ds.Setup();
        yield return ds;

        ds = new DataSource { N = 1_000_000, Gap = 8, PageSize = 4096 };
        ds.Setup();
        yield return ds;
    }

    [Benchmark(Baseline = true, Description = "SparseSets.Add (sequential)")]
    public int Sparse_Add_Sequential()
    {
        var set = new SparseSets(_ds.PageSize);
        foreach (var k in _ds.KeysExisting) set.Add(k);
        return set.Count;
    }

    [Benchmark(Description = "Dictionary.Add (sequential)")]
    public int Dict_Add_Sequential()
    {
        var dict = new Dictionary<int, byte>(capacity: _ds.N * 2);
        foreach (var k in _ds.KeysExisting) dict[k] = 1;
        return dict.Count;
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ContainsBench
{
    private DataSource _ds = default!;

    [ParamsSource(nameof(DataSources))]
    public DataSource DS { get => _ds; set => _ds = value; }
    public IEnumerable<DataSource> DataSources()
    {
        var ds = new DataSource { N = 1_000_000, Gap = 64, PageSize = 4096 };
        ds.Setup();
        yield return ds;
    }

    [Benchmark(Baseline = true, Description = "SparseSets.Contains (hit)")]
    public int Sparse_Contains_Hit()
    {
        int hits = 0;
        var set = _ds.SparseForQuery;
        foreach (var k in _ds.KeysExistingShuffled) if (set.Contains(k)) hits++;
        return hits;
    }

    [Benchmark(Description = "Dictionary.ContainsKey (hit)")]
    public int Dict_Contains_Hit()
    {
        int hits = 0;
        var dict = _ds.DictForQuery;
        foreach (var k in _ds.KeysExistingShuffled) if (dict.ContainsKey(k)) hits++;
        return hits;
    }

    [Benchmark(Description = "SparseSets.Contains (miss)")]
    public int Sparse_Contains_Miss()
    {
        int hits = 0;
        var set = _ds.SparseForQuery;
        foreach (var k in _ds.KeysMissing) if (set.Contains(k)) hits++;
        return hits;
    }

    [Benchmark(Description = "Dictionary.ContainsKey (miss)")]
    public int Dict_Contains_Miss()
    {
        int hits = 0;
        var dict = _ds.DictForQuery;
        foreach (var k in _ds.KeysMissing) if (dict.ContainsKey(k)) hits++;
        return hits;
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class RemoveBench
{
    private DataSource _ds = default!;

    [ParamsSource(nameof(DataSources))]
    public DataSource DS { get => _ds; set => _ds = value; }
    public IEnumerable<DataSource> DataSources()
    {
        var ds = new DataSource { N = 1_000_000, Gap = 8, PageSize = 4096 };
        ds.Setup();
        yield return ds;
    }
    private SparseSets _set = default!;
    private Dictionary<int, byte> _dict = default!;

    [IterationSetup]
    public void IterationSetup()
    {
        _set = new SparseSets(_ds.PageSize);
        foreach (var k in _ds.KeysExisting) _set.Add(k);

        _dict = new Dictionary<int, byte>(capacity: _ds.N * 2);
        foreach (var k in _ds.KeysExisting) _dict[k] = 1;
    }

    [Benchmark(Baseline = true, Description = "SparseSets.Remove (sequential)")]
    public int Sparse_Remove_Sequential()
    {
        foreach (var k in _ds.KeysExisting) _set.Remove(k);
        return _set.Count;
    }

    [Benchmark(Description = "Dictionary.Remove (sequential)")]
    public int Dict_Remove_Sequential()
    {
        foreach (var k in _ds.KeysExisting) _dict.Remove(k);
        return _dict.Count;
    }
}

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class IterateBench
{
    private DataSource _ds = default!;

    [ParamsSource(nameof(DataSources))]
    public DataSource DS { get => _ds; set => _ds = value; }
    public IEnumerable<DataSource> DataSources()
    {
        var ds = new DataSource { N = 1_000_000, Gap = 64, PageSize = 4096 };
        ds.Setup();
        yield return ds;
    }

    [Benchmark(Baseline = true, Description = "SparseSets iterate dense")]
    public long Sparse_Iterate_All()
    {
        long sum = 0;
        foreach (var k in _ds.SparseForQuery.Dense) sum += k;
        return sum; // 防止 JIT 消除
    }

    [Benchmark(Description = "Dictionary iterate keys")]
    public long Dict_Iterate_All()
    {
        long sum = 0;
        foreach (var k in _ds.DictForQuery.Keys) sum += k;
        return sum;
    }
}
