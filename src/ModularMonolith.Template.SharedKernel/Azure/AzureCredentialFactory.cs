using Azure.Core;
using Azure.Identity;

namespace ModularMonolith.Template.SharedKernel.Azure;

public static class AzureCredentialFactory
{
    #region Public Method Declarations

    public static TokenCredential CreateDefault(bool isDevelopment)
    {
        DefaultAzureCredentialOptions options = new()
        {
            ExcludeManagedIdentityCredential = isDevelopment,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeVisualStudioCredential = !isDevelopment,
            ExcludeAzurePowerShellCredential = true,
        };
        return new DefaultAzureCredential(options);
    }

    #endregion
}