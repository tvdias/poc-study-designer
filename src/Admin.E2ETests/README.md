# Admin E2E Tests with Aspire Integration

This project contains end-to-end (E2E) tests for the Admin application that are integrated with .NET Aspire orchestration.

## Overview

Unlike the standalone Playwright tests in `src/Admin/e2e/`, these E2E tests:
- ✅ Run as part of the .NET test suite
- ✅ Automatically start the entire Aspire application stack (API, databases, Redis, Admin frontend)
- ✅ Automatically discover service URLs from Aspire (no manual configuration needed)
- ✅ Ensure all dependencies are running with the correct configuration

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
1. Start the entire Aspire application stack
2. Wait for all services to be healthy
3. Automatically discover the Admin app URL
4. Run Playwright E2E tests against the running application
5. Clean up all resources when complete

## How It Works

### Architecture

1. **AspireAppHostFixture**: Starts the entire Aspire application using `DistributedApplicationTestingBuilder`
2. **PlaywrightTestBase**: Initializes Playwright and provides helper methods to get service URLs from Aspire
3. **Test Classes**: Inherit from `PlaywrightTestBase` and use the automatically discovered URLs

### Example Test

```csharp
[Collection("E2E")]
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

**Cause**: Aspire is taking longer than 20 seconds to start all services (PostgreSQL, Redis, Azure Service Bus emulator, API, Admin app).

**Solutions**:
1. **Ensure Docker is running** - Aspire uses containers for databases and services
   ```bash
   docker ps  # Should show containers running
   ```

2. **Ensure sufficient system resources**:
   - At least 4GB RAM available
   - Docker has adequate CPU/memory limits configured
   - No other heavy processes running

3. **Run tests with more time** - On slow systems, services may need more time to initialize:
   - First run is slower (downloading images)
   - Subsequent runs are faster (images cached)

4. **Pre-start services** - Start Aspire manually first, then run tests:
   ```bash
   # Terminal 1: Start Aspire
   cd src/AppHost
   dotnet run

   # Wait for all services to be healthy (check Aspire dashboard)
   # Terminal 2: Run tests (they'll connect to existing Aspire instance)
   cd src/Admin.E2ETests
   dotnet test
   ```

### Can't find Admin app URL
The test uses `CreateHttpClient("app-admin")` to get the URL. Verify the AppHost has `app-admin` resource.

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
