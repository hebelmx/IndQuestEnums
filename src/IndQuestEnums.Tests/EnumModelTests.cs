using IndQuestEnums;
using IndQuestEnums.EntityFramework;
using Shouldly;
using Xunit;

namespace IndQuestEnums.Tests;

public class EnumModelTests
{
    [Fact]
    public void FromValue_WithValidValue_ReturnsCorrectInstance()
    {
        // Act
        var active = StatusEnum.FromValue(1);

        // Assert
        active.ShouldNotBeNull();
        active.ShouldBe(StatusEnum.Active);
        active.Value.ShouldBe(1);
        active.Name.ShouldBe("Active");
        active.DisplayName.ShouldBe("Active Status");
    }

    [Fact]
    public void FromValue_WithUnsetDisplayName_FallsBackToName()
    {
        // Act
        var inactive = StatusEnum.FromValue(2);

        // Assert
        inactive.ShouldNotBeNull();
        inactive.ShouldBe(StatusEnum.Inactive);
        inactive.DisplayName.ShouldBe("Inactive");
    }

    [Fact]
    public void FromName_WithValidName_ReturnsCorrectInstance()
    {
        // Act
        var active = StatusEnum.FromName("Active");

        // Assert
        active.ShouldNotBeNull();
        active.ShouldBe(StatusEnum.Active);
    }

    [Fact]
    public void FromDisplayName_WithValidDisplayName_ReturnsCorrectInstance()
    {
        // Act
        var active = EnumModel.FromDisplayName<StatusEnum>("Active Status");

        // Assert
        active.ShouldNotBeNull();
        active.ShouldBe(StatusEnum.Active);
    }

    [Fact]
    public void FromValue_WithInvalidValue_ReturnsInvalidInstance()
    {
        // Act
        var result = StatusEnum.FromValue(99);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(StatusEnum.Invalid);
        result.Value.ShouldBe(EnumModel.InvalidValue);
        result.Name.ShouldBe(EnumModel.InvalidName);
    }

    [Fact]
    public void FromName_WithInvalidName_ReturnsInvalidInstance()
    {
        // Act
        var result = StatusEnum.FromName("NonExistent");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(StatusEnum.Invalid);
    }

    [Fact]
    public void FromValue_WhenNoInvalidFieldDeclared_ReturnsNewDefaultInstance()
    {
        // Act
        var result = EnumModel.FromValue<NoInvalidEnum>(99);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(EnumModel.InvalidValue);
        result.Name.ShouldBe(EnumModel.InvalidName);
        result.DisplayName.ShouldBe(EnumModel.InvalidName);
    }

    [Fact]
    public void FromValue_CacheIsO1AndReturnsSameReference()
    {
        // Act
        var first = StatusEnum.FromValue(1);
        var second = StatusEnum.FromValue(1);

        // Assert
        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void DuplicateValue_LastDeclaredWinsInLookup()
    {
        // Act
        var resolved = EnumModel.FromValue<DuplicateValueEnum>(1);

        // Assert
        resolved.ShouldNotBeNull();
        resolved.Name.ShouldBe("Second"); // Last wins
    }

    [Fact]
    public void Equals_WithSameTypeAndValue_ReturnsTrue()
    {
        // Arrange
        var first = StatusEnum.FromValue(1);
        var second = StatusEnum.Active;

        // Assert
        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue(); // Reference equality works because of caching
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var first = StatusEnum.Active;
        var second = StatusEnum.Inactive;

        // Assert
        first.Equals(second).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var first = StatusEnum.Active;

        // Assert
        first.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void ImplicitConversionToInt_ReturnsValue()
    {
        // Act
        int val = StatusEnum.Active;

        // Assert
        val.ShouldBe(1);
    }

    [Fact]
    public void ImplicitConversionToString_ReturnsDisplayName()
    {
        // Act
        string name = StatusEnum.Active;

        // Assert
        name.ShouldBe("Active Status");
    }

    [Fact]
    public void AbsoluteDifference_ReturnsCorrectDifference()
    {
        // Act
        var diff = EnumModel.AbsoluteDifference(StatusEnum.Active, StatusEnum.Inactive);

        // Assert
        diff.ShouldBe(1);
    }

    [Fact]
    public void ComparisonOperators_WorkCorrectly()
    {
        // Arrange
        StatusEnum active = StatusEnum.Active;     // Value = 1
        StatusEnum inactive = StatusEnum.Inactive; // Value = 2
        StatusEnum? nullEnum = null;

        // Act & Assert
        (active == StatusEnum.Active).ShouldBeTrue();
        (active != inactive).ShouldBeTrue();
        (active < inactive).ShouldBeTrue();
        (active <= inactive).ShouldBeTrue();
        (active <= StatusEnum.Active).ShouldBeTrue();
        (inactive > active).ShouldBeTrue();
        (inactive >= active).ShouldBeTrue();
        (inactive >= StatusEnum.Inactive).ShouldBeTrue();

        // Null comparisons
        (nullEnum == null).ShouldBeTrue();
        (active == null).ShouldBeFalse();
        (null == active).ShouldBeFalse();
        (nullEnum < active).ShouldBeTrue();
        (active > nullEnum).ShouldBeTrue();
        (nullEnum <= active).ShouldBeTrue();
        (active >= nullEnum).ShouldBeTrue();
    }

    [Fact]
    public void EFConverter_ConvertsValueCorrectly()
    {
        // Arrange
        var converter = new EnumModelConverter<StatusEnum>();

        // Act
        var intValue = (int)converter.ConvertToProvider(StatusEnum.Active)!;
        var enumValue = (StatusEnum)converter.ConvertFromProvider(1)!;

        // Assert
        intValue.ShouldBe(1);
        enumValue.ShouldBe(StatusEnum.Active);
    }

    [Fact]
    public void EFComparer_ComparesCorrectly()
    {
        // Arrange
        var comparer = new EnumModelComparer<StatusEnum>();

        // Act & Assert
        comparer.Equals(StatusEnum.Active, StatusEnum.Active).ShouldBeTrue();
        comparer.Equals(StatusEnum.Active, StatusEnum.Inactive).ShouldBeFalse();
        comparer.Equals(null, null).ShouldBeTrue();
        comparer.Equals(StatusEnum.Active, null).ShouldBeFalse();
        comparer.Equals(null, StatusEnum.Active).ShouldBeFalse();

        comparer.GetHashCode(StatusEnum.Active).ShouldBe(StatusEnum.Active.Value.GetHashCode());
    }
}
