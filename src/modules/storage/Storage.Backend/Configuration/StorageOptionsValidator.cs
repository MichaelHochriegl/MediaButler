using Domain.Storage;
using Microsoft.Extensions.Options;

namespace Storage.Backend.Configuration;

internal sealed class StorageOptionsValidator
    : IValidateOptions<StorageOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        StorageOptions options)
    {
        List<string> errors = [];

        var roots = new Dictionary<string, string>(
            StringComparer.Ordinal);

        foreach (var (id, location) in options.Locations)
        {
            try
            {
                _ = new StorageLocationId(id);
            }
            catch (ArgumentException exception)
            {
                errors.Add(
                    $"Storage location ID '{id}' is invalid: {exception.Message}");

                continue;
            }

            if (string.IsNullOrWhiteSpace(location.DisplayName))
                errors.Add(
                    $"Storage location '{id}' must have a display name.");

            if (string.IsNullOrWhiteSpace(location.RootPath))
            {
                errors.Add(
                    $"Storage location '{id}' must have a root path.");

                continue;
            }

            if (!Path.IsPathFullyQualified(location.RootPath))
            {
                errors.Add(
                    $"Root path '{location.RootPath}' for storage location '{id}' must be absolute.");

                continue;
            }

            string normalizedRoot;

            try
            {
                normalizedRoot =
                    Path.TrimEndingDirectorySeparator(
                        Path.GetFullPath(location.RootPath));
            }
            catch (Exception exception)
                when (exception is ArgumentException
                          or NotSupportedException
                          or PathTooLongException)
            {
                errors.Add(
                    $"Root path '{location.RootPath}' for storage location '{id}' is invalid.");

                continue;
            }

            if (!roots.TryAdd(normalizedRoot, id))
                errors.Add(
                    $"Storage locations '{roots[normalizedRoot]}' and '{id}' use the same root path '{normalizedRoot}'.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}