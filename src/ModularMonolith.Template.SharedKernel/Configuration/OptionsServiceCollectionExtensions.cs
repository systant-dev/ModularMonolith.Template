using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Template.SharedKernel.Configuration;

public static class OptionsServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddValidatedOptions<TOptions, TValidator>(string sectionName)
            where TOptions : class, IModuleOptions
            where TValidator : class, IValidator<TOptions>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);

            serviceCollection.AddSingleton<IValidator<TOptions>, TValidator>();
            serviceCollection.AddSingleton<IValidateOptions<TOptions>, FluentValidateModuleOptions<TOptions>>();
            serviceCollection.AddOptions<TOptions>()
                             .BindConfiguration($"{ModuleConfiguration.SectionName}:{sectionName}")
                             .ValidateOnStart();

            return serviceCollection;
        }
    }
}