# E2E Tests

This project contains end-to-end (E2E) tests that exercise the full application stack (React frontends -> API -> database) using Aspire for orchestration and Playwright for browser automation.

Minimal overview:

- Purpose: verify the Admin and Designer frontends load, interact with the API, and that the API and persistence behave as expected.
- Approach: tests combine Playwright UI interactions with direct API verification via an Aspire-configured HTTP client.
- Fixture: `E2ETestFixture` starts the full Aspire application and launches a headless Chromium instance used by tests.

Supported tooling:

- .NET 10
- xUnit.v3
- Aspire.Hosting.Testing
- Playwright for .NET (Chromium headless)

Quickstart

1. Install Playwright browsers (one-time):

```bash
cd src/Api.E2ETests
# PowerShell (Windows)
pwsh bin/Debug/net10.0/playwright.ps1 install chromium
# macOS / Linux
./bin/Debug/net10.0/playwright.sh install chromium
```

2. Run the full Aspire stack (optional; the fixture also starts it during tests):

```bash
dotnet run --project src/AppHost
```

3. Run tests:

```bash
dotnet test src/Api.E2ETests
```

Notes and best practices

- Tests use an assembly-level fixture to start Aspire and Playwright; each test receives an isolated browser context/page.
- UI assertions are paired with API calls to validate persistence.
- Use stable selectors where possible (consider adding `data-testid` attributes in the frontend).
- For CI, tests run in headless mode and include common flags (`--no-sandbox`).

Debugging

- To view the browser during local runs, set `Headless = false` in `E2ETestFixture` when launching Chromium.
- Enable Playwright tracing (screenshots/traces) to capture failures.
- Check Aspire logs (dashboard) for service-level errors.
