using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.HealthChecks;
using ModularMonolith.Template.Api.Logging;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.Observability;
using ModularMonolith.Template.Api.RequestPipeline;
using ModularMonolith.Template.Api.Versioning;
using ModularMonolith.Template.SharedKernel.Compliance;
using ModularMonolith.Template.SharedKernel.Messaging;
using Scalar.AspNetCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddModules(builder.Configuration);

builder.Services.AddInProcessMessaging();
builder.Services.AddOpenApi();
builder.Services.AddCustomProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddModuleApiVersioning();
builder.Services.AddModuleRateLimiting();
builder.Services.AddModuleRequestTimeouts();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(builder.Configuration).ReadFrom.Services(services));

builder.Logging.EnableRedaction();
builder.Services.AddRedaction(redaction =>
{
    redaction.SetRedactor<MaskingRedactor>(new DataClassificationSet(DataTaxonomy.PersonalData));
    redaction.SetRedactor<ErasingRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));
});

WebApplication webApplication = builder.Build();

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi();
    webApplication.MapScalarApiReference(options =>
    {
        options.EnableDarkMode();
    });
}

webApplication.UseCustomExceptionHandler();
webApplication.UseHttpsRedirection();
webApplication.UseMiddleware<CorrelationIdMiddleware>();
webApplication.UseEnrichedSerilogRequestLogging();
webApplication.UseRateLimiter();
webApplication.UseRequestTimeouts();
webApplication.UseStatusCodePages();

webApplication.MapModuleEndpoints(webApplication.Configuration);
webApplication.MapModuleHealthChecks();

webApplication.Run();

public partial class Program;
