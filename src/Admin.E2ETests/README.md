# Admin E2E Tests with Aspire Integration

This project contains end-to-end (E2E) tests for the Admin application that are integrated with .NET Aspire orchestration.

## Overview

These E2E tests use an optimized Aspire configuration that only starts the services needed for testing:
- ✅ **PostgreSQL** - Database for API
- ✅ **API** - Backend service
- ✅ **Admin app** - Frontend application being tested

Services NOT started (faster startup):
- ❌ Redis (not required for Admin app)
- ❌ Azure Service Bus (not required for Admin app)
- ❌ Designer app (not being tested)
- ❌ Azure Functions (not required for Admin app)

### Key Features

- ✅ Run as part of the .NET test suite
- ✅ **Single Aspire instance shared across all tests** (much faster)
- ✅ Automatically start only required services
- ✅ Automatically discover service URLs from Aspire (no manual configuration)
- ✅ Tests run in collection to ensure proper sequencing

## Running the Tests

### Prerequisites

- .NET 10.0 SDK
- .NET Aspire 13 workload: `dotnet workload install aspire`
- Docker Desktop (or Podman)
- Playwright browsers: Install once with:
  ```bash
  cd src/Admin.E2ETests
  dotnet build
  pwsh bin/Debug/net10.0/playwright.ps1 install chromium
  ```

### Run Tests

```bash
# From the solution root
dotnet test src/Admin.E2ETests/Admin.E2ETests.csproj

# Or from the test project directory
cd src/Admin.E2ETests
dotnet test

# List available tests
dotnet test --list-tests
```

That's it! The tests will:
1. Start only the required services (PostgreSQL, API, Admin app) - **much faster than full stack**
2. **Reuse the same Aspire instance for all tests** (even faster)
3. Wait for all services to be healthy
4. Automatically discover the Admin app URL
5. Run Playwright E2E tests against the running application
6. Clean up all resources when complete

## How It Works

### Architecture

1. **TestAppHost**: Custom Aspire configuration that only starts required services (PostgreSQL, API, Admin)
2. **AspireAppHostFixture**: Starts the optimized Aspire stack using TestAppHost
3. **AdminE2ECollection**: xUnit collection that ensures all tests share the same Aspire instance (faster)
4. **PlaywrightTestBase**: Initializes Playwright and provides helper methods to get service URLs from Aspire
5. **Test Classes**: Inherit from `PlaywrightTestBase` and use the automatically discovered URLs

### Example Test

```csharp
[Collection("AdminE2E")]  // All tests in this collection share one Aspire instance
public class TagsE2ETests : PlaywrightTestBase, IClassFixture<AspireAppHostFixture>
{
    public TagsE2ETests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task ShouldCompleteFullCrudFlowForTag()
    {
        // Get the Admin app URL from Aspire (automatically assigned)
        var baseUrl = GetAdminAppUrl();
        
        // Navigate and test
        await Page.GotoAsync($"{baseUrl}/tags");
        // ... test code ...
    }
}
```

## Performance Optimizations

This test project is optimized for fast execution:

1. **Minimal Services**: Only starts PostgreSQL, API, and Admin app (skips Redis, Service Bus, Functions, Designer)
2. **Shared Aspire Instance**: All tests in the `AdminE2E` collection share one Aspire instance
3. **Simplified Dependencies**: API only depends on PostgreSQL; Admin app has no dependencies
4. **Parallel Test Execution**: Tests can run in parallel within the shared Aspire instance

## Advantages Over Standalone Tests

| Feature | Standalone (`src/Admin/e2e/`) | Aspire Integrated (this project) |
|---------|-------------------------------|----------------------------------|
| Manual setup | ❌ Must start Aspire separately | ✅ Automatic |
| URL configuration | ❌ Manual (ports change) | ✅ Automatic discovery |
| Dependency management | ❌ Manual | ✅ Aspire handles it |
| Test isolation | ⚠️ Shared state | ✅ Fresh stack per run |
| CI/CD integration | ⚠️ Complex | ✅ Simple `dotnet test` |
| Run command | `npm run test:e2e` | `dotnet test` |

