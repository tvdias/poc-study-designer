# Testing Guide

This document describes the testing strategy and implementation for the POC Study Designer project.

## Overview

The project includes two main types of tests:
- **Unit Tests** (`src/Api.Tests/`) - Test individual components and business logic in isolation
- **Integration Tests** (`src/Api.IntegrationTests/`) - Test the API endpoints with the full application stack

## Integration Tests

### Purpose

Integration tests verify that the API endpoints work correctly with the entire application stack, including:
- ASP.NET Core web host
- Database (PostgreSQL via .NET Aspire)
- Entity Framework Core
- Validation logic
- HTTP routing and serialization

### Technology Stack

The integration tests use:
- **xUnit v3** - Testing framework
- **.NET Aspire Testing** (`Aspire.Hosting.Testing`) - Provides `DistributedApplication` for testing
- **Microsoft.Testing.Platform** - Modern .NET testing platform

### Project Structure

```
src/Api.IntegrationTests/
├── Api.IntegrationTests.csproj
├── TagTests.cs                    # Tests for Tag endpoints
├── CommissioningMarketTests.cs    # Tests for CommissioningMarket endpoints
└── FieldworkMarketTests.cs        # Tests for FieldworkMarket endpoints
```

### Running Integration Tests

Integration tests can be run using several methods:

#### Using dotnet test (Recommended)

```bash
# Run all integration tests
dotnet test src/Api.IntegrationTests/Api.IntegrationTests.csproj

# Run tests with verbose output
dotnet test src/Api.IntegrationTests/Api.IntegrationTests.csproj -v normal

# Run specific test
dotnet test src/Api.IntegrationTests/Api.IntegrationTests.csproj --filter "FullyQualifiedName~TagTests"
```

#### Using dotnet run

The integration tests project is configured with `OutputType=Exe` and `EnableMSTestingRunner=true`, allowing it to be run directly:

```bash
cd src/Api.IntegrationTests
dotnet run
```

### Test Architecture

#### Shared Fixture

All integration tests use a shared `BoxedAppHostFixture` that:
- Starts the entire application via .NET Aspire's `DistributedApplication`
- Manages the lifecycle of all services (API, Database, Redis, etc.)
- Is shared across all test classes using xUnit's `IClassFixture<T>` pattern
- Properly disposes resources after tests complete

Example:

```csharp
public class BoxedAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHostAssembly::Program>();
        App = await appHost.BuildAsync();
        await App.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}
```

#### Test Pattern

All integration tests follow a consistent pattern:

1. **Arrange**: Get HTTP client from the Aspire app
2. **Act**: Make HTTP requests to API endpoints
3. **Assert**: Verify response status codes and data

Example:

```csharp
[Fact]
public async Task CreateAndGetTags_WorksCorrectly()
{
    // Arrange
    var appName = "api";
    var client = fixture.App.CreateHttpClient(appName);

    // Act - Create
    var newTag = new CreateTagRequest("Integration Test Tag");
    var createResponse = await client.PostAsJsonAsync("/api/tags", newTag, 
        cancellationToken: TestContext.Current.CancellationToken);

    // Assert - Create
    createResponse.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
    var createdTag = await createResponse.Content.ReadFromJsonAsync<CreateTagResponse>(
        cancellationToken: TestContext.Current.CancellationToken);
    Assert.NotNull(createdTag);
    Assert.Equal(newTag.Name, createdTag.Name);
}
```

### Current Test Coverage

The integration tests currently cover the following API features:

#### TagTests
- ✅ Create and Get Tags
- ✅ Get Tag by ID

#### CommissioningMarketTests
- ✅ Create and Get Commissioning Markets
- ✅ Get Commissioning Market by ID
- ✅ Update Commissioning Market
- ✅ Delete Commissioning Market
- ✅ Search Commissioning Markets

#### FieldworkMarketTests
- ✅ Create and Get Fieldwork Markets
- ✅ Get Fieldwork Market by ID
- ✅ Update Fieldwork Market
- ✅ Delete Fieldwork Market
- ✅ Search Fieldwork Markets

### Coverage Gaps

The following API features are **not yet covered** by integration tests:

