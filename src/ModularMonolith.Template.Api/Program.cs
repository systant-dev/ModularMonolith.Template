using ModularMonolith.Template.Api.Modular;
using Scalar.AspNetCore;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);
webApplicationBuilder.Services.AddOpenApi();
webApplicationBuilder.Services.AddModules(webApplicationBuilder.Configuration);

WebApplication webApplication = webApplicationBuilder.Build();

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi();
    webApplication.MapScalarApiReference(options =>
    {
        options.EnableDarkMode();
    });
}

webApplication.UseHttpsRedirection();
webApplication.MapModuleEndpoints(webApplication.Configuration);
webApplication.Run();