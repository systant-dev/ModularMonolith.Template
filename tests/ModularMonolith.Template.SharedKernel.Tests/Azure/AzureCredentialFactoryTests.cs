using Azure.Core;
using FluentAssertions;
using ModularMonolith.Template.SharedKernel.Azure;

namespace ModularMonolith.Template.SharedKernel.Tests.Azure;

public sealed class AzureCredentialFactoryTests
{
    #region Test Method Declarations

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateDefault_ShouldReturnTokenCredential(bool isDevelopment)
    {
        TokenCredential credential = AzureCredentialFactory.CreateDefault(isDevelopment);

        credential.Should().NotBeNull();
    }

    #endregion
}