using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Configuration;

public static class OptionsServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddValidatedModuleOptions<TOptions, TValidator>(string sectionName)
            where TOptions: ModuleOptions
            where TValidator : class, IValidator<TOptions>
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sectionName);
            serviceCollection.AddSingleton<IValidator<TOptions>, TValidator>();
            serviceCollection.AddOptions<TOptions>(sectionName).BindConfiguration(sectionName).ValidateOnStart();
            return serviceCollection;
        }
    }
}