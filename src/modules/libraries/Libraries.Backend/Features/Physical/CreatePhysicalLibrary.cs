using Domain.Libraries;
using Domain.Libraries.Physical;
using FastEndpoints;
using Libraries.Contracts.Features;
using Libraries.Contracts.Features.Physical;
using Microsoft.EntityFrameworkCore;
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

        var library = PhysicalLibrary.Create(new LibraryName(req.Name),
            req.Sources.Select(s => new PhysicalLibrarySource(Enum.Parse<LibraryMediaKind>(s.Kind.ToString()),
                new PhysicalPath(s.Path))));
        
        dbContext.Add(library);
        await dbContext.SaveChangesAsync(ct);

        await Send.CreatedAtAsync<GetLibraryByIdEndpoint>(new { id = library.Id.Value },
            new CreatePhysicalLibraryResponse(library.Id.Value), cancellation: ct);
    }
}
