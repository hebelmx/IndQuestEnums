using BenchmarkDotNet.Attributes;
using IndQuestEnums;

namespace IndQuestEnums.Benchmarks;

/// <summary>Benchmarks the cached value lookup, the linear name/display-name scans,
/// and the implicit conversions — the per-request hot paths from ADR-0002.</summary>
[MemoryDiagnoser]
public class LookupBenchmarks
{
    [GlobalSetup]
    public void Warm() => EnumModel.Warm<BenchEnum>();

    [Benchmark(Baseline = true)]
    public BenchEnum FromValue_CacheHit() => EnumModel.FromValue<BenchEnum>(2);

    [Benchmark]
    public BenchEnum FromName_LinearScan() => EnumModel.FromName<BenchEnum>("Running");

    [Benchmark]
    public BenchEnum FromDisplayName_LinearScan() => EnumModel.FromDisplayName<BenchEnum>("Running State");

    [Benchmark]
    public int ImplicitToInt() => BenchEnum.Running;

    [Benchmark]
    public string ImplicitToString() => BenchEnum.Running;
}
