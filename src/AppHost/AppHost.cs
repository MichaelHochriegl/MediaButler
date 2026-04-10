using Projects;
using ServiceDiscovery;

var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Backend>(Descriptors.Backend);

builder.AddProject<Frontend>(Descriptors.Frontend)
    .WithReference(backend)
    .WaitFor(backend);

builder.Build().Run();