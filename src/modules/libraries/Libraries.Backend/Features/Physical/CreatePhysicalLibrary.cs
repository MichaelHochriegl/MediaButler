using Domain.Libraries;
using Domain.Libraries.Physical;
using FastEndpoints;
using Libraries.Contracts.Features.Physical;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Persistence;

namespace Libraries.Backend.Features.Physical;

public class CreatePhysicalLibraryEndpoint(AppDbContext dbContext) : Endpoint<CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>
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
        var alreadyPresent = await dbContext.Libs.Libraries.AnyAsync(l => l.Name == new LibraryName(req.Name), cancellationToken: ct);

        if (alreadyPresent)
        {
            ThrowError(l => l.Name, $"Library with the name '{req.Name}' already present");
        }

        var sources = req.Sources
            .Select(s => new PhysicalLibrarySource(
                (LibraryMediaKind)s.Kind,
                new PhysicalPath(s.Path)))
            .ToList();

        if (sources.Select(s => s.Path).Distinct().Count() != sources.Count)
        {
            ThrowError(l => l.Sources, "Duplicate sources are not allowed");
            return;
        }
        
        var library = PhysicalLibrary.Create(new LibraryName(req.Name),
            req.Sources.Select(s => new PhysicalLibrarySource((LibraryMediaKind)s.Kind,
                new PhysicalPath(s.Path))));

        try
        {
            dbContext.Add(library);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
                                          {
                                              SqlState: PostgresErrorCodes.UniqueViolation,
                                              ConstraintName: "IX_Library_Name"
                                          })
        {
            ThrowError(r => r.Name, $"Library with the name '{req.Name}' already present");
            return;
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException
                                          {
                                              SqlState: PostgresErrorCodes.UniqueViolation,
                                              ConstraintName: "IX_LibrarySource_Path"
                                          })
        {
            ThrowError(r => r.Sources, "One or more sources paths are already managed by another library");
            return;
        }

        await Send.CreatedAtAsync<GetLibraryByIdEndpoint>(new { id = library.Id },
            new CreatePhysicalLibraryResponse(library.Id), cancellation: ct);
    }
}
