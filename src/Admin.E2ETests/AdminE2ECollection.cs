using Xunit;

namespace Admin.E2ETests;

/// <summary>
/// xUnit collection definition that ensures all E2E tests share the same Aspire instance.
/// This makes tests much faster as Aspire (and all its services) only starts once.
/// </summary>
[CollectionDefinition("AdminE2E")]
public class AdminE2ECollection : ICollectionFixture<AspireAppHostFixture>
{
    // This class is just a marker for xUnit.
    // xUnit will create a single instance of AspireAppHostFixture and share it
    // across all test classes that use [Collection("AdminE2E")]
}
