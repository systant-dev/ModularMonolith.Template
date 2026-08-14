using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Serialisation;
using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Tests.Serialisation;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Serialisation)]
public sealed class JsonDefaultsTests
{
    #region Test Method Declarations

    [Fact]
    public void Web_ShouldNotBeNull()
    {
        JsonDefaults.Web.Should().NotBeNull();
    }

    [Fact]
    public void Web_ShouldUseCamelCasePropertyNaming()
    {
        JsonDefaults.Web.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
    }

    [Fact]
    public void Web_ShouldBeCaseInsensitive()
    {
        JsonDefaults.Web.PropertyNameCaseInsensitive.Should().BeTrue();
    }

    [Fact]
    public void Web_ShouldReturnSameInstance()
    {
        JsonDefaults.Web.Should().BeSameAs(JsonDefaults.Web);
    }

    [Fact]
    public void Web_ShouldSerialiseUsingCamelCase()
    {
        var value = new { SampleValue = 1 };
        string json = JsonSerializer.Serialize(value, JsonDefaults.Web);

        json.Should().Contain("sampleValue");
    }

    #endregion
}
