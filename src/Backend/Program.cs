using FastEndpoints;
using FastEndpoints.Swagger;
using Libraries.Backend;
using Modules.Common;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterLibrariesModule();

builder.Services.AddFastEndpoints(options =>
{
    options.Assemblies = ModuleDescriptors.BackendAssemblies;
}).SwaggerDocument(options =>
{
    options.DocumentSettings = s =>
    {
        s.Title = "MediaButler API";
        s.Version = "v1";
    };
});

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseFastEndpoints(c =>
{
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
