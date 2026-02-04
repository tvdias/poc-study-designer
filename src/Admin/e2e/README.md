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

### Option 1: With Aspire (Recommended)

The easiest way to run E2E tests is using .NET Aspire to orchestrate all services:

1. Start the application with Aspire:
   ```bash
   # From the project root
   aspire run
   ```

2. In a separate terminal, run the E2E tests:
   ```bash
   cd src/Admin
   npm run test:e2e
   ```

### Option 2: Manual Setup

If you prefer to run services manually:

1. Start the API and database services
2. Start the Admin dev server:
   ```bash
   cd src/Admin
   npm run dev
   ```

3. In a separate terminal, run the tests with a custom base URL:
   ```bash
   BASE_URL=http://localhost:5173 npm run test:e2e
   ```

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

- **Base URL**: `http://localhost:5173` (can be overridden with `BASE_URL` env var)
- **Browser**: Chromium (Desktop Chrome)
- **Workers**: 1 (tests run sequentially to avoid conflicts)
- **Retry**: 2 on CI, 0 locally
- **Web Server**: Automatically starts `npm run dev` if not already running

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

### Tests fail with "Connection refused"
- Ensure the API and Admin dev server are running
- Check the BASE_URL is correct
- Verify all services are healthy in Aspire dashboard

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
