using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;

namespace Admin.E2ETests;

/// <summary>
/// Base class for E2E tests using Playwright.
/// Handles Playwright browser initialization and provides access to the Admin app URL from Aspire.
/// </summary>
public class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    protected AspireAppHostFixture AspireFixture { get; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    protected PlaywrightTestBase(AspireAppHostFixture aspireFixture)
    {
        AspireFixture = aspireFixture;
    }

    /// <summary>
    /// Gets the base URL of the Admin application from Aspire.
    /// The URL is automatically determined by Aspire and may change between runs.
    /// </summary>
    protected string GetAdminAppUrl()
    {
        // Create an HTTP client for the app-admin resource
        // This will give us the correct URL that Aspire assigned
        var client = AspireFixture.App.CreateHttpClient("app-admin");
        var baseUrl = client.BaseAddress?.ToString().TrimEnd('/');
        
        if (string.IsNullOrEmpty(baseUrl))
        {
            throw new InvalidOperationException("Admin app URL could not be determined from Aspire");
        }

        return baseUrl;
    }

    public async ValueTask InitializeAsync()
    {
        // Install Playwright browsers if needed
        Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });
        Context = await _browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }
        if (Context != null)
        {
            await Context.CloseAsync();
        }
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
        _playwright?.Dispose();
    }
}