- ❌ Clients (`/api/clients`)
- ❌ Configuration Questions (`/api/configuration-questions`)
- ❌ Metric Groups (`/api/metric-groups`)
- ❌ Modules (`/api/modules`)
- ❌ Module Questions (`/api/modules/{id}/questions`)
- ❌ Product Templates (`/api/product-templates`)
- ❌ Product Template Lines (`/api/product-templates/{id}/lines`)
- ❌ Products (`/api/products`)
- ❌ Product Config Questions (`/api/products/{id}/config-questions`)
- ❌ Product Config Question Display Rules (`/api/products/{productId}/config-questions/{questionId}/display-rules`)
- ❌ Question Bank (`/api/question-bank`)
- ❌ Question Bank Answers (`/api/question-bank/{id}/answers`)

### Recommendations

1. **Expand Coverage**: Add integration tests for all remaining API endpoints to ensure comprehensive coverage

2. **Error Scenarios**: Add tests for error cases:
   - Invalid input validation
   - Conflict scenarios (duplicate names, constraint violations)
   - Not found scenarios
   - Authorization/authentication (when implemented)

3. **Complex Workflows**: Add tests for multi-step workflows:
   - Creating Products with Config Questions and Display Rules
   - Creating Product Templates with Template Lines
   - Module and Question Bank relationships

4. **Performance Tests**: Consider adding performance tests for:
   - Bulk operations
   - Search queries with large datasets
   - Complex joins and relationships

5. **Data Isolation**: Consider adding test data cleanup or using separate test databases for each test run to ensure test isolation

## Unit Tests

### Location
`src/Api.Tests/`

### Purpose
Unit tests focus on testing individual components in isolation, such as:
- Validators (FluentValidation)
- Business logic
- Utility functions

### Running Unit Tests

```bash
# Run all unit tests
dotnet test src/Api.Tests/Api.Tests.csproj

# Run with verbose output
dotnet test src/Api.Tests/Api.Tests.csproj -v normal
```

## Best Practices

### Writing Integration Tests

1. **Use Meaningful Test Names**: Use descriptive test names that explain the scenario and expected behavior
   ```csharp
   [Fact]
   public async Task CreateTag_WithValidData_ReturnsCreated()
   ```

2. **Follow AAA Pattern**: Structure tests with Arrange, Act, Assert sections

3. **Use TestContext.Current.CancellationToken**: Pass the cancellation token to all async operations for proper test timeout handling

4. **Assert HTTP Status Codes**: Always verify the response status code matches expectations

5. **Test Both Success and Failure Cases**: Don't just test happy paths

6. **Clean Up Test Data**: Consider cleanup strategies if tests modify shared state

### Writing Unit Tests

1. **Test One Thing**: Each test should verify one specific behavior

2. **Use Test Data Builders**: For complex object creation, consider using test data builders

3. **Mock External Dependencies**: Use mocking frameworks for external dependencies

4. **Avoid Test Interdependencies**: Tests should not depend on each other

## Continuous Integration

Integration and unit tests should be run as part of the CI/CD pipeline to ensure:
- All changes are properly tested
- Breaking changes are caught early
- Code quality is maintained

Example GitHub Actions workflow:

```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Unit Tests
        run: dotnet test src/Api.Tests/Api.Tests.csproj
      
      - name: Run Integration Tests
        run: dotnet test src/Api.IntegrationTests/Api.IntegrationTests.csproj
```

## Troubleshooting

### Common Issues

1. **Tests Timeout**: Integration tests start the entire application stack and may take time. Increase test timeout if needed.

2. **Port Conflicts**: Ensure no other services are running on the ports used by Aspire.

3. **Database Connection Issues**: Verify that Docker is running and PostgreSQL container is accessible.

4. **Build Warnings**: The integration tests may show Entity Framework version conflict warnings. These are typically harmless but should be monitored.

### Debugging Tests

1. **Run Single Test**: Use `--filter` to run a specific test
   ```bash
   dotnet test --filter "FullyQualifiedName=Api.IntegrationTests.TagTests.CreateAndGetTags_WorksCorrectly"
   ```

2. **Enable Verbose Logging**: Use `-v detailed` for more information
   ```bash
   dotnet test -v detailed
   ```

3. **Attach Debugger**: Set breakpoints in your IDE and run tests in debug mode

## Future Enhancements

1. **E2E Tests**: Consider adding end-to-end tests that test the full stack including React frontends

2. **Contract Testing**: Add contract tests to verify API contracts remain stable

3. **Load Testing**: Add load tests to verify performance under stress

4. **Mutation Testing**: Consider mutation testing to verify test quality

5. **Code Coverage Reports**: Integrate code coverage reporting (e.g., Coverlet)
