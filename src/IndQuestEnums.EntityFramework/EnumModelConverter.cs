using IndQuestEnums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IndQuestEnums.EntityFramework;

/// <summary>
/// Generic EF Core ValueConverter for <see cref="EnumModel"/> types.
/// Stores the stable integer <see cref="EnumModel.Value"/> in the database
/// and rehydrates it using the O(1) cached lookup.
/// </summary>
/// <typeparam name="TEnum">The concrete <see cref="EnumModel"/> type.</typeparam>
public sealed class EnumModelConverter<TEnum> : ValueConverter<TEnum, int>
    where TEnum : EnumModel, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModelConverter{TEnum}"/> class.
    /// </summary>
    public EnumModelConverter()
        : base(
            e => e.Value,
            v => EnumModel.FromValue<TEnum>(v))
    {
    }
}
