namespace ModularMonolith.Template.Api.RequestPipeline;

public static class CorsExtensions
{
    #region Field Declarations

    public const string DefaultPolicyName = "default";

    public const string ConfigurationSectionName = "Cors";

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModuleCors(IConfiguration configuration)
        {
            string[] allowedOrigins = configuration.GetSection($"{ConfigurationSectionName}:AllowedOrigins")
                                                   .Get<string[]>() ?? [];

            serviceCollection.AddCors(options =>
            {
                options.AddPolicy(DefaultPolicyName, policy =>
                {
                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    }
                });
            });

            return serviceCollection;
        }
    }
}
