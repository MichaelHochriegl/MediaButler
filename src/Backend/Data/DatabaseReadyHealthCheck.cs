using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Data;

/// <summary>
/// Represents a health check to determine if the application's database is ready for use.
/// </summary>
/// <seealso cref="IHealthCheck"/>
internal sealed class DatabaseReadyHealthCheck(StartupState startupState) : IHealthCheck
{
    /// <summary>
    /// Checks the health status of the database readiness during application startup.
    /// </summary>
    /// <param name="context">
    /// The context that provides options and execution information for the health check.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to signal the health check operation should be canceled.
    /// </param>
    /// <returns>
    /// A task that represents the result of the health check operation, indicating whether the database
    /// is ready (<see cref="HealthCheckResult.Healthy"/>) or not (<see cref="HealthCheckResult.Unhealthy"/>).
    /// </returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(startupState.DatabaseReady
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy());
    }
}