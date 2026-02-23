using Agile360.Application.Common;
using FluentAssertions;
using Xunit;

namespace Agile360.UnitTests.Application.Common;

public class ResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsIsSuccessTrueAndValueSet()
    {
        var value = 42;
        var result = Result<int>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_WithError_ReturnsIsSuccessFalseAndErrorSet()
    {
        var error = "Something failed";
        var result = Result<string>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }
}
