using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

[assembly: AssemblyFixture(typeof(Api.E2ETests.E2ETestFixture))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Api.E2ETests;

/// <summary>
/// Assembly-level fixture for E2E tests that starts the full Aspire application
/// including frontend apps. Initialized once and shared across all test classes.
/// </summary>
public class E2ETestFixture : IAsyncLifetime
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(300);

    public DistributedApplication App { get; private set; } = null!;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Console.WriteLine($"[E2E] Starting Aspire application at {DateTime.UtcNow:O}");
        
        // Start the Aspire application with all services (API, DB, Frontend apps)
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();
        App = await appHost.BuildAsync();
        
        Console.WriteLine($"[E2E] Starting app resources at {DateTime.UtcNow:O}");
        await App.StartAsync().WaitAsync(DefaultTimeout);
        Console.WriteLine($"[E2E] App resources started at {DateTime.UtcNow:O}");

        // Wait for API to be healthy (it has a registered health check)
        Console.WriteLine($"[E2E] Waiting for API health check at {DateTime.UtcNow:O}");
        await App.ResourceNotifications.WaitForResourceHealthyAsync("api").WaitAsync(DefaultTimeout);
        Console.WriteLine($"[E2E] API is healthy at {DateTime.UtcNow:O}");

        // Vite apps don't have Aspire health checks, so wait for them to be running
        // and then probe their HTTP endpoints to confirm they're serving requests.
        Console.WriteLine($"[E2E] Waiting for Admin app at {DateTime.UtcNow:O}");
        await WaitForViteAppReadyAsync("app-admin");
        Console.WriteLine($"[E2E] Admin app is ready at {DateTime.UtcNow:O}");
        
        Console.WriteLine($"[E2E] Waiting for Designer app at {DateTime.UtcNow:O}");
        await WaitForViteAppReadyAsync("app-designer");
        Console.WriteLine($"[E2E] Designer app is ready at {DateTime.UtcNow:O}");

        // Initialize Playwright for browser automation
        Console.WriteLine($"[E2E] Initializing Playwright at {DateTime.UtcNow:O}");
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Launch browser in headless mode for CI/CD compatibility
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        });
        Console.WriteLine($"[E2E] E2E fixture initialization complete at {DateTime.UtcNow:O}");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (Browser != null)
        {
            await Browser.DisposeAsync();
        }

        Playwright?.Dispose();

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
    /// Creates an HTTP client configured for the API resource.
    /// </summary>
    public HttpClient CreateApiClient()
    {
        return App.CreateHttpClient("api");
    }

    /// <summary>
    /// Creates an HTTP client configured for the Admin app resource.
    /// </summary>
    public HttpClient CreateAdminClient()
    {
        return App.CreateHttpClient("app-admin");
    }

    /// <summary>
    /// Creates an HTTP client configured for the Designer app resource.
    /// </summary>
    public HttpClient CreateDesignerClient()
    {
        return App.CreateHttpClient("app-designer");
    }

    /// <summary>
    /// Waits for a Vite app to be running and able to serve HTTP requests.
    /// Vite dev servers don't register Aspire health checks, so we poll the root
    /// endpoint until it responds successfully.
    /// </summary>
    private async Task WaitForViteAppReadyAsync(string resourceName)
    {
        using var httpClient = App.CreateHttpClient(resourceName);
        var delay = TimeSpan.FromSeconds(2);
        var start = DateTime.UtcNow;
        var attempts = 0;
        Exception? lastException = null;

        while (DateTime.UtcNow - start < DefaultTimeout)
        {
            attempts++;
            try
            {
                var response = await httpClient.GetAsync("/");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[E2E] Vite app '{resourceName}' is ready after {attempts} attempts in {(DateTime.UtcNow - start).TotalSeconds:F1}s");
                    return;
                }
                lastException = new Exception($"HTTP {response.StatusCode}");
            }
            catch (Exception ex)
            {
                // Vite dev server not ready yet, retry
                lastException = ex;
            }

            if (attempts % 10 == 0)
            {
                Console.WriteLine($"[E2E] Still waiting for '{resourceName}' after {attempts} attempts ({(DateTime.UtcNow - start).TotalSeconds:F1}s elapsed)...");
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException($"Vite app '{resourceName}' did not become ready within {DefaultTimeout.TotalSeconds}s after {attempts} attempts. Last error: {lastException?.Message}");
    }
}
