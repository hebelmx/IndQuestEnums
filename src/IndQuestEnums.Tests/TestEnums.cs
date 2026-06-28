using IndQuestEnums;

namespace IndQuestEnums.Tests;

public sealed class StatusEnum : EnumModel
{
    public static readonly StatusEnum Invalid = new(EnumModel.InvalidState, EnumModel.InvalidName);
    public static readonly StatusEnum Active = new(1, "Active", "Active Status");
    public static readonly StatusEnum Inactive = new(2, "Inactive"); // DisplayName should fall back to Name

    public StatusEnum() { }

    private StatusEnum(int value, string name, string displayName = "")
        : base(value, name, displayName)
    {
    }

    public static StatusEnum FromValue(int value) => FromValue<StatusEnum>(value);
    public static StatusEnum FromName(string name) => FromName<StatusEnum>(name);
}

public sealed class DuplicateValueEnum : EnumModel
{
    public static readonly DuplicateValueEnum Invalid = new(EnumModel.InvalidState, EnumModel.InvalidName);
    public static readonly DuplicateValueEnum First = new(1, "First");
    public static readonly DuplicateValueEnum Second = new(1, "Second"); // Duplicate value 1

    public DuplicateValueEnum() { }

    private DuplicateValueEnum(int value, string name)
        : base(value, name)
    {
    }
}

public sealed class NoInvalidEnum : EnumModel
{
    // Declares no static readonly field named "Invalid"
    public static readonly NoInvalidEnum First = new(1, "First");

    public NoInvalidEnum() { }

    private NoInvalidEnum(int value, string name)
        : base(value, name)
    {
    }
}

// Invalid sentinel is NOT -1 (it is 8). Pins that InvalidValue<T>() returns the declared
// Invalid field, not FromValue(-1).
public sealed class FlowLikeEnum : EnumModel
{
    public static readonly FlowLikeEnum None = new(0, "None");
    public static readonly FlowLikeEnum Started = new(1, "Started");
    public static readonly FlowLikeEnum Running = new(2, "Running");
    public static readonly FlowLikeEnum Done = new(4, "Done");
    public static readonly FlowLikeEnum Invalid = new(8, "Invalid");

    public FlowLikeEnum() { }

    private FlowLikeEnum(int value, string name)
        : base(value, name)
    {
    }
}

// -1 is a REAL member (Inactive); the invalid sentinel is int.MinValue. Pins that
// FromValue(-1) returns Inactive while InvalidValue<T>() returns the real Invalid.
public sealed class MinusOneStateEnum : EnumModel
{
    public static readonly MinusOneStateEnum Invalid = new(int.MinValue, "Invalid");
    public static readonly MinusOneStateEnum Inactive = new(-1, "Inactive");
    public static readonly MinusOneStateEnum None = new(0, "None");
    public static readonly MinusOneStateEnum Active = new(1, "Active");

    public MinusOneStateEnum() { }

    private MinusOneStateEnum(int value, string name)
        : base(value, name)
    {
    }
}
