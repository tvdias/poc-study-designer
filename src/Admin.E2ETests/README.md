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

Due to xUnit v3 using the new test platform, tests must be run directly using the test executable:

```bash
# Build the project
cd src/Admin.E2ETests
dotnet build

# Run all tests
./bin/Debug/net10.0/Admin.E2ETests

# List available tests
./bin/Debug/net10.0/Admin.E2ETests -list tests

# Run with specific options (e.g., diagnostics)
./bin/Debug/net10.0/Admin.E2ETests -diagnostics
```

**Note**: `dotnet test` doesn't work with xUnit v3 due to VSTest compatibility issues. Use the direct executable approach shown above.

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
| CI/CD integration | ⚠️ Complex | ✅ Integrated with .NET stack |
| Run command | `npm run test:e2e` | `./bin/Debug/net10.0/Admin.E2ETests` |

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

### Tests fail with "Playwright not installed"
The tests automatically install Playwright browsers on first run. If this fails:
```bash
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
```

### Tests timeout waiting for services
Increase timeout in `AspireAppHostFixture.InitializeAsync()` or check Docker is running.

### Can't find Admin app URL
The test uses `CreateHttpClient("app-admin")` to get the URL. Verify the AppHost has `app-admin` resource.

## CI/CD Integration

Since xUnit v3 requires running the test executable directly:

```yaml
- name: Build E2E Tests
  run: |
    cd src/Admin.E2ETests
    dotnet build
    
- name: Install Playwright Browsers
  run: |
    cd src/Admin.E2ETests
    pwsh bin/Debug/net10.0/playwright.ps1 install chromium

- name: Run E2E Tests
  run: |
    cd src/Admin.E2ETests
    ./bin/Debug/net10.0/Admin.E2ETests
  env:
    ASPIRE_ALLOW_UNSECURED_TRANSPORT: true
```

## Comparison with API Integration Tests

This project follows the same pattern as `src/Api.IntegrationTests/`:
- Uses `Aspire.Hosting.Testing`
- Uses `BoxedAppHostFixture` / `AspireAppHostFixture` pattern
- Starts the entire application stack
- Discovers service URLs automatically

The difference is that API integration tests use HttpClient to test the API directly, while these E2E tests use Playwright to test the full UI→API→Database flow.
