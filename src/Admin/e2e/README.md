# E2E Tests for Admin Application

This directory contains end-to-end (E2E) tests for the Admin application using Playwright.

## Overview

The E2E tests cover the complete user flows from UI to API with real databases for the following features:
- **Tags**: Create, Read, Update, Delete operations
- **Commissioning Markets**: Create, Read, Update, Delete operations
- **Fieldwork Markets**: Create, Read, Update, Delete operations

## Prerequisites

Before running the E2E tests, ensure you have:
- Node.js 18.x or later
- .NET 10.0 SDK
- .NET Aspire 13
- Docker Desktop (or Podman)
- All project dependencies installed

## Running E2E Tests

### ⚠️ Important: Backend API Required

**E2E tests require a running backend API** to function properly. You have two options:

### Option 1: With Aspire (Recommended - Full Stack)

This approach runs the complete application stack including PostgreSQL, Redis, and all services.

1. **Install Aspire workload** (if not already installed):
   ```bash
   dotnet workload install aspire
   ```

2. **Start the application with Aspire**:
   ```bash
   # From the project root
   cd src/AppHost
   dotnet run
   ```
   
   This will:
   - Start the Aspire Dashboard (typically at http://localhost:15888)
   - Orchestrate all services (API, databases, Redis, etc.)
   - Start the Admin frontend

3. **Note the Admin app URL** from the Aspire dashboard. It's usually something like:
   - `http://localhost:5173` (or another port if that's occupied)

4. **Run the E2E tests** in a separate terminal:
   ```bash
   cd src/Admin
   
   # If the Admin app is at http://localhost:5173 (default)
   npm run test:e2e
   
   # If the Admin app is at a different URL (e.g., http://localhost:5174)
   PLAYWRIGHT_BASE_URL=http://localhost:5174 npm run test:e2e
   ```

### Option 2: With Standalone Dev Server (Frontend Only)

If you want to run tests without Aspire, you can start just the frontend dev server. However, **you must have the API running separately** for tests to work.

1. **Start the API service** (see API documentation for setup)

2. **Set the API URL** environment variable:
   ```bash
   # Example: API running at http://localhost:5000
   export services__api__http__0=http://localhost:5000
   ```

3. **Run the E2E tests** (this will auto-start the dev server):
   ```bash
   cd src/Admin
   npm run test:e2e
   ```

   The tests will automatically:
   - Start the Vite dev server on http://localhost:5173
   - Proxy `/api` calls to your backend
   - Run all tests
   - Stop the dev server when complete

**Note**: If you get connection errors, ensure:
- The API service is running and accessible
- Port 5173 is available (or set a different port in vite.config.ts)
- The API proxy target is correctly configured

## Available Test Commands

- `npm run test:e2e` - Run all E2E tests in headless mode
- `npm run test:e2e:headed` - Run tests in headed mode (visible browser)
- `npm run test:e2e:ui` - Run tests with Playwright UI mode (interactive)
- `npx playwright test tags.spec.ts` - Run only Tags tests
- `npx playwright test --grep "CRUD"` - Run tests matching a pattern

## Test Structure

```
e2e/
├── helpers.ts                      # Shared test utilities
├── tags.spec.ts                    # Tags feature tests
├── commissioning-markets.spec.ts   # Commissioning Markets tests
└── fieldwork-markets.spec.ts       # Fieldwork Markets tests
```

## Test Coverage

Each feature test includes:

1. **Full CRUD Flow Test**:
   - Navigate to the feature page
   - Create a new entry
   - Verify it appears in the list
   - Edit the entry
   - Verify the update is reflected
   - Delete the entry
   - Verify it's removed from the list

2. **Validation Test**:
   - Attempt to create an entry without required fields
   - Verify appropriate validation errors are shown

3. **Duplicate Handling Test**:
   - Create an entry
   - Attempt to create another with the same identifier
   - Verify conflict error is shown
   - Clean up the test data

## Configuration

Test configuration is in `playwright.config.ts`. Key settings:

- **Base URL**: Configurable via environment variables:
  - `PLAYWRIGHT_BASE_URL` (preferred)
  - `BASE_URL` (alternative)
  - Default: `http://localhost:5173`
  - **Important**: Set this to match the Admin app URL shown in the Aspire dashboard
- **Browser**: Chromium (Desktop Chrome)
- **Workers**: 1 (tests run sequentially to avoid database conflicts)
- **Retry**: 2 on CI, 0 locally
- **Web Server**: Not auto-started (must be started via Aspire before running tests)

## Viewing Test Results

After running tests, you can view detailed reports:

```bash
npx playwright show-report
```

This opens an HTML report with:
- Test execution timeline
- Screenshots on failure
- Video recordings (on failure)
- Trace viewer for debugging

## Debugging Tests

### Run in UI Mode
```bash
npm run test:e2e:ui
```

### Run in Headed Mode
```bash
npm run test:e2e:headed
```

### Debug a Specific Test
```bash
npx playwright test tags.spec.ts --debug
```

### Use Playwright Inspector
Tests automatically pause if you add `await page.pause()` in the test code.

## Best Practices

1. **Unique Test Data**: Tests generate unique names using timestamps to avoid conflicts
2. **Cleanup**: Tests clean up their data after validation/error scenarios
3. **Isolation**: Each test is independent and doesn't rely on others
4. **Assertions**: Tests verify both positive and negative scenarios
5. **Timeouts**: Reasonable timeouts are set for async operations

## CI/CD Integration

On CI:
- Tests run in headless mode
- Retries are enabled (2 attempts)
- HTML reports are generated
- Set `BASE_URL` environment variable to point to the test environment

Example CI configuration:
```yaml
- name: Run E2E tests
  run: |
    cd src/Admin
    npm run test:e2e
  env:
    BASE_URL: ${{ env.ADMIN_URL }}
```

## Troubleshooting

### Tests fail with "ERR_CONNECTION_REFUSED" or "Connection refused"

**This error means the application is not running.** You have two solutions:

**Solution 1: Use Aspire (Recommended)**
```bash
# Terminal 1: Start Aspire
cd src/AppHost
dotnet run

# Terminal 2: Run tests (after Aspire shows services are ready)
cd src/Admin
npm run test:e2e
```

**Solution 2: Run API separately**
```bash
# Terminal 1: Start the API service
cd src/Api
dotnet run

# Terminal 2: Set API URL and run tests
cd src/Admin
export services__api__http__0=http://localhost:5000  # Or your API URL
npm run test:e2e
```

### Tests fail with "Navigation timeout"
- **Ensure Aspire is running**: Check that `dotnet run` is active in the AppHost directory
- **Verify services are healthy**: Open the Aspire dashboard (http://localhost:15888) and ensure all services are green
- **Check the Admin app URL**: Make sure `PLAYWRIGHT_BASE_URL` matches the URL shown in Aspire
- **Wait for all services to start**: Give Aspire 30-60 seconds to start all containers before running tests

### Tests fail with API errors (404, 500, etc.)
- **Verify API connectivity**: The Admin frontend proxies `/api` calls to the backend
- **Check API is accessible**: Try accessing http://localhost:5000/api/tags (or your API URL) in a browser
- **Verify database is running**: The API needs PostgreSQL to be running
- **Check API logs**: Look in Aspire dashboard or API console for error messages

### Tests fail with database errors
- Check PostgreSQL container is running in Aspire
- Verify database migrations have been applied
- Check Aspire dashboard logs for database connection issues

### Port 5173 is already in use
If you see "Port 5173 is already in use":
- Stop any existing Vite dev servers
- Or change the port in `vite.config.ts`:
  ```typescript
  server: {
    port: 5174,  // Use a different port
    proxy: { /* ... */ }
  }
  ```
- Then update tests: `PLAYWRIGHT_BASE_URL=http://localhost:5174 npm run test:e2e`

### Tests timeout
- Increase timeouts in `playwright.config.ts` if needed
- Check for slow database queries or API responses
- Ensure Docker containers are running properly

### Tests are flaky
- Check for race conditions in the application
- Increase wait times for async operations
- Review network conditions

## Contributing

When adding new E2E tests:
1. Follow the existing test structure and naming conventions
2. Use the `TestHelpers` class for common operations
3. Generate unique test data to avoid conflicts
4. Clean up test data after validation/error scenarios
5. Add descriptive test names that explain the scenario
6. Document any special setup or configuration needed
