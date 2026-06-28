using System.Text.Json;
using BenchmarkDotNet.Attributes;
using IndQuestEnums;
using IndQuestEnums.EntityFramework;

namespace IndQuestEnums.Benchmarks;

/// <summary>Benchmarks the EF int⇄enum ValueConverter and the System.Text.Json
/// converter round-trips — invoked ~8×/request per ADR-0002.</summary>
[MemoryDiagnoser]
public class ConverterBenchmarks
{
    private readonly EnumModelConverter<BenchEnum> _ef = new();
    private readonly JsonSerializerOptions _json = new();
    private string _payload = "2";

    [GlobalSetup]
    public void Setup()
    {
        EnumModel.Warm<BenchEnum>();
        _json.Converters.Add(new EnumModelJsonConverter());
        _payload = JsonSerializer.Serialize(BenchEnum.Running, _json);
    }

    [Benchmark]
    public object? Ef_ToProvider() => _ef.ConvertToProvider(BenchEnum.Running);

    [Benchmark]
    public object? Ef_FromProvider() => _ef.ConvertFromProvider(2);

    [Benchmark]
    public string Json_Serialize() => JsonSerializer.Serialize(BenchEnum.Running, _json);

    [Benchmark]
    public BenchEnum? Json_Deserialize() => JsonSerializer.Deserialize<BenchEnum>(_payload, _json);
}
