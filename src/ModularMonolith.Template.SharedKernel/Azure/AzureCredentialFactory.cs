extern alias AzureCore;
using Azure.Identity;
using TokenCredential = AzureCore::Azure.Core.TokenCredential;

namespace ModularMonolith.Template.SharedKernel.Azure;

public static class AzureCredentialFactory
{
    #region Public Method Declarations

    public static TokenCredential CreateDefault(bool isDevelopment, Action<DefaultAzureCredentialOptions>? configure = null)
    {
        DefaultAzureCredentialOptions options = new()
        {
            ExcludeManagedIdentityCredential = isDevelopment,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeVisualStudioCredential = !isDevelopment,
            ExcludeAzurePowerShellCredential = true,
        };

        configure?.Invoke(options);

        return new DefaultAzureCredential(options);
    }

    #endregion
}