// <copyright file="EnumModelJsonConverter.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. All rights reserved.
// </copyright>

namespace IndQuestEnums;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// System.Text.Json converter factory for <see cref="EnumModel"/>-derived types. Serializes an
/// instance as its integer <see cref="EnumModel.Value"/> and resolves it back through the cached
/// <see cref="EnumModel.FromValue{TEnum}(int)"/> lookup — no per-call reflection, no per-call
/// allocation. An unrecognized or non-numeric token resolves to the type's <c>Invalid</c> instance
/// (never throws on read).
/// </summary>
/// <remarks>
/// <para>
/// Opt-in: it ships in the dependency-free core (System.Text.Json is in-box) but changes nothing
/// until you register it — e.g. <c>options.Converters.Add(new EnumModelJsonConverter());</c>.
/// </para>
/// <para>
/// The on-the-wire form is a bare integer (not a <c>{Value,Name,DisplayName}</c> object). Suitable
/// for caches and message payloads; do not point it at a store that persists the older object form.
/// </para>
/// </remarks>
public sealed class EnumModelJsonConverter : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert is not null
            && typeof(EnumModel).IsAssignableFrom(typeToConvert)
            && !typeToConvert.IsAbstract;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(typeToConvert);

        var converterType = typeof(EnumModelJsonConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}

/// <summary>
/// Strongly-typed System.Text.Json converter for a single <see cref="EnumModel"/> subtype,
/// produced by <see cref="EnumModelJsonConverter"/>.
/// </summary>
/// <typeparam name="TEnum">The concrete enumeration type.</typeparam>
public sealed class EnumModelJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : EnumModel, new()
{
    /// <inheritdoc/>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var value))
        {
            return EnumModel.FromValue<TEnum>(value);
        }

        // Anything else (missing/null/string/legacy object) → graceful invalid fallback.
        reader.Skip();
        return EnumModel.InvalidValue<TEnum>();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteNumberValue(value.Value);
    }
}