## Test Structure

```
src/Admin.E2ETests/
├── Admin.E2ETests.csproj          # Project file with Aspire + Playwright
├── AspireAppHostFixture.cs        # Starts entire Aspire stack
├── PlaywrightTestBase.cs          # Base class with Playwright setup
├── TestHelpers.cs                 # Common test operations
├── TagsE2ETests.cs                # Tags feature tests
├── CommissioningMarketsE2ETests.cs # (to be added)
└── FieldworkMarketsE2ETests.cs    # (to be added)
```

## Adding New Tests

1. Create a new test class inheriting from `PlaywrightTestBase`
2. Add `IClassFixture<AspireAppHostFixture>` to share the Aspire instance
3. Use `GetAdminAppUrl()` to get the base URL
4. Write Playwright tests as usual

Example:

```csharp
[Collection("E2E")]
public class MyFeatureE2ETests : PlaywrightTestBase, IClassFixture<AspireAppHostFixture>
{
    public MyFeatureE2ETests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task MyTest()
    {
        var baseUrl = GetAdminAppUrl();
        await Page.GotoAsync($"{baseUrl}/my-feature");
        // ... test code ...
    }
}
```

## Troubleshooting

### Playwright browsers not installed
The tests require Playwright browsers to be installed. Install them once after building:
```bash
cd src/Admin.E2ETests
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

### Tests timeout waiting for Aspire to start
**Error**: `Polly.Timeout.TimeoutRejectedException: The operation didn't complete within the allowed timeout of '00:00:20'`

**Note**: This should be much less common now with the optimized configuration that only starts 3 services instead of 8+.

**Cause**: Aspire is taking longer than 20 seconds to start services (PostgreSQL, API, Admin app).

**Solutions**:
1. **Ensure Docker is running** - Aspire uses containers for databases
   ```bash
   docker ps  # Should show containers running
   ```

2. **Ensure sufficient system resources**:
   - At least 2GB RAM available (reduced from 4GB due to fewer services)
   - Docker has adequate CPU/memory limits configured

3. **First run is slower** - Container images need to be downloaded
   - Subsequent runs are much faster (images cached)

4. **Pre-start services** - Start Aspire manually first (rare need now):
   ```bash
   # Terminal 1: Start Aspire
   cd src/AppHost
   dotnet run

   # Wait for all services to be healthy
   # Terminal 2: Run tests
   cd src/Admin.E2ETests
   dotnet test
   ```

### Tests run slowly
- Tests share a single Aspire instance via the `AdminE2E` collection
- If tests seem slow, ensure all test classes use `[Collection("AdminE2E")]`
- Each test class that uses a different collection name will start its own Aspire instance

### Can't find Admin app URL
The test uses `CreateHttpClient("app-admin")` to get the URL. Verify the TestAppHost has `app-admin` resource.

## CI/CD Integration

Since xUnit v3 requires running the test executable directly:

```yaml
- name: Run E2E Tests
  run: dotnet test src/Admin.E2ETests/Admin.E2ETests.csproj
  env:
    ASPIRE_ALLOW_UNSECURED_TRANSPORT: true
```

**Note**: Playwright browsers will be automatically installed when the test project is built for the first time.

## Comparison with API Integration Tests

This project follows the same pattern as `src/Api.IntegrationTests/`:
- Uses `Aspire.Hosting.Testing`
- Uses `BoxedAppHostFixture` / `AspireAppHostFixture` pattern
- Starts the entire application stack
- Discovers service URLs automatically
- Works with `dotnet test` command

The difference is that API integration tests use HttpClient to test the API directly, while these E2E tests use Playwright to test the full UI→API→Database flow.
