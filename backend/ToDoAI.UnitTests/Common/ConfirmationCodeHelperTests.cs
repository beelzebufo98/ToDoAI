using ToDoAI.Application.Common;

namespace ToDoAI.UnitTests.Common;

public sealed class ConfirmationCodeHelperTests
{
    [Fact]
    public void Generate_ShouldReturn_SixDigitString()
    {
        // Arrange / Act
        var code = ConfirmationCodeHelper.Generate();

        // Assert
        code.Should().HaveLength(6);
    }

    [Fact]
    public void Generate_ShouldReturn_OnlyDigits()
    {
        // Arrange / Act
        var code = ConfirmationCodeHelper.Generate();

        // Assert
        code.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public void Generate_ShouldReturn_ValueInExpectedRange()
    {
        // Arrange / Act
        var code = ConfirmationCodeHelper.Generate();
        var numericValue = int.Parse(code);

        // Assert
        numericValue.Should().BeInRange(100000, 999999);
    }

    [Fact]
    public void Generate_RepeatedCalls_ShouldProduceDifferentValues()
    {
        // Arrange
        const int iterations = 20;

        // Act
        var codes = Enumerable.Range(0, iterations)
            .Select(_ => ConfirmationCodeHelper.Generate())
            .ToList();

        // Assert
        codes.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Hash_ShouldReturn_64CharacterHexString()
    {
        // Arrange
        const string input = "123456";

        // Act
        var hash = ConfirmationCodeHelper.Hash(input);

        // Assert
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex(@"^[0-9a-f]+$");
    }

    [Fact]
    public void Hash_ShouldReturn_LowercaseString()
    {
        // Arrange
        const string input = "123456";

        // Act
        var hash = ConfirmationCodeHelper.Hash(input);

        // Assert
        hash.Should().Be(hash.ToLowerInvariant());
    }

    [Fact]
    public void Hash_SameInput_ShouldReturn_SameOutput()
    {
        // Arrange
        const string input = "123456";

        // Act
        var hash1 = ConfirmationCodeHelper.Hash(input);
        var hash2 = ConfirmationCodeHelper.Hash(input);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_DifferentInputs_ShouldReturn_DifferentOutputs()
    {
        // Arrange
        const string input1 = "123456";
        const string input2 = "654321";

        // Act
        var hash1 = ConfirmationCodeHelper.Hash(input1);
        var hash2 = ConfirmationCodeHelper.Hash(input2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("abc")]
    [InlineData("")]
    public void Hash_AnyInput_ShouldAlwaysReturn_64CharacterString(string input)
    {
        // Act
        var hash = ConfirmationCodeHelper.Hash(input);

        // Assert
        hash.Should().HaveLength(64);
    }
}
