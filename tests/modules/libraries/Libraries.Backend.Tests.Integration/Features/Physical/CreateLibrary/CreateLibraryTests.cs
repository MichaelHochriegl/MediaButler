using System.Net;
using AwesomeAssertions;
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
    public async Task Given_ValidStorageSources_Should_CreateAndPersistLogicalSources()
    {
        var request = CreateRequest("Physical Test Library",
            Source("media", "./Movies/Action/..", MediaKind.Movies),
            Source("media", "TV Shows", MediaKind.TvShows));

        var (response, result) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/libraries/{result.Id}");

        var sources = await GetPersistedSourcesAsync(result.Id);
        sources.Should().BeEquivalentTo([
            new PersistedSource("media", "Movies", (int)MediaKind.Movies),
            new PersistedSource("media", "TV Shows", (int)MediaKind.TvShows)
        ]);
        sources.Should().OnlyContain(source =>
            !source.RelativePath.StartsWith(app.MediaRootPath, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Given_MultipleSourcesWithSameMediaKind_Should_CreateLibrary()
    {
        var request = CreateRequest("Multi-Source Movie Library",
            Source("media", "Movies", MediaKind.Movies),
            Source("media", "Movies-Archive", MediaKind.Movies));

        var (response, result) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sources = await GetPersistedSourcesAsync(result.Id);
        sources.Should().HaveCount(2);
        sources.Should().OnlyContain(source => source.MediaKind == (int)MediaKind.Movies);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Given_LibraryWithInvalidName_Should_ReturnBadRequest(string invalidName)
    {
        var request = CreateRequest(invalidName, Source("media", "Movies", MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "name");
    }

    [Fact]
    public async Task Given_LibraryWithSameNameAlreadyExists_Should_ReturnBadRequest()
    {
        var first = CreateRequest("Duplicate Physical Library",
            Source("media", "Movies", MediaKind.Movies));
        var duplicate = CreateRequest("Duplicate Physical Library",
            Source("media", "TV Shows", MediaKind.TvShows));
        var (createdResponse, _) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(first);
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(duplicate);

        AssertBadRequestFor(response, problemDetails, "name");
    }

    [Fact]
    public async Task Given_ConcurrentRequestsWithSameName_Should_CreateOneLibraryAndRejectTheOthers()
    {
        var request = CreateRequest("Concurrent Physical Library",
            Source("media", "Movies", MediaKind.Movies));

        var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(async _ =>
        {
            var (response, _) = await app.Client
                .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);
            return response;
        }));

        responses.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.Created);
        responses.Should().OnlyContain(response =>
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Given_ExactSourceIsManagedByAnotherLibrary_Should_ReturnBadRequest()
    {
        var existing = CreateRequest("Movies", Source("media", "Movies", MediaKind.Movies));
        var conflicting = CreateRequest("Archive Movies", Source("media", "Movies", MediaKind.Movies));
        var (createdResponse, _) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(existing);
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(conflicting);

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Theory]
    [InlineData("Movies", "Movies/Action")]
    [InlineData("Movies/Action", "Movies")]
    public async Task Given_OverlappingSourceIsManagedByAnotherLibrary_Should_ReturnBadRequest(
        string existingPath,
        string conflictingPath)
    {
        var existing = CreateRequest("Existing", Source("media", existingPath, MediaKind.Movies));
        var conflicting = CreateRequest("Conflicting", Source("media", conflictingPath, MediaKind.Movies));
        var (createdResponse, _) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(existing);
        createdResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(conflicting);

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Fact]
    public async Task Given_NoSources_Should_ReturnBadRequest()
    {
        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(new CreatePhysicalLibraryRequest("Physical Library", []));

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Fact]
    public async Task Given_NullSources_Should_ReturnBadRequest()
    {
        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(new CreatePhysicalLibraryRequest("Physical Library", null!));

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Fact]
    public async Task Given_NullSource_Should_ReturnBadRequest()
    {
        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(new CreatePhysicalLibraryRequest("Physical Library", [null!]));

        AssertBadRequestFor(response, problemDetails, "sources[0]");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Given_InvalidStorageLocationId_Should_ReturnBadRequest(string? locationId)
    {
        var request = CreateRequest("Physical Library",
            Source(locationId, "Movies", MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].storageLocationId");
    }

    [Fact]
    public async Task Given_UnknownStorageLocationId_Should_RejectSource()
    {
        var request = CreateRequest("Physical Library",
            Source("unknown", "Movies", MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertRejectedFor(response, problemDetails, "sources[0].storageLocationId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("/Movies")]
    [InlineData("../Movies")]
    [InlineData("Movies/../../outside")]
    public async Task Given_InvalidRelativePath_Should_ReturnBadRequest(string? relativePath)
    {
        var request = CreateRequest("Physical Library",
            Source("media", relativePath, MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].relativePath");
    }

    [Fact]
    public async Task Given_RelativePathExceedsMaximumLength_Should_ReturnBadRequest()
    {
        var request = CreateRequest("Physical Library",
            Source("media", new string('a', 1025), MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].relativePath");
    }

    [Fact]
    public async Task Given_TargetDirectoryDoesNotExist_Should_RejectSource()
    {
        var request = CreateRequest("Physical Library",
            Source("media", "Missing", MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertRejectedFor(response, problemDetails, "sources[0].relativePath");
    }

    [Fact]
    public async Task Given_StorageLocationIsUnavailable_Should_RejectSource()
    {
        var request = CreateRequest("Physical Library",
            Source("offline", "Movies", MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertRejectedFor(response, problemDetails, "sources[0].relativePath");
    }

    [Fact]
    public async Task Given_TargetIsAFile_Should_ReturnBadRequest()
    {
        var request = CreateRequest("Physical Library",
            Source("media", Path.GetFileName(app.FilePath), MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].relativePath");
    }

    [Fact(Skip = "Symbolic-link validation requires Linux semantics.", SkipUnless = nameof(IsLinux))]
    public async Task Given_TargetTraversesSymbolicLink_Should_ReturnBadRequest()
    {
        var request = CreateRequest("Physical Library",
            Source("media", Path.GetFileName(app.SymlinkPath), MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].relativePath");
    }

    [Theory]
    [InlineData("Movies", "Movies")]
    [InlineData("Movies", "./Movies")]
    [InlineData("Movies", "TV Shows/../Movies")]
    [InlineData("Movies/Action/..", "Movies")]
    public async Task Given_SourcesResolveToSameDirectory_Should_ReturnBadRequest(
        string firstPath,
        string secondPath)
    {
        var request = CreateRequest("Duplicate Sources",
            Source("media", firstPath, MediaKind.Movies),
            Source("media", secondPath, MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Theory]
    [InlineData("Movies", "Movies/Action")]
    [InlineData("Movies/Action", "Movies")]
    public async Task Given_OverlappingSourcesInSameLocation_Should_ReturnBadRequest(
        string firstPath,
        string secondPath)
    {
        var request = CreateRequest("Overlapping Sources",
            Source("media", firstPath, MediaKind.Movies),
            Source("media", secondPath, MediaKind.Movies));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources");
    }

    [Fact]
    public async Task Given_SiblingSourcesInSameLocation_Should_CreateLibrary()
    {
        var request = CreateRequest("Sibling Sources",
            Source("media", "Movies", MediaKind.Movies),
            Source("media", "Movies-Archive", MediaKind.Movies));

        var (response, _) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Given_SameRelativePathInDifferentLocations_Should_CreateLibrary()
    {
        var request = CreateRequest("Distributed Sources",
            Source("media", "Movies", MediaKind.Movies),
            Source("archive", "Movies", MediaKind.Movies));

        var (response, _) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, CreatePhysicalLibraryResponse>(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Given_SourceWithUnknownMediaKind_Should_ReturnBadRequest()
    {
        var request = CreateRequest("Physical Library",
            Source("media", "Movies", (MediaKind)999));

        var (response, problemDetails) = await app.Client
            .POSTAsync<CreatePhysicalLibraryEndpoint, CreatePhysicalLibraryRequest, ProblemDetails>(request);

        AssertBadRequestFor(response, problemDetails, "sources[0].kind");
    }

    public static bool IsLinux => OperatingSystem.IsLinux();

    private static CreatePhysicalLibraryRequest CreateRequest(string name, params CreatePhysicalLibrarySource[] sources)
    {
        return new CreatePhysicalLibraryRequest(name, sources);
    }

    private static CreatePhysicalLibrarySource Source(
        string? locationId,
        string? relativePath,
        MediaKind kind)
    {
        return new CreatePhysicalLibrarySource(locationId!, relativePath!, kind);
    }

    private static void AssertBadRequestFor(HttpResponseMessage response, ProblemDetails problemDetails, string propertyName)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Errors.Should().Contain(error => error.Name == propertyName);
    }

    private static void AssertRejectedFor(HttpResponseMessage response, ProblemDetails problemDetails, string propertyName)
    {
        response.IsSuccessStatusCode.Should().BeFalse();
        problemDetails.Status.Should().Be((int)response.StatusCode);
        problemDetails.Errors.Should().Contain(error => error.Name == propertyName);
    }

    private async Task<IReadOnlyList<PersistedSource>> GetPersistedSourcesAsync(Guid libraryId)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.OpenConnectionAsync(TestContext.Current.CancellationToken);

        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText =
            """
            SELECT "StorageLocationId", "RelativePath", "MediaKind"
            FROM "LibrarySource"
            WHERE "library_id" = @libraryId
            ORDER BY "StorageLocationId", "RelativePath"
            """;
        var parameter = command.CreateParameter();
        parameter.ParameterName = "libraryId";
        parameter.Value = libraryId;
        command.Parameters.Add(parameter);

        List<PersistedSource> sources = [];
        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            sources.Add(new PersistedSource(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt32(2)));
        }

        return sources;
    }

    private sealed record PersistedSource(
        string StorageLocationId,
        string RelativePath,
        int MediaKind);
}
