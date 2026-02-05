# Admin E2E Tests with Aspire Integration

This project contains end-to-end (E2E) tests for the Admin application that are integrated with .NET Aspire orchestration.

## Overview

These E2E tests use the **main AppHost** with environment-based service filtering. When `ASPIRE_TEST_MODE=AdminE2E` is set, only the services needed for testing are started:

- ✅ **PostgreSQL** - Database for API
- ✅ **API** - Backend service  
- ✅ **Admin Vite app** - Frontend application (via Aspire's ViteApp)

Services NOT started in AdminE2E mode (faster startup):
- ❌ Redis (not required for Admin app)
- ❌ Azure Service Bus (not required for Admin app)
- ❌ Designer app (not being tested)
- ❌ Azure Functions (not required for Admin app)

### Key Features

- ✅ Uses **main AppHost** (single source of truth, same as Api.IntegrationTests)
- ✅ Environment-based service filtering (no separate test AppHost needed)
- ✅ Run as part of the .NET test suite (`dotnet test`)
- ✅ **Single Aspire instance shared across all tests** (much faster)
- ✅ Tests run in collection to ensure proper sequencing
- ✅ Fast initialization (~30-50 seconds for complete stack)

## Running the Tests

### Prerequisites

- .NET 10.0 SDK
- .NET Aspire 13 workload: `dotnet workload install aspire`
- Docker Desktop (or Podman) - for PostgreSQL container
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
1. Set `ASPIRE_TEST_MODE=AdminE2E` environment variable
2. Start main AppHost in AdminE2E mode (PostgreSQL, API, Admin app only)
3. **Reuse the same Aspire instance for all tests** (even faster)
4. Run Playwright E2E tests against the running application
5. Clean up all resources when complete

## How It Works

### Environment-Based Service Filtering

The main `AppHost.cs` checks the `ASPIRE_TEST_MODE` environment variable:

```csharp
var testMode = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE");
var isAdminE2ETest = testMode == "AdminE2E";

// Only add services when NOT in AdminE2E mode
if (!isAdminE2ETest)
{
    builder.AddRedis("cache");
    builder.AddAzureServiceBus("servicebus");
    builder.AddViteApp("app-designer", "../Designer");
    // Azure Functions...
}
```

### Performance Optimizations

**Minimal Services via Environment Variable**
- AppHost checks `ASPIRE_TEST_MODE` and skips unnecessary services
- Only starts: PostgreSQL, API, and Admin app
- Skips: Redis, Azure Service Bus, Designer app, Azure Functions
- Result: ~60% fewer services = faster startup

**Shared Aspire Instance**
- Single Aspire instance shared across all tests via xUnit collection
- First test pays startup cost (~20-30 seconds)
- Subsequent tests run immediately (~1-5 seconds each)

**Expected Timings**
- First test (cold start): 20-40 seconds
- Subsequent tests: 1-5 seconds each
- Total for 2 tests: ~25-45 seconds

If tests take longer than 60 seconds, see troubleshooting section below.

## Architecture

1. **AppHost (Main)**: Production AppHost with environment-based service filtering
2. **AspireAppHostFixture**: Sets `ASPIRE_TEST_MODE=AdminE2E` and starts the main AppHost
3. **AdminE2ECollection**: xUnit collection definition that ensures all tests share the fixture
4. **PlaywrightTestBase**: Base class for E2E tests with Playwright browser automation
5. **TagsE2ETests**: Actual test class that inherits from PlaywrightTestBase

### Test Flow

1. **xUnit** discovers tests and sees `[Collection("AdminE2E")]` attribute
2. **AdminE2ECollection** creates a single `AspireAppHostFixture` instance
3. **AspireAppHostFixture.InitializeAsync()** sets environment variable and starts main AppHost in AdminE2E mode
4. Main AppHost starts only required services (PostgreSQL, API, Admin)
5. **PlaywrightTestBase.InitializeAsync()** creates Playwright browser and page
6. **Test methods** get Admin app URL from Aspire and run Playwright tests
7. After all tests complete, fixtures are disposed and services are cleaned up

## Adding New Tests

To add a new E2E test class:

```csharp
using Microsoft.Playwright;

namespace Admin.E2ETests;

[Collection("AdminE2E")]  // ✅ IMPORTANT: Use collection to share Aspire instance
public class MyNewTests : PlaywrightTestBase
{
    public MyNewTests(AspireAppHostFixture aspireFixture) : base(aspireFixture)
    {
    }

    [Fact]
    public async Task MyTest()
    {
        Assert.NotNull(Page);
        var baseUrl = GetAdminAppUrl(); // ✅ Get URL from Aspire
        
        await Page.GotoAsync($"{baseUrl}/my-page");
        // ... test logic ...
    }
}
```

**Key points:**
- ✅ Use `[Collection("AdminE2E")]` attribute
- ✅ Inherit from `PlaywrightTestBase`
- ✅ Use `GetAdminAppUrl()` to get the Admin app URL
- ❌ Do NOT add `IClassFixture<AspireAppHostFixture>` (creates conflicts)

## Troubleshooting

### Tests Hang or Take Too Long

**Symptoms:** Tests take longer than 60 seconds or appear to hang

**Common Causes:**
1. **Docker not running**: Aspire needs Docker for PostgreSQL
   - Solution: Start Docker Desktop
2. **Insufficient resources**: PostgreSQL + API + Admin app need resources
   - Solution: Ensure Docker has at least 2-3GB RAM allocated
3. **First run**: Docker images need to be downloaded
   - Solution: Wait for first run (can take 2-3 minutes), subsequent runs are faster
4. **Port conflicts**: Another service using required ports
   - Solution: Stop other services or let Aspire allocate dynamic ports

### Conflicting Fixture Patterns

**Symptoms:** Tests hang or fixtures are created multiple times

**Problem:** Using both `[Collection("AdminE2E")]` and `IClassFixture<AspireAppHostFixture>` causes xUnit to try creating the fixture twice.

**Solution:** Use ONLY the collection:
```csharp
[Collection("AdminE2E")]  // ✅ Correct
public class MyTests : PlaywrightTestBase
{
    // ❌ Do NOT add: IClassFixture<AspireAppHostFixture>
}
```

### Environment Variable Not Set

**Symptoms:** All services start (Redis, Service Bus, etc.) even in tests

**Problem:** `ASPIRE_TEST_MODE` environment variable not set

**Solution:** The `AspireAppHostFixture.InitializeAsync()` should set it automatically. If not working, check:
1. Fixture is being used (via collection)
2. No other code is clearing environment variables
3. Tests are running in the same process

### Playwright Browsers Not Installed

**Symptoms:** Error about missing Chromium or browser executables

**Solution:** Install Playwright browsers:
```bash
cd src/Admin.E2ETests
dotnet build
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

## Project Structure

```
src/Admin.E2ETests/
├── Admin.E2ETests.csproj      # Project file with AppHost reference
├── AdminE2ECollection.cs      # xUnit collection definition
├── AspireAppHostFixture.cs    # Starts main AppHost in AdminE2E mode
├── PlaywrightTestBase.cs      # Base class for Playwright tests
├── TagsE2ETests.cs            # Example E2E test class
├── TestHelpers.cs             # Helper methods for UI interactions
└── README.md                  # This file
```

## Comparison with Api.IntegrationTests

Both test projects follow the same pattern:

| Aspect | Api.IntegrationTests | Admin.E2ETests |
|--------|---------------------|----------------|
| AppHost | Uses main AppHost | Uses main AppHost |
| Service Filtering | None (all services) | Via `ASPIRE_TEST_MODE=AdminE2E` |
| Fixture Pattern | `IClassFixture` per test class | `[Collection]` shared across all |
| Test Framework | xUnit v3 | xUnit v3 |
| What's Tested | API endpoints (HTTP) | UI flows (Playwright) |

The key difference is that Admin.E2ETests uses environment-based filtering to skip unnecessary services, making tests faster while still using the same main AppHost configuration.
