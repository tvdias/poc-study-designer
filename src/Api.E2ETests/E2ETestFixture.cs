extern alias AppHostAssembly;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;
using Xunit;

[assembly: AssemblyFixture(typeof(Api.E2ETests.E2ETestFixture))]

namespace Api.E2ETests;

/// <summary>
/// Fixture for E2E tests that starts the full Aspire application including frontend apps.
/// This fixture is shared across all E2E tests in the same test class.
/// </summary>
public class E2ETestFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = null!;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        // Start the Aspire application with all services (API, DB, Redis, Frontend apps)
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHostAssembly::Program>();
        App = await appHost.BuildAsync();
        await App.StartAsync();

        // Initialize Playwright for browser automation
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // Launch browser in headless mode for CI/CD compatibility
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        });

        // Wait for services to be ready
        var httpClient = new HttpClient();
        await WaitForResourceAsync(httpClient, GetApiUrl());
        await WaitForResourceAsync(httpClient, GetAdminAppUrl());
        await WaitForResourceAsync(httpClient, GetDesignerAppUrl());
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        if (Playwright != null)
        {
            Playwright.Dispose();
        }

        if (App != null)
        {
            await App.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a new browser context with a fresh page for test isolation.
    /// </summary>
    public async Task<IPage> CreatePageAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        return await context.NewPageAsync();
    }

    /// <summary>
    /// Gets the base URL for the Admin app.
    /// </summary>
    public string GetAdminAppUrl()
    {
        var endpoint = App.GetEndpoint("app-admin", "http");
        return endpoint.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Gets the base URL for the Designer app.
    /// </summary>
    public string GetDesignerAppUrl()
    {
        var endpoint = App.GetEndpoint("app-designer", "http");
        return endpoint.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Gets the base URL for the API.
    /// </summary>
    public string GetApiUrl()
    {
        var endpoint = App.GetEndpoint("api", "http");
        return endpoint.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Creates an HTTP client for direct API calls (for setup/verification).
    /// </summary>
    public HttpClient CreateApiClient()
    {
        return App.CreateHttpClient("api");
    }


    private async Task WaitForResourceAsync(HttpClient client, string url)
    {
        var timeout = TimeSpan.FromSeconds(180);
        var delay = TimeSpan.FromSeconds(1);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Waiting for {url}: {ex.Message}");
                // Ignore transient errors during startup
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException($"Timed out waiting for resource at {url} to become available.");
    }
}
