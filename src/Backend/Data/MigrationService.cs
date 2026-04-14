using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

/// <summary>
/// Handles database migrations as a hosted service during application startup.
/// </summary>
/// <remarks>
/// This service ensures that the database schema is up to date by applying any pending migrations
/// before the application starts handling requests. It also updates the startup state to indicate
/// whether the database is ready.
/// </remarks>
internal sealed class MigrationService(
    ILogger<MigrationService> logger,
    IServiceScopeFactory scopeFactory,
    StartupState startupState) : IHostedService
{
    /// <summary>
    /// Represents the name of the activity source used for tracing and diagnostics
    /// within the migration service layer of the application.
    /// </summary>
    public const string ActivitySourceName = "Backend.Migration";
    
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    /// <summary>
    /// Starts the migration service, performing necessary database migrations
    /// and updating the application startup state.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> used to observe cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous operation of starting the migration service.
    /// </returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Database migration");
        
        try
        {
            logger.LogInformation("Starting database migration...");
            
            using var scope = scopeFactory.CreateScope();
            
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync(cancellationToken);
            
            startupState.DatabaseReady = true;
            
            logger.LogInformation("Database migration completed");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to start migration service");
            activity?.AddException(e);
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            startupState.DatabaseReady = false;
            throw;
        }
    }

    /// <summary>
    /// Stops the migration service asynchronously.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that is used to propagate notification that the operation should be canceled.
    /// </param>
    /// <return>
    /// A <see cref="Task"/> that represents the asynchronous stop operation.
    /// </return>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}