using Domain.Libraries;
using FastEndpoints;
using Libraries.Contracts.Features;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Libraries.Backend.Features;

internal class GetLibraryByIdEndpoint(AppDbContext dbContext) 
    : Endpoint<GetLibraryByIdRequest, GetLibraryByIdResponse, GetLibraryByIdMapper>
{
    public override void Configure()
    {
        Get("/libraries/{id}");
    }

    public override async Task HandleAsync(GetLibraryByIdRequest req, CancellationToken ct)
    {
        var library = await dbContext.Libs.Libraries.FirstOrDefaultAsync(l => l.Id == new LibraryId(req.Id), ct);
        if (library == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(Map.FromEntity(library), ct);
    }
}

internal class GetLibraryByIdMapper : Mapper<GetLibraryByIdRequest, GetLibraryByIdResponse, Library>
{
    public override GetLibraryByIdResponse FromEntity(Library e) => new(e.Id.Value, e.Name.Value, e.CreatedAt, e.UpdatedAt);
}