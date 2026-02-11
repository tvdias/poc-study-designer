# E2E Tests

This project contains End-to-End (E2E) tests for the POC Study Designer application. These tests cover the full stack from the React frontends (Admin and Designer apps) through the API to the database.

## Technology Stack

- **xUnit.v3** - Test framework using the new Microsoft Testing Platform
- **Aspire.Hosting.Testing** - Aspire integration for spinning up the full application stack
- **Playwright** - Browser automation for testing the React frontends

## Test Structure

The E2E tests are organized by application area:

### AdminTagsE2ETests
Tests for Tags management in the Admin app:
- Create tag through UI and verify database persistence
- Edit tag through UI and verify database update
- Delete tag through UI and verify removal from database
- View tag details through UI

### AdminClientsE2ETests
Tests for Clients management in the Admin app:
- Create client through UI and verify database persistence
- Edit client through UI and verify database update
- Delete client through UI and verify removal from database
- Search/filter clients through UI

### DesignerAppE2ETests
Tests for basic Designer app functionality:
- App loads successfully
- Navigation works correctly
- API connectivity
- No console or page errors

## Test Fixture

The `E2ETestFixture` class provides:
- Full Aspire application startup (API, Database, Redis, Frontend apps)
- Playwright browser automation (Chromium in headless mode)
- Helper methods for:
  - Creating new pages with isolated contexts
  - Getting URLs for Admin app, Designer app, and API
  - Creating HTTP clients for direct API calls (for setup/verification)

## Prerequisites

Before running the E2E tests, you need to install Playwright browsers:

```bash
# Navigate to the E2E tests project
cd src/Api.E2ETests

# Install Playwright browsers
pwsh bin/Debug/net10.0/playwright.ps1 install
# OR on Linux/Mac:
# ./bin/Debug/net10.0/playwright.sh install
```

## Running the Tests

### Using dotnet test

```bash
# Run all E2E tests
dotnet test src/Api.E2ETests

# Run specific test class
dotnet test src/Api.E2ETests --filter "FullyQualifiedName~AdminTagsE2ETests"

# Run specific test method
dotnet test src/Api.E2ETests --filter "FullyQualifiedName~CreateTag_ThroughUI_ShouldPersistInDatabase"
```

### Using Aspire Dashboard

```bash
# Start the full stack with Aspire
dotnet run --project src/AppHost

# Then run tests in a separate terminal
dotnet test src/Api.E2ETests
```

### CI/CD Considerations

The tests are configured to run in headless mode with `--no-sandbox` and `--disable-setuid-sandbox` flags for CI/CD environments like GitHub Actions.

## Test Patterns

### Arrange-Act-Assert Pattern

Each test follows the AAA pattern:
1. **Arrange**: Set up test data (often via API), create a browser page, navigate to the app
2. **Act**: Perform UI interactions (clicks, form fills, etc.)
3. **Assert**: Verify UI state AND database state (via API calls)

### Test Isolation

- Each test method gets a fresh browser context and page
- Tests create their own test data with unique identifiers (GUIDs)
- The Aspire fixture is shared across tests in the same class for performance
- Database state is not rolled back between tests (rely on unique test data)

### Database Verification

Tests verify both:
1. **UI State**: That the UI reflects the changes (using Playwright assertions)
2. **Database State**: That the changes persisted to the database (using HTTP client API calls)

This ensures the full stack is working correctly.

## Debugging Tests

### View Browser During Tests

To see the browser during test execution (non-headless mode), modify `E2ETestFixture.cs`:

```csharp
Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false,  // Change to false
    Args = ["--no-sandbox", "--disable-setuid-sandbox"]
});
```

### Enable Playwright Tracing

Add tracing to capture screenshots, videos, and traces on failure:

```csharp
var context = await Browser.NewContextAsync(new BrowserNewContextOptions
{
    IgnoreHTTPSErrors = true,
    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
    RecordVideoDir = "videos/",
    RecordVideoSize = new RecordVideoSize { Width = 1920, Height = 1080 }
});

await context.Tracing.StartAsync(new TracingStartOptions
{
    Screenshots = true,
    Snapshots = true
});

// ... run test ...

await context.Tracing.StopAsync(new TracingStopOptions
{
    Path = "trace.zip"
});
```

### Check Aspire Logs

When tests fail, check the Aspire dashboard logs:
1. The fixture starts the full Aspire application
2. All service logs are available in the Aspire dashboard
3. Look for errors in the API, Database, or Frontend logs

## Known Issues & Limitations

1. **Test Data Cleanup**: Tests do not clean up test data. This is intentional to avoid database resets that could slow down tests. Tests use unique identifiers to avoid conflicts.

2. **Timing Issues**: Some UI operations may need additional wait times depending on the environment. Adjust `WaitForLoadStateAsync` timeouts as needed.

3. **Selector Brittleness**: Tests use text-based selectors (e.g., `"button:has-text('Save')"`). If UI text changes, tests will need updates. Consider adding `data-testid` attributes to the React components for more stable selectors.

## Extending the Tests

To add new E2E tests:

1. Create a new test class that accepts `E2ETestFixture` in its constructor
2. Add the `IClassFixture<E2ETestFixture>` attribute to share the fixture
3. Use `fixture.CreatePageAsync()` to get a new browser page
4. Navigate to the app using `fixture.GetAdminAppUrl()` or `fixture.GetDesignerAppUrl()`
5. Perform UI interactions using Playwright API
6. Verify database state using `fixture.CreateApiClient()`
7. Don't forget to close the page in a finally block

Example:

```csharp
public class MyNewE2ETests(E2ETestFixture fixture) : IClassFixture<E2ETestFixture>
{
    [Fact]
    public async Task MyTest_ShouldWork()
    {
        var page = await fixture.CreatePageAsync();
        try
        {
            await page.GotoAsync(fixture.GetAdminAppUrl());
            // ... test logic ...
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
```

## Performance Tips

1. **Share the Fixture**: Use `IClassFixture<E2ETestFixture>` to share the Aspire app instance across tests in the same class
2. **Parallel Execution**: xUnit runs test classes in parallel by default, but tests within a class run sequentially
3. **Selective Testing**: Use `--filter` to run specific tests during development
4. **Fast Feedback**: Start with unit tests, then integration tests, then E2E tests

## Resources

- [xUnit.v3 Documentation](https://xunit.net/docs/getting-started/v3)
- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Aspire Testing Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/testing/testing-overview)
