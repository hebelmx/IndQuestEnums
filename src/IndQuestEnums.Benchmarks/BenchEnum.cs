using IndQuestEnums;

namespace IndQuestEnums.Benchmarks;

/// <summary>Representative SmartEnum used to exercise the hot lookup/convert paths.</summary>
public sealed class BenchEnum : EnumModel
{
    public static readonly BenchEnum Invalid = new(EnumModel.InvalidState, EnumModel.InvalidName);
    public static readonly BenchEnum None = new(0, "None", "No State");
    public static readonly BenchEnum Started = new(1, "Started", "Started State");
    public static readonly BenchEnum Running = new(2, "Running", "Running State");
    public static readonly BenchEnum Done = new(4, "Done", "Done State");

    public BenchEnum() { }

    private BenchEnum(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }
}
