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
                normalizedRoot = location.NormalizedRootPath;
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

            foreach (var (existingRoot, existingId) in roots)
            {
                if (normalizedRoot == existingRoot)
                    errors.Add(
                        $"Storage locations '{existingId}' and '{id}' use the same root path '{normalizedRoot}'.");
                else if (IsNestedWithin(normalizedRoot, existingRoot)
                         || IsNestedWithin(existingRoot, normalizedRoot))
                    errors.Add(
                        $"Storage locations '{existingId}' and '{id}' have overlapping root paths '{existingRoot}' and '{normalizedRoot}'.");
            }

            roots.TryAdd(normalizedRoot, id);
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }

    private static bool IsNestedWithin(string path, string root)
    {
        var prefix = Path.EndsInDirectorySeparator(root)
            ? root
            : root + Path.DirectorySeparatorChar;

        return path.StartsWith(prefix, StringComparison.Ordinal);
    }
}