using AwesomeAssertions;
using Monbsoft.CliCoreKit.Core;

namespace CliCoreKit.Core.Tests;

public class ArgumentParserTests
{
    private readonly ArgumentParser _parser = new();

    [Fact]
    public void Parse_LongOption_WithValue()
    {
        // Arrange
        var args = new[] { "--name=John" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("name").Should().BeTrue();
        result.GetOptionValue("name").Should().Be("John");
    }

    [Fact]
    public void Parse_LongOption_WithEquals()
    {
        // Arrange
        var args = new[] { "--name=John" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("name").Should().BeTrue();
        result.GetOptionValue("name").Should().Be("John");
    }

    [Fact]
    public void Parse_ShortOption_WithValue()
    {
        // Arrange
        var args = new[] { "-n", "John" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("n").Should().BeTrue();
        result.GetOptionValue("n").Should().Be("John");
    }

    [Fact]
    public void Parse_CombinedShortOptions()
    {
        // Arrange
        var args = new[] { "-abc" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("a").Should().BeTrue();
        result.HasOption("b").Should().BeTrue();
        result.HasOption("c").Should().BeTrue();
    }

    [Fact]
    public void Parse_WindowsStyle_Option()
    {
        // Arrange
        var args = new[] { "/name", "John" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("name").Should().BeTrue();
        result.GetOptionValue("name").Should().Be("John");
    }

    [Fact]
    public void Parse_PositionalArguments()
    {
        // Arrange
        var args = new[] { "file1.txt", "file2.txt" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.Positional.Should().HaveCount(2);
        result.GetPositional(0).Should().Be("file1.txt");
        result.GetPositional(1).Should().Be("file2.txt");
    }

    [Fact]
    public void Parse_MixedOptionsAndPositionals()
    {
        // Arrange
        var args = new[] { "--verbose", "file1.txt", "-o", "output.txt" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("verbose").Should().BeTrue();
        result.HasOption("o").Should().BeTrue();
        result.GetOptionValue("o").Should().Be("output.txt");
        result.Positional.Should().ContainSingle().Which.Should().Be("file1.txt");
    }

    [Fact]
    public void Parse_DoubleDash_EndsOptions()
    {
        // Arrange
        var args = new[] { "--verbose", "--", "--not-an-option" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("verbose").Should().BeTrue();
        result.Positional.Should().ContainSingle().Which.Should().Be("--not-an-option");
    }

    [Fact]
    public void Parse_FlagOption_WithoutValue()
    {
        // Arrange
        var args = new[] { "--verbose", "--dry-run" };

        // Act
        var result = _parser.Parse(args);

        // Assert
        result.HasOption("verbose").Should().BeTrue();
        result.HasOption("dry-run").Should().BeTrue();
        result.GetOptionValue("verbose").Should().BeNull();
    }
}
