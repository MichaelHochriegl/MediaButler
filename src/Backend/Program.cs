using System.Text.Json.Serialization;
using Backend;
using Backend.Data;
using FastEndpoints;
using FastEndpoints.Swagger;
using Libraries.Backend;
using Modules.Common;
using Persistence;
using ServiceDiscovery;
using Storage.Backend;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterStorageModule();
builder.RegisterLibrariesModule();

builder.AddNpgsqlDbContext<AppDbContext>(Descriptors.Database);

builder.Services.AddSingleton<StartupState>();
builder.Services.AddHostedService<MigrationService>();
builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseReadyHealthCheck>("database_ready");

builder.Services.AddFastEndpoints(options =>
{
    options.Assemblies = [.. ModuleDescriptors.BackendAssemblies, .. ModuleDescriptors.ContractAssemblies];
    options.IncludeAbstractValidators = true;
}).SwaggerDocument(options =>
{
    options.DocumentSettings = s =>
    {
        s.Title = "MediaButler API";
        s.Version = "v1";
    };
});

builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource(MigrationService.ActivitySourceName);
    });

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseFastEndpoints(c =>
{
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
    c.Endpoints.RoutePrefix = "api";
    c.Endpoints.Configurator = e =>
    {
        e.AllowAnonymous();
    };
    c.Errors.UseProblemDetails();
});

app.UseSwaggerGen();

app.Run();
