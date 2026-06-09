using IndQuestEnums;

namespace IndQuestEnums.Tests;

public sealed class StatusEnum : EnumModel
{
    public static readonly StatusEnum Invalid = new(EnumModel.InvalidValue, EnumModel.InvalidName);
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
    public static readonly DuplicateValueEnum Invalid = new(EnumModel.InvalidValue, EnumModel.InvalidName);
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
