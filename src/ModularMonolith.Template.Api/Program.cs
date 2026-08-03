using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.RequestPipeline;
using Scalar.AspNetCore;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);
webApplicationBuilder.Services.AddModules(webApplicationBuilder.Configuration);

webApplicationBuilder.Services.AddOpenApi();
webApplicationBuilder.Services.AddCustomProblemDetails();

WebApplication webApplication = webApplicationBuilder.Build();

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi();
    webApplication.MapScalarApiReference(options =>
    {
        options.EnableDarkMode();
    });
}

webApplication.MapModuleEndpoints(webApplication.Configuration);

webApplication.UseCustomExceptionHandler();
webApplication.UseHttpsRedirection();
webApplication.UseMiddleware<CorrelationIdMiddleware>();
webApplication.UseStatusCodePages();

webApplication.Run();