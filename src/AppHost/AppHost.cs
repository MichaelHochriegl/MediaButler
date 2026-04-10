using Projects;
using ServiceDiscovery;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Backend>(Descriptors.Backend);

builder.Build().Run();