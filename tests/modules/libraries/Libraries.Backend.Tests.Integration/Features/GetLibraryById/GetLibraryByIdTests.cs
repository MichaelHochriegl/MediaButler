using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using FastEndpoints;
using FastEndpoints.Testing;
using Libraries.Backend.Features;
using Libraries.Backend.Features.Physical;
using Libraries.Contracts.Features;
using Libraries.Contracts.Features.Physical;

namespace Libraries.Backend.Tests.Integration.Features.GetLibraryById;

public class GetLibraryByIdTests(GetLibraryByIdAppFixture app) : TestBase<GetLibraryByIdAppFixture>
{
    protected override async ValueTask SetupAsync()
    {
        await base.SetupAsync();
        await app.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Given_ExistingLibrary_Should_ReturnLibrary()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Movies",
        [
            new CreatePhysicalLibrarySource("//mnt/movies", MediaKind.Movies),
        ]);
        var beforeCreation = DateTimeOffset.UtcNow;

        var (createResponse, created) =
            await app.Client
                .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(
                    request);
        var afterCreation = DateTimeOffset.UtcNow;
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Should().NotBeNull();

        // Act
        var (response, result) =
            await app.Client.GETAsync<GetLibraryByIdEndpoint, GetLibraryByIdRequest, GetLibraryByIdResponse>(
                new GetLibraryByIdRequest(created.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result.Id.Should().Be(created.Id);
        result.Name.Should().Be(request.Name);
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation).And.BeOnOrBefore(afterCreation);
        result.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task Given_UnknownLibraryId_Should_ReturnNotFound()
    {
        // Act
        var response = await app.Client.GETAsync<GetLibraryByIdEndpoint, GetLibraryByIdRequest>(new GetLibraryByIdRequest(Guid.NewGuid()));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
