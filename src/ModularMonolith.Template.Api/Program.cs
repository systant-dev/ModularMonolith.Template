using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.HealthChecks;
using ModularMonolith.Template.Api.Logging;
using ModularMonolith.Template.Api.Messaging;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.Observability;
using ModularMonolith.Template.Api.RequestPipeline;
using ModularMonolith.Template.Api.Security;
using ModularMonolith.Template.Api.Versioning;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Outbox;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInProcessMessaging();
builder.Services.AddGeneratedMessageDispatch();
builder.Services.AddCustomProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddModuleApiVersioning();
builder.Services.AddModuleRateLimiting();
builder.Services.AddModuleRequestTimeouts();
builder.Services.AddModuleCors(builder.Configuration);
builder.Services.AddModuleAuthentication(builder.Configuration);
builder.Services.AddModuleAuthorization();
builder.Services.AddModules(builder.Configuration);
builder.Services.AddOutboxProcessing(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.AddSerilogWithRedaction();

WebApplication webApplication = builder.Build();

// Initialise each module's database (EF migrations or Dapper DDL) on startup. This runs in every
// environment so the in-memory example databases are created before requests are served.
await webApplication.Services.MigrateModuleDatabasesAsync();

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi().WithDocumentPerVersion().AllowAnonymous();
    webApplication.MapScalarApiReference().AllowAnonymous();
}

webApplication.UseCustomExceptionHandler();
webApplication.UseHttpsRedirection();
webApplication.UseMiddleware<CorrelationIdMiddleware>();
webApplication.UseEnrichedSerilogRequestLogging();
webApplication.UseCors(CorsExtensions.DefaultPolicyName);
webApplication.UseRateLimiter();
webApplication.UseRequestTimeouts();
webApplication.UseAuthentication();
webApplication.UseAuthorization();
webApplication.UseStatusCodePages();

webApplication.MapModuleEndpoints(webApplication.Configuration);
webApplication.MapModuleHealthChecks();

webApplication.Run();