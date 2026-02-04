# Admin E2E Tests with Aspire Integration

This project contains end-to-end (E2E) tests for the Admin application that are integrated with .NET Aspire orchestration.

## Overview

These E2E tests use an optimized Aspire configuration that only starts the services needed for testing:
- ✅ **PostgreSQL** - Database for API
- ✅ **API** - Backend service

The **Admin Vite app** is started manually by the test framework (not via Aspire) for better performance and reliability.

Services NOT started (faster startup):
- ❌ Admin app via Aspire (started manually with Process for speed)
- ❌ Redis (not required for Admin app)
- ❌ Azure Service Bus (not required for Admin app)
- ❌ Designer app (not being tested)
- ❌ Azure Functions (not required for Admin app)

### Key Features

- ✅ Run as part of the .NET test suite (`dotnet test`)
- ✅ **Single Aspire instance shared across all tests** (much faster)
- ✅ **Vite dev server started once and shared across all tests**
- ✅ Automatically start only required services
- ✅ Automatically discover API URL from Aspire and pass to Vite
- ✅ Tests run in collection to ensure proper sequencing
- ✅ Fast initialization (~30-40 seconds for complete stack)

## Running the Tests

### Prerequisites

- .NET 10.0 SDK
- .NET Aspire 13 workload: `dotnet workload install aspire`
- Docker Desktop (or Podman) - for PostgreSQL container
- Node.js 18+ and npm - for Vite dev server
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
1. Start required Aspire services (PostgreSQL, API) - **much faster than full stack**
2. **Reuse the same Aspire instance for all tests** (even faster)
3. Start Vite dev server for Admin app (once, shared across all tests)
4. Automatically discover the API URL and pass it to Vite
5. Run Playwright E2E tests against the running application
6. Clean up all resources when complete

## How It Works

### Performance Optimizations

The Admin E2E tests are optimized for fast execution:

**Minimal Services**
- Only starts via Aspire: PostgreSQL and API
- Admin app started manually via Process (faster than Aspire orchestration)
- Skips: Redis, Azure Service Bus, Designer app, Azure Functions
- Result: ~60% fewer services = faster startup

**Parallel Startup**
- Removed `.WaitFor()` chains that forced sequential startup
- Removed health checks that blocked until services were fully ready
- Aspire starts services in parallel automatically
- Tests wait intelligently for services with retry logic

**Shared Aspire Instance**
- Single Aspire instance shared across all tests via xUnit collection
- First test pays startup cost (~10-30 seconds)
- Subsequent tests run immediately (~1-5 seconds each)

**Expected Timings**
- First test (cold start): 10-30 seconds
- Subsequent tests: 1-5 seconds each
- Total for 2 tests: ~15-35 seconds

If tests take longer than 60 seconds, see troubleshooting section below.

## Architecture

1. **TestAppHost**: Custom Aspire configuration that only starts required services (PostgreSQL, API, Admin)
2. **AspireAppHostFixture**: Starts the optimized Aspire stack using TestAppHost
3. **AdminE2ECollection**: xUnit collection that ensures all tests share the same Aspire instance (faster)
4. **PlaywrightTestBase**: Initializes Playwright and provides helper methods to get service URLs from Aspire
5. **Test Classes**: Inherit from `PlaywrightTestBase` and use the automatically discovered URLs

### Example Test

```csharp
[Collection("AdminE2E")]  // All tests in this collection share one Aspire instance
public class TagsE2ETests : PlaywrightTestBase
{
    public TagsE2ETests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task ShouldCompleteFullCrudFlowForTag()
    {
        // Get the Admin app URL from Aspire (waits up to 60s for service to be ready)
        var baseUrl = await GetAdminAppUrlAsync();
        
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
2. Add `[Collection("AdminE2E")]` attribute to use the shared Aspire instance
3. **Important**: Do NOT add `IClassFixture<AspireAppHostFixture>` - it conflicts with the collection fixture
4. Use `GetAdminAppUrl()` to get the base URL
5. Write Playwright tests as usual

Example:

```csharp
[Collection("AdminE2E")]  // ✅ Use collection fixture
public class MyFeatureE2ETests : PlaywrightTestBase  // ❌ Do NOT add IClassFixture
{
    public MyFeatureE2ETests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task MyTest()
    {
        var baseUrl = await GetAdminAppUrlAsync();
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

### Tests hang or timeout for 1.5+ minutes
**Error**: Test appears to hang during initialization and never completes.

**Recent Fix**: Tests now include intelligent retry logic that waits up to 60 seconds for services to become available. If tests still hang beyond this:

**Cause**: Could be conflicting xUnit fixture patterns, Docker issues, or service startup problems.

**Solutions**:

1. **Check for conflicting fixtures**: If a test class has both `[Collection("AdminE2E")]` and `IClassFixture<AspireAppHostFixture>`, xUnit tries to initialize the fixture in two incompatible ways, causing deadlocks.

   ```csharp
   // ❌ WRONG - Causes hanging
   [Collection("AdminE2E")]
   public class MyTests : PlaywrightTestBase, IClassFixture<AspireAppHostFixture>

   // ✅ CORRECT - Works properly  
   [Collection("AdminE2E")]
   public class MyTests : PlaywrightTestBase
   {
       public MyTests(AspireAppHostFixture fixture) : base(fixture) { }
   }
   ```

   The collection definition (`AdminE2ECollection.cs`) already declares the fixture, so individual test classes should NOT use `IClassFixture`.

2. **Check Docker**: Ensure Docker is running and has adequate resources (2-3GB RAM)

3. **Check service logs**: Look at Aspire Dashboard or test output for specific error messages

4. **Try manual startup**: Start Aspire manually to see detailed startup logs:
   ```bash
   cd src/AppHost && dotnet run
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
