using Domain.Libraries;
using FastEndpoints;
using Libraries.Contracts.Features;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Libraries.Backend.Features;

public class GetLibraryByIdEndpoint(AppDbContext dbContext)
    : Endpoint<GetLibraryByIdRequest, GetLibraryByIdResponse, GetLibraryByIdMapper>
{
    public override void Configure()
    {
        Get("/libraries/{id}");
        Summary(s =>
        {
            s.Summary = "Get a library by ID";
            s.Description = "Retrieves a library's name and timestamps by its unique identifier.";

            s.Response<GetLibraryByIdResponse>(200, "The library was found and returned.");
            s.Response(404, "No library was found with the provided ID.");
        });
    }

    public override async Task HandleAsync(GetLibraryByIdRequest req, CancellationToken ct)
    {
        var library = await dbContext.Libs.Libraries.FirstOrDefaultAsync(l => l.Id == req.Id, ct);
        if (library == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(Map.FromEntity(library), ct);
    }
}

public class GetLibraryByIdMapper : Mapper<GetLibraryByIdRequest, GetLibraryByIdResponse, Library>
{
    public override GetLibraryByIdResponse FromEntity(Library e) => new(e.Id, e.Name.Value, e.CreatedAt, e.UpdatedAt);
}
