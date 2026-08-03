using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.Logging;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.RequestPipeline;
using Scalar.AspNetCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddModules(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddCustomProblemDetails();
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(builder.Configuration).ReadFrom.Services(services));

WebApplication webApplication = builder.Build();

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
webApplication.UseEnrichedSerilogRequestLogging();
webApplication.UseHttpsRedirection();
webApplication.UseMiddleware<CorrelationIdMiddleware>();
webApplication.UseStatusCodePages();

webApplication.Run();