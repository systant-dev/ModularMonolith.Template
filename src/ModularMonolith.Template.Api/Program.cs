using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.HealthChecks;
using ModularMonolith.Template.Api.Logging;
using ModularMonolith.Template.Api.Messaging;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.Observability;
using ModularMonolith.Template.Api.Outbox;
using ModularMonolith.Template.Api.RequestPipeline;
using ModularMonolith.Template.Api.Security;
using ModularMonolith.Template.Api.Versioning;
using ModularMonolith.Template.SharedKernel.Features;
using ModularMonolith.Template.SharedKernel.Idempotency;
using ModularMonolith.Template.SharedKernel.Outbox;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

bool appConfigurationConnected = builder.Configuration.ConnectAppConfigurationFeatureFlags(builder.Environment.IsDevelopment());

builder.Services.AddMessaging();
builder.Services.AddCustomProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddModuleApiVersioning();
builder.Services.AddModuleRateLimiting();
builder.Services.AddModuleRequestTimeouts();
builder.Services.AddModuleCors(builder.Configuration);
builder.Services.AddModuleAuthentication(builder.Configuration);
builder.Services.AddModuleAuthorization();
builder.Services.AddIdempotency();
builder.Services.AddModuleFeatureManagement();
if (appConfigurationConnected)
{
    builder.Services.AddModuleAppConfiguration();
}
builder.Services.AddModules(builder.Configuration);
builder.Services.AddOutboxProcessing(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.AddSerilogWithRedaction();

WebApplication webApplication = builder.Build();

if (webApplication.Configuration.GetValue("Modules:RunDatabaseInitialisationOnStartup", defaultValue: true))
{
    await webApplication.Services.MigrateModuleDatabasesAsync();
}

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi().WithDocumentPerVersion().AllowAnonymous();
    webApplication.MapScalarApiReference().AllowAnonymous();
}

webApplication.UseMiddleware<CorrelationIdMiddleware>();
if (appConfigurationConnected)
{
    webApplication.UseModuleAppConfiguration();
}
webApplication.UseCustomExceptionHandler();
webApplication.UseHttpsRedirection();
webApplication.UseEnrichedSerilogRequestLogging();
webApplication.UseCors(CorsExtensions.DefaultPolicyName);
webApplication.UseRateLimiter();
webApplication.UseRequestTimeouts();
webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseStatusCodePages();

webApplication.MapModuleEndpoints(webApplication.Configuration);
webApplication.MapModuleHealthChecks();
webApplication.MapOutboxDeadLetterEndpoints();

webApplication.Run();
