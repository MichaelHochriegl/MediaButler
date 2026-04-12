using FastEndpoints;
using Libraries.Contracts.features;

namespace Libraries.Backend.features;

public class CreateLibraryEndpoint : Endpoint<CreateLibraryRequest>
{
    public override void Configure()
    {
        Post("/libraries");
        
    }
    
    public override async Task HandleAsync(CreateLibraryRequest req, CancellationToken ct)
    {
        await Send.OkAsync(new CreateLibraryResponse(Guid.NewGuid()), cancellation: ct);
    }
}