using Domain.Libraries;
using Domain.Libraries.Physical;
using FastEndpoints;
using Libraries.Contracts.Features;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Libraries.Backend.Features;

public class CreateLibraryEndpoint(AppDbContext dbContext) : Endpoint<CreateLibraryRequest>
{
    public override void Configure()
    {
        Post("/libraries");
    }
    
    public override async Task HandleAsync(CreateLibraryRequest req, CancellationToken ct)
    {
        var alreadyPresent = await dbContext.Libs.Libraries.AnyAsync(l => l.Name == new LibraryName(req.Name), cancellationToken: ct);

        if (alreadyPresent)
        {
            ThrowError(l => l.Name, $"Library with the name '{req.Name}' already present");
        }

        var library = req.Kind switch
        {
            LibraryKind.Physical => PhysicalLibrary.Create(new LibraryName(req.Name),
                req.Sources.Select(s => new PhysicalLibrarySource(Enum.Parse<LibraryMediaKind>(s.Kind.ToString()),
                    new PhysicalPath(s.Path)))),
            LibraryKind.Virtual => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        dbContext.Add(library);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateLibraryResponse(Guid.NewGuid()), cancellation: ct);
    }
}