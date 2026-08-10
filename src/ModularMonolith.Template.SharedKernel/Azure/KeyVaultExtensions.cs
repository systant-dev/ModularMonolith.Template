extern alias AzureCore;
using Microsoft.Extensions.Configuration;
using TokenCredential = AzureCore::Azure.Core.TokenCredential;

namespace ModularMonolith.Template.SharedKernel.Azure;

public static class KeyVaultExtensions
{
    extension(ConfigurationManager configurationManager)
    {
        public void ConnectKeyVault(bool isDevelopment, string key = "KeyVault:Url", bool optional = false)
        {
            string? keyVaultUrl = configurationManager[key];
            if (string.IsNullOrWhiteSpace(keyVaultUrl))
            {
                if (optional)
                {
                    return;
                }

                throw new InvalidOperationException($"Missing configuration value: {key}");
            }

            Uri keyVaultUri = new(keyVaultUrl);
            TokenCredential credential = AzureCredentialFactory.CreateDefault(isDevelopment);
            configurationManager.AddAzureKeyVault(keyVaultUri, credential);
        }
    }
}