using Projects;
using ServiceDiscovery;

var builder = DistributedApplication.CreateBuilder(args);

var databaseServer = builder.AddPostgres(Descriptors.DatabaseServer)
    .WithImageTag(Tags.PostgresTag)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume(Descriptors.DatabaseVolume);

var database = databaseServer.AddDatabase(Descriptors.Database);

var backend = builder.AddProject<Backend>(Descriptors.Backend)
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Frontend>(Descriptors.Frontend)
    .WithReference(backend)
    .WaitFor(backend);

builder.Build().Run();