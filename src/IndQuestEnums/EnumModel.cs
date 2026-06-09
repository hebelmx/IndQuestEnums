// <copyright file="EnumModel.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. All rights reserved.
// </copyright>

namespace IndQuestEnums;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Base class for strongly-typed enumerations (the SmartEnum / "EnumModel" pattern):
/// a finite set of named, valued, display-named instances declared as
/// <c>public static readonly</c> fields on a derived <c>sealed</c> type.
/// </summary>
/// <remarks>
/// <para>
/// Thread-safe with O(1) lookup after a one-time reflection warm-up per derived type.
/// Lookups never throw — an unmatched value/name resolves to the type's <c>Invalid</c>
/// instance (a public static readonly field named <c>Invalid</c> on the derived type),
/// or a default instance if none is declared.
/// </para>
/// <para>
/// Dependency-free by design: no Entity Framework, no lookup-table machinery — safe to
/// reference from a pure Domain layer. EF value-converters/comparers live in the
/// companion package <c>IndQuestEnums.EntityFramework</c>. (Carries forward the
/// display-name fix made in the ExxerCube.Prisma port of the original IndTrace type.)
/// </para>
/// </remarks>
public abstract class EnumModel : IComparable, IEquatable<EnumModel>
{
    private static readonly ConcurrentDictionary<Type, Dictionary<int, EnumModel>> LookupCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModel"/> class as the invalid value.
    /// Required to satisfy the <c>new()</c> generic constraint on the factory methods.
    /// </summary>
    protected EnumModel()
    {
        this.Value = InvalidValue;
        this.Name = InvalidName;
        this.DisplayName = InvalidName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModel"/> class.
    /// </summary>
    /// <param name="value">The stable integer value (storage/serialization key).</param>
    /// <param name="name">The internal, invariant name.</param>
    /// <param name="displayName">Optional human-facing name; falls back to <paramref name="name"/>.</param>
    protected EnumModel(int value, string name, string displayName = "")
    {
        this.Value = value;
        this.Name = name;
        this.DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    }

    /// <summary>Gets the sentinel integer value used for the invalid/default instance.</summary>
    public const int InvalidValue = -1;

    /// <summary>Gets the sentinel name used for the invalid/default instance.</summary>
    public const string InvalidName = "Invalid Value";

    /// <summary>Gets the stable integer value (storage/serialization key).</summary>
    public int Value { get; }

    /// <summary>Gets the internal, invariant name.</summary>
    public string Name { get; }

    /// <summary>Gets the human-facing display name (falls back to <see cref="Name"/>).</summary>
    public string DisplayName { get; }

    /// <summary>Implicitly converts an instance to its integer <see cref="Value"/>.</summary>
    /// <param name="model">The instance to convert.</param>
    public static implicit operator int(EnumModel model) => model.Value;

    /// <summary>Implicitly converts an instance to its <see cref="DisplayName"/>.</summary>
    /// <param name="model">The instance to convert.</param>
    public static implicit operator string(EnumModel model) => model.DisplayName;

    /// <summary>Returns all declared instances of <typeparamref name="TEnum"/>.</summary>
    /// <typeparam name="TEnum">The derived enumeration type.</typeparam>
    /// <returns>Every <c>public static readonly</c> instance declared on the type.</returns>
    public static IEnumerable<TEnum> GetAll<TEnum>()
        where TEnum : EnumModel =>
        typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(field => field.GetValue(null))
            .OfType<TEnum>();

    /// <summary>Determines whether <paramref name="value"/> matches a declared instance.</summary>
    /// <typeparam name="TEnum">The derived enumeration type.</typeparam>
    /// <param name="value">The integer value to test.</param>
    /// <returns><see langword="true"/> if a declared instance has that value.</returns>
    public static bool Exists<TEnum>(int value)
        where TEnum : EnumModel =>
        GetAll<TEnum>().Any(item => item.Value == value);

    /// <summary>Resolves an instance by value via an O(1) cached lookup (never throws).</summary>
    /// <typeparam name="TEnum">The derived enumeration type.</typeparam>
    /// <param name="value">The integer value to resolve.</param>
    /// <returns>The matching instance, or the type's <c>Invalid</c> instance when unmatched.</returns>
    public static TEnum FromValue<TEnum>(int value)
        where TEnum : EnumModel, new()
    {
        var lookup = LookupCache.GetOrAdd(typeof(TEnum), _ => BuildLookup<TEnum>());
        return lookup.TryGetValue(value, out var found) ? (TEnum)found : Invalid<TEnum>();
    }

    /// <summary>Resolves an instance by internal name (never throws).</summary>
    /// <typeparam name="TEnum">The derived enumeration type.</typeparam>
    /// <param name="name">The internal name to resolve.</param>
    /// <returns>The matching instance, or the type's <c>Invalid</c> instance when unmatched.</returns>
    public static TEnum FromName<TEnum>(string name)
        where TEnum : EnumModel, new() =>
        GetAll<TEnum>().FirstOrDefault(item => item.Name == name) ?? Invalid<TEnum>();

    /// <summary>Resolves an instance by display name (never throws).</summary>
    /// <typeparam name="TEnum">The derived enumeration type.</typeparam>
    /// <param name="displayName">The display name to resolve.</param>
    /// <returns>The matching instance, or the type's <c>Invalid</c> instance when unmatched.</returns>
    public static TEnum FromDisplayName<TEnum>(string displayName)
        where TEnum : EnumModel, new() =>
        GetAll<TEnum>().FirstOrDefault(item => item.DisplayName == displayName) ?? Invalid<TEnum>();

    /// <summary>Returns the absolute difference between two instances' values.</summary>
    /// <param name="first">The first instance.</param>
    /// <param name="second">The second instance.</param>
    /// <returns>The absolute difference of their <see cref="Value"/>s.</returns>
    public static int AbsoluteDifference(EnumModel first, EnumModel second) =>
        Math.Abs(first.Value - second.Value);

    /// <inheritdoc/>
    public bool Equals(EnumModel? other) =>
        other is not null && this.GetType() == other.GetType() && this.Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => this.Equals(obj as EnumModel);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(this.GetType(), this.Value);

    /// <inheritdoc/>
    public int CompareTo(object? obj) =>
        obj is EnumModel enumeration ? this.Value.CompareTo(enumeration.Value) : 1;

    /// <summary>Compares two instances for equality.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(EnumModel? left, EnumModel? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Compares two instances for inequality.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(EnumModel? left, EnumModel? right) =>
        !(left == right);

    /// <summary>Determines if the first instance is less than the second instance.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the first instance is less than the second; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(EnumModel? left, EnumModel? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>Determines if the first instance is less than or equal to the second instance.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the first instance is less than or equal to the second; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(EnumModel? left, EnumModel? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>Determines if the first instance is greater than the second instance.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the first instance is greater than the second; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(EnumModel? left, EnumModel? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>Determines if the first instance is greater than or equal to the second instance.</summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns><see langword="true"/> if the first instance is greater than or equal to the second; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(EnumModel? left, EnumModel? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>Deconstructs the instance into its components.</summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The internal name.</param>
    /// <param name="displayName">The display name.</param>
    public void Deconstruct(out int value, out string name, out string displayName) =>
        (value, name, displayName) = (this.Value, this.Name, this.DisplayName);

    /// <inheritdoc/>
    public override string ToString() => this.DisplayName;

    private static TEnum Invalid<TEnum>()
        where TEnum : EnumModel, new()
    {
        var invalidField = typeof(TEnum).GetField(
            "Invalid",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return invalidField?.GetValue(null) as TEnum ?? new TEnum();
    }

    private static Dictionary<int, EnumModel> BuildLookup<TEnum>()
        where TEnum : EnumModel
    {
        var lookup = new Dictionary<int, EnumModel>();

        // Indexer assignment (not ToDictionary) so duplicate values do not throw — last wins.
        foreach (var instance in GetAll<TEnum>())
        {
            lookup[instance.Value] = instance;
        }

        return lookup;
    }
}
