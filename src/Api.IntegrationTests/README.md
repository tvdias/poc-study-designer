# API Integration Tests

This project contains integration tests for the POC Study Designer API. These tests verify that API endpoints work correctly with the full application stack.

## Quick Start

Run all integration tests:
```bash
dotnet test
```

Run from the project directory:
```bash
cd src/Api.IntegrationTests
dotnet run
```

## What's Tested

### Current Coverage

- **Tags API** - Create, Get, Get by ID
- **Commissioning Markets API** - Full CRUD operations and search
- **Fieldwork Markets API** - Full CRUD operations and search

### Not Yet Covered

The following API endpoints need integration tests:
- Clients
- Configuration Questions
- Metric Groups
- Modules and Module Questions
- Product Templates and Template Lines
- Products and Product Config Questions
- Question Bank and Answers

## Test Architecture

### Aspire Testing

Tests use `.NET Aspire Testing` which:
- Starts the entire application stack (API, Database, Redis, etc.)
- Manages service lifecycle automatically
- Provides HTTP clients for testing

### Shared Fixture

All tests share a `BoxedAppHostFixture` that:
- Initializes once for all test classes
- Starts the Aspire `DistributedApplication`
- Disposes properly after tests complete

### Test Pattern

```csharp
[Fact]
public async Task TestName_WorksCorrectly()
{
    // Arrange: Get HTTP client
    var client = fixture.App.CreateHttpClient("api");

    // Act: Call API endpoint
    var response = await client.PostAsJsonAsync("/api/endpoint", request, 
        cancellationToken: TestContext.Current.CancellationToken);

    // Assert: Verify results
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<ResponseType>(
        cancellationToken: TestContext.Current.CancellationToken);
    Assert.NotNull(result);
}
```

## Adding New Tests

To add integration tests for a new API feature:

1. Create a new test class in this project (e.g., `ClientTests.cs`)
2. Inherit from `IClassFixture<BoxedAppHostFixture>`
3. Follow the existing test pattern (see `TagTests.cs` for a simple example)
4. Test all CRUD operations if applicable
5. Add tests for search/query operations
6. Consider error scenarios (validation failures, conflicts, not found)

Example:

```csharp
extern alias AppHostAssembly;
using Api.Features.YourFeature;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

public class YourFeatureTests(BoxedAppHostFixture fixture) : IClassFixture<BoxedAppHostFixture>
{
    [Fact]
    public async Task CreateYourEntity_WorksCorrectly()
    {
        // Arrange
        var appName = "api";
        var client = fixture.App.CreateHttpClient(appName);

        // Act
        var newEntity = new CreateYourEntityRequest("Test Data");
        var createResponse = await client.PostAsJsonAsync("/api/your-endpoint", newEntity, 
            cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        createResponse.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateYourEntityResponse>(
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
    }
}
```

## Best Practices

1. **Use TestContext.Current.CancellationToken**: Always pass the cancellation token to async operations
2. **Test Happy and Error Paths**: Test both successful operations and error scenarios
3. **Verify Status Codes**: Always assert the expected HTTP status code
4. **Use Descriptive Names**: Test names should clearly describe what they test
5. **Keep Tests Independent**: Tests should not depend on execution order
6. **Clean Test Data**: Consider cleanup if tests create data that could affect other tests

## Troubleshooting

### Tests Fail to Start

Ensure:
- Docker is running (required by Aspire for PostgreSQL, Redis, etc.)
- No port conflicts with other running services
- .NET 10.0 SDK is installed
- Aspire workload is installed: `dotnet workload install aspire`

### Slow Test Execution

- Integration tests start the full application stack, so they take longer than unit tests
- The `BoxedAppHostFixture` is shared across test classes to minimize startup time
- Consider running specific test classes when developing: `dotnet test --filter "FullyQualifiedName~TagTests"`

### Debugging Tests

- Set breakpoints in your IDE
- Run tests in debug mode
- Use `dotnet test -v detailed` for verbose output
- Check Aspire dashboard logs if services fail to start

## More Information

For detailed testing documentation, see **[TESTING.md](../../TESTING.md)** in the repository root.
