using Domain.Libraries;
using Domain.Libraries.Physical;
using Domain.Storage;
using FastEndpoints;
using Libraries.Contracts.Features.Physical;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence;
using Storage.Integration;

namespace Libraries.Backend.Features.Physical;

public class CreatePhysicalLibraryEndpoint(AppDbContext dbContext, IStorageDirectoryResolver resolver)
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
                    var root = resolver.ResolveDirectory(locationId, string.Empty);

                    sources.Add(new PhysicalLibrarySource((LibraryMediaKind)source.Kind, locationId, root.RelativePath));
                }
                catch (StorageLocationNotFoundException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure(
                        $"sources[{i}].storageLocationId",
                        "The storage location is not configured."));
                }
                catch (InvalidStoragePathException)
                {
                    AddError(new FluentValidation.Results.ValidationFailure(
                        $"sources[{i}].relativePath",
                        "The path is invalid or traverses a symbolic link."));
                }
            }
            ThrowIfAnyErrors();

            if (sources.Select(s => new { s.StorageLocationId, s.RelativePath }).Distinct().Count() != sources.Count)
            {
                ThrowError(l => l.Sources, "Duplicate sources are not allowed");
                return;
            }

            var library = PhysicalLibrary.Create(new LibraryName(req.Name), sources);


            dbContext.Add(library);
            await dbContext.SaveChangesAsync(ct);

            await Send.CreatedAtAsync<GetLibraryByIdEndpoint>(new { id = library.Id },
                new CreatePhysicalLibraryResponse(library.Id), cancellation: ct);
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
    }
}