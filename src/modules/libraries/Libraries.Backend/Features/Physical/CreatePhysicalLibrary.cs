using System.Data;
using Domain.Libraries;
using Domain.Libraries.Physical;
using Domain.Storage;
using FastEndpoints;
using Libraries.Contracts.Features.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Persistence;
using Storage.Integration;

namespace Libraries.Backend.Features.Physical;

public class CreatePhysicalLibraryEndpoint(
    IServiceScopeFactory scopeFactory,
    AppDbContext dbContext,
    IStorageDirectoryResolver resolver)
    : Endpoint<CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>
{
    public override void Configure()
    {
        Post("/libraries/physical");
        Summary(s =>
        {
            s.Summary = "Create a library";
            s.Description =
                "Creates a new physical library composed of filesystem paths.";

            s.Response<CreatePhysicalLibraryResponse>(201, "The library was created successfully.");
            s.Response(400, "The request was invalid, or a library with the same name already exists.");
        });
    }

    public override async Task HandleAsync(CreatePhysicalLibraryRequest req, CancellationToken ct)
    {
        var alreadyPresent =
            await dbContext.Libs.Libraries.AnyAsync(l => l.Name == new LibraryName(req.Name), cancellationToken: ct);

        if (alreadyPresent)
        {
            ThrowError(l => l.Name, $"Library with the name '{req.Name}' already present");
        }

        try
        {
            var sources = new List<PhysicalLibrarySource>();

            foreach (var (source, i) in req.Sources.Select((s, i) => (s, i)))
            {
                try
                {
                    var locationId = new StorageLocationId(source.StorageLocationId);
                    var resolvedDirectory = resolver.ResolveDirectory(locationId, source.RelativePath);

                    sources.Add(new PhysicalLibrarySource((LibraryMediaKind)source.Kind, locationId,
                        resolvedDirectory.RelativePath));
                }
                catch (StorageLocationNotFoundException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure(
                        $"sources[{i}].storageLocationId",
                        "The storage location is not configured."));
                }
                catch (StorageDirectoryNotAccessibleException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure(
                        $"sources[{i}].relativePath",
                        "The storage directory is not accessible."));
                }
                catch (InvalidStoragePathException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure(
                        $"sources[{i}].relativePath",
                        "The path is invalid or traverses a symbolic link."));
                }
                catch (StorageDirectoryNotFoundException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure($"sources[{i}].relativePath",
                        "The path is invalid or does not exist."));
                }
            }

            ThrowIfAnyErrors();

            if (sources.Select(s => new { s.StorageLocationId, s.RelativePath }).Distinct().Count() != sources.Count)
            {
                ThrowError(l => l.Sources, "Duplicate sources are not allowed");
                return;
            }

            var locationIds = sources
                .Select(s => s.StorageLocationId)
                .Distinct()
                .ToArray();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            var libraryId = await strategy.ExecuteAsync<Guid>(async attempCt =>
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var attempContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await using var transaction = await attempContext.Database
                    .BeginTransactionAsync(IsolationLevel.ReadCommitted, attempCt);

                await attempContext.Database.AcquirePhysicalLibrarySourceLocks(locationIds, attempCt);

                var existingSources = await dbContext.Libs.LibrarySources
                    .OfType<PhysicalLibrarySource>()
                    .AsNoTracking()
                    .Where(s => locationIds.Contains(s.StorageLocationId))
                    .ToListAsync(attempCt);

                if (sources.Any(s => existingSources.Any(es => es.OverlapsWith(s))))
                {
                    ThrowError(r => r.Sources,
                        "One or more source paths overlap sources managed by another library.");
                }

                var attemptSources = sources
                    .Select(s => new PhysicalLibrarySource(s.MediaKind,
                        s.StorageLocationId,
                        s.RelativePath))
                    .ToArray();

                var library = PhysicalLibrary.Create(new LibraryName(req.Name), attemptSources);

                attempContext.Add(library);
                await attempContext.SaveChangesAsync(attempCt);
                await transaction.CommitAsync(attempCt);

                return library.Id;
            }, ct);


            await Send.CreatedAtAsync<GetLibraryByIdEndpoint>(new { id = libraryId },
                new CreatePhysicalLibraryResponse(libraryId), cancellation: ct);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
                                          {
                                              SqlState: PostgresErrorCodes.UniqueViolation,
                                              ConstraintName: "IX_Library_Name"
                                          })
        {
            ThrowError(r => r.Name, $"Library with the name '{req.Name}' already present");
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
                                          {
                                              SqlState: PostgresErrorCodes.UniqueViolation,
                                              ConstraintName: "IX_LibrarySource_StorageLocationId_RelativePath"
                                          })
        {
            ThrowError(r => r.Sources, "One or more sources paths are already managed by another library");
        }
        catch (OverlappingPhysicalLibrarySourceException)
        {
            ThrowError(r => r.Sources, "One or more source paths are overlapping each other");
        }
    }
}