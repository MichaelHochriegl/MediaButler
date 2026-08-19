using System.Net;
using AwesomeAssertions;
using Domain.Libraries;
using FastEndpoints;
using FastEndpoints.Testing;
using Libraries.Backend.Features.Physical;
using Libraries.Contracts.Features.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace Libraries.Backend.Tests.Integration.Features.Physical.CreateLibrary;

public class CreateLibraryTests(CreateLibraryAppFixture app) : TestBase<CreateLibraryAppFixture>
{
    protected override async ValueTask SetupAsync()
    {
        await base.SetupAsync();
        await app.ResetDatabaseAsync();
    }

    [Fact]
    public async Task Given_ValidPhysicalLibrary_Should_CreateLibrary()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Physical Test Library",
        [
            new CreatePhysicalLibrarySource("//mnt/movies", MediaKind.Movies),
            new CreatePhysicalLibrarySource("//mnt/tv-shows", MediaKind.TvShows),
        ]);

        // Act
        var (response, result) =
            await app.Client
                .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(
                    request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/libraries/{result.Id}");
    }

    [Fact]
    public async Task Given_MultipleSourcesWithSameMediaKind_Should_CreateLibrary()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Multi-Source Movie Library",
        [
            new CreatePhysicalLibrarySource("//mnt/movies", MediaKind.Movies),
            new CreatePhysicalLibrarySource("//mnt/movies-archive", MediaKind.Movies),
        ]);

        // Act
        var (response, result) =
            await app.Client
                .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(
                    request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // TODO: use the proper endpoints once they are in place, assert with DbContext is just a temp solution
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var library = await db.Libs.PhysicalLibraries
            .Include(l => l.Sources)
            .SingleAsync(l => l.Id == result.Id, cancellationToken: TestContext.Current.CancellationToken);

        library.Sources.Should().HaveCount(2);
        library.Sources.Should().OnlyContain(source => source.MediaKind == LibraryMediaKind.Movies);
        library.Sources.Select(source => source.Path.Value)
            .Should().BeEquivalentTo("//mnt/movies", "//mnt/movies-archive");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Given_LibraryWithInvalidName_Should_ReturnBadRequest(string invalidName)
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest(invalidName,
            [new CreatePhysicalLibrarySource("//mnt/movies", MediaKind.Movies)]);

        // Act
        var (response, problemDetails) =
            await app.Client.POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(
                request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().ContainSingle(error =>
            error.Name == "name" &&
            error.Reason == "'name' must not be empty.");
    }

    [Fact]
    public async Task Given_LibraryWithSameNameAlreadyExists_Should_ReturnBadRequest()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Duplicate Physical Library",
            [new CreatePhysicalLibrarySource("//mnt/movies", MediaKind.Movies)]);

        var (createResponse, _) =
            await app.Client
                .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(
                    request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var (response, problemDetails) =
            await app.Client.POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(
                request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().ContainSingle(error =>
            error.Name == "name" &&
            error.Reason == $"Library with the name '{request.Name}' already present");
    }

    [Fact]
    public async Task Given_NoSources_Should_ReturnBadRequest()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Physical Library", []);

        // Act
        var (response, problemDetails) =
            await app.Client.POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(
                request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().ContainSingle(error => error.Name == "sources");
    }

    [Fact]
    public async Task Given_NullSources_Should_ReturnBadRequest()
    {
        // Arrange
        var request = new CreatePhysicalLibraryRequest("Physical Library", null!);

        // Act
        var (response, problemDetails) =
            await app.Client.POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(
                request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().ContainSingle(error => error.Name == "sources");
    }
}
