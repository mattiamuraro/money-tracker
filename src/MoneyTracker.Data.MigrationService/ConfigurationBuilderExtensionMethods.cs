using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

namespace MoneyTracker.Data.MigrationService;

internal static class ConfigurationBuilderExtensionMethods
{
    internal static void AddEnvironmentSecretProviders(this HostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Environment.IsDevelopment())
            return;

        var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
        if (string.IsNullOrWhiteSpace(keyVaultUri))
            throw new InvalidOperationException("KeyVault:VaultUri must be configured outside Development.");

        if (!Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var vaultUri))
            throw new InvalidOperationException("KeyVault:VaultUri must be a valid absolute URI.");

        builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
    }
}
