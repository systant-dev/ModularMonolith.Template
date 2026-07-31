using ModularMonolith.Template.Api.Modular;

WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);
webApplicationBuilder.Services.AddOpenApi();
webApplicationBuilder.Services.AddModules(webApplicationBuilder.Configuration);

WebApplication webApplication = webApplicationBuilder.Build();

// Configure the HTTP request pipeline.
if (webApplication.Environment.IsDevelopment())
{
    webApplication.MapOpenApi();
}

webApplication.UseHttpsRedirection();
webApplication.Run();