using ModularMonolith.Template.Api.Exceptions;
using ModularMonolith.Template.Api.HealthChecks;
using ModularMonolith.Template.Api.Logging;
using ModularMonolith.Template.Api.Modular;
using ModularMonolith.Template.Api.Observability;
using ModularMonolith.Template.Api.RequestPipeline;
using ModularMonolith.Template.Api.Security;
using ModularMonolith.Template.Api.Versioning;
using ModularMonolith.Template.SharedKernel.Messaging;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInProcessMessaging();
builder.Services.AddOpenApi("v1");
builder.Services.AddCustomProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddModuleApiVersioning();
builder.Services.AddModuleRateLimiting();
builder.Services.AddModuleRequestTimeouts();
builder.Services.AddModuleCors(builder.Configuration);
builder.Services.AddModuleAuthentication(builder.Configuration);
builder.Services.AddModuleAuthorization();
builder.Services.AddModules(builder.Configuration);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.AddSerilogWithRedaction();

WebApplication webApplication = builder.Build();

if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi().AllowAnonymous();
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