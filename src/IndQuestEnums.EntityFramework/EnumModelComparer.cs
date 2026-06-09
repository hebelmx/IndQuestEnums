using IndQuestEnums;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IndQuestEnums.EntityFramework;

/// <summary>
/// Generic EF Core ValueComparer for <see cref="EnumModel"/> types.
/// Performs comparison and hash code generation based on the stable integer <see cref="EnumModel.Value"/>.
/// </summary>
/// <typeparam name="TEnum">The concrete <see cref="EnumModel"/> type.</typeparam>
public sealed class EnumModelComparer<TEnum> : ValueComparer<TEnum>
    where TEnum : EnumModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModelComparer{TEnum}"/> class.
    /// </summary>
    public EnumModelComparer()
        : base(
            (a, b) => ReferenceEquals(a, b) || (a != null && b != null && a.Value == b.Value),
            e => e.Value.GetHashCode(),
            e => e)
    {
    }
}
