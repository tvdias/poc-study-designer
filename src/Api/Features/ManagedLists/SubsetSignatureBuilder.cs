using System.Security.Cryptography;
using System.Text;

namespace Api.Features.ManagedLists;

public static class SubsetSignatureBuilder
{
    /// <summary>
    /// Builds a deterministic signature hash from a collection of ManagedListItem IDs.
    /// The IDs are sorted and deduplicated before hashing to ensure consistency.
    /// </summary>
    /// <param name="managedListItemIds">Collection of ManagedListItem IDs</param>
    /// <returns>SHA-256 hash of the sorted, unique IDs</returns>
    public static string BuildSignature(IEnumerable<Guid> managedListItemIds)
    {
        // Sort and deduplicate the IDs
        var sortedUniqueIds = managedListItemIds
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        if (sortedUniqueIds.Count == 0)
        {
            throw new ArgumentException("Cannot build signature from empty collection", nameof(managedListItemIds));
        }

        // Concatenate IDs into a single string
        var concatenated = string.Join("|", sortedUniqueIds.Select(id => id.ToString("N")));

        // Compute SHA-256 hash
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(concatenated));
        
        // Convert to hex string
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
