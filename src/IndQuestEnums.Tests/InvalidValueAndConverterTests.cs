using System.Text.Json;
using IndQuestEnums;
using Shouldly;
using Xunit;

namespace IndQuestEnums.Tests;

public class InvalidValueTests
{
    [Fact]
    public void InvalidValue_Generic_ReturnsDeclaredInvalidField_NotMinusOne()
    {
        var result = EnumModel.InvalidValue<FlowLikeEnum>();

        result.ShouldBe(FlowLikeEnum.Invalid);
        result.Value.ShouldBe(8); // the declared Invalid, NOT -1
    }

    [Fact]
    public void InvalidValue_Generic_WhenMinusOneIsRealMember_ReturnsRealInvalid()
    {
        // The trap: FromValue(-1) returns the real member at -1 ...
        EnumModel.FromValue<MinusOneStateEnum>(-1).ShouldBe(MinusOneStateEnum.Inactive);

        // ... while InvalidValue<T>() returns the declared Invalid sentinel.
        var invalid = EnumModel.InvalidValue<MinusOneStateEnum>();
        invalid.ShouldBe(MinusOneStateEnum.Invalid);
        invalid.Value.ShouldBe(int.MinValue);
    }

    [Fact]
    public void InvalidValue_ByType_ReturnsDeclaredInvalid()
    {
        // Runtime Type (the converter/serializer scenario the non-generic overload exists for).
        Type type = typeof(FlowLikeEnum);
        var result = EnumModel.InvalidValue(type);

        result.ShouldBe(FlowLikeEnum.Invalid);
        result.Value.ShouldBe(8);
    }

    [Fact]
    public void InvalidValue_WhenNoInvalidFieldDeclared_ReturnsFreshSentinel()
    {
        var result = EnumModel.InvalidValue<NoInvalidEnum>();

        result.ShouldNotBeNull();
        result.Value.ShouldBe(EnumModel.InvalidState);
        result.Name.ShouldBe(EnumModel.InvalidName);
    }

    [Fact]
    public void InvalidValue_ByType_NullType_Throws()
    {
        Should.Throw<ArgumentNullException>(() => EnumModel.InvalidValue((Type)null!));
    }

    [Fact]
    public void Warm_DoesNotThrow_AndResolvesAfter()
    {
        EnumModel.Warm<FlowLikeEnum>();

        EnumModel.FromValue<FlowLikeEnum>(1).ShouldBe(FlowLikeEnum.Started);
        EnumModel.InvalidValue<FlowLikeEnum>().ShouldBe(FlowLikeEnum.Invalid);
    }
}

public class EnumModelJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    [Fact]
    public void Write_EmitsBareInteger_NotObject()
    {
        var json = JsonSerializer.Serialize(StatusEnum.Active, Options);

        json.ShouldBe("1");
    }

    [Fact]
    public void RoundTrip_PreservesInstance()
    {
        var json = JsonSerializer.Serialize(StatusEnum.Active, Options);
        var back = JsonSerializer.Deserialize<StatusEnum>(json, Options);

        back.ShouldBe(StatusEnum.Active);
    }

    [Fact]
    public void Read_UnknownValue_ReturnsInvalid()
    {
        var result = JsonSerializer.Deserialize<StatusEnum>("99", Options);

        result.ShouldBe(StatusEnum.Invalid);
    }

    [Fact]
    public void Read_UnknownValue_ForNonMinusOneInvalid_ReturnsDeclaredInvalid()
    {
        var result = JsonSerializer.Deserialize<FlowLikeEnum>("999", Options);

        result.ShouldBe(FlowLikeEnum.Invalid);
        result!.Value.ShouldBe(8); // not degraded to -1
    }

    [Fact]
    public void Read_NonNumericToken_ReturnsInvalid()
    {
        var result = JsonSerializer.Deserialize<StatusEnum>("{}", Options);

        result.ShouldBe(StatusEnum.Invalid);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new EnumModelJsonConverter());
        return options;
    }
}
