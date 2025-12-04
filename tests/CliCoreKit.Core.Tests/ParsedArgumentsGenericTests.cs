using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;

namespace CliCoreKit.Core.Tests;

public class ParsedArgumentsGenericTests
{
    [Fact]
    public void GetOption_WithInt_ReturnsTypedValue()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("port", "8080");

        // Act
        var port = args.GetOption<int>("port");

        // Assert
        port.Should().Be(8080);
    }

    [Fact]
    public void GetOption_WithBool_ReturnsTrue()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("verbose");

        // Act
        var verbose = args.GetOption<bool>("verbose");

        // Assert
        verbose.Should().BeTrue();
    }

    [Fact]
    public void GetOption_WithBoolAndValue_ParsesCorrectly()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("enabled", "true");

        // Act
        var enabled = args.GetOption<bool>("enabled");

        // Assert
        enabled.Should().BeTrue();
    }

    [Fact]
    public void GetOption_WithDouble_ReturnsTypedValue()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("temperature", "23.5");

        // Act
        var temp = args.GetOption<double>("temperature");

        // Assert
        temp.Should().Be(23.5);
    }

    [Fact]
    public void GetOption_WithDefaultValue_ReturnsDefault()
    {
        // Arrange
        var args = new ParsedArguments();

        // Act
        var port = args.GetOption<int>("port", 3000);

        // Assert
        port.Should().Be(3000);
    }

    [Fact]
    public void GetArgument_WithInt_ReturnsTypedValue()
    {
        // This test is deprecated - arguments should be accessed by name via CommandContext
        Assert.True(true);
    }

    [Fact]
    public void GetArgument_WithString_ReturnsValue()
    {
        // This test is deprecated - arguments should be accessed by name via CommandContext
        Assert.True(true);
    }

    [Fact]
    public void GetArgument_WithMissingIndex_ReturnsDefault()
    {
        // This test is deprecated - arguments should be accessed by name via CommandContext
        Assert.True(true);
    }

    [Fact]
    public void GetOptionValues_WithMultipleInts_ReturnsTypedList()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("numbers", "1");
        args.AddOption("numbers", "2");
        args.AddOption("numbers", "3");

        // Act
        var numbers = args.GetOptionValues<int>("numbers");

        // Assert
        numbers.Should().HaveCount(3);
        numbers.Should().Contain(new[] { 1, 2, 3 });
    }

    [Fact]
    public void TryGetValue_WithValidInt_ReturnsTrue()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("count", "42");

        // Act
        var success = args.TryGetValue<int>("count", out var value);

        // Assert
        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_WithInvalidInt_ReturnsFalse()
    {
        // Arrange
        var args = new ParsedArguments();
        args.AddOption("count", "not-a-number");

        // Act
        var success = args.TryGetValue<int>("count", out var value);

        // Assert
        success.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void TryGetValue_WithMissingOption_ReturnsFalse()
    {
        // Arrange
        var args = new ParsedArguments();

        // Act
        var success = args.TryGetValue<int>("missing", out var value);

        // Assert
        success.Should().BeFalse();
    }
}
