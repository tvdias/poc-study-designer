using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;
using System.Diagnostics;

namespace Admin.E2ETests;

/// <summary>
/// Base class for E2E tests using Playwright.
/// Handles Playwright browser initialization and Vite dev server management.
/// Gets the API URL from Aspire and starts a local Vite dev server with that configuration.
/// </summary>
public class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private static Process? _viteProcess;
    private static string? _adminAppUrl;
    private static readonly SemaphoreSlim _viteServerLock = new(1, 1);

    protected AspireAppHostFixture AspireFixture { get; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    protected PlaywrightTestBase(AspireAppHostFixture aspireFixture)
    {
        AspireFixture = aspireFixture;
    }

    /// <summary>
    /// Gets the base URL of the Admin application.
    /// Starts the Vite dev server if it hasn't been started yet.
    /// </summary>
    protected async Task<string> GetAdminAppUrlAsync()
    {
        await _viteServerLock.WaitAsync();
        try
        {
            if (_adminAppUrl == null)
            {
                // Get API URL from Aspire
                var apiUrl = await GetApiUrlAsync();
                
                // Start Vite dev server
                await StartViteServerAsync(apiUrl);
                
                // Admin app will run on port 5174 (Vite's default + 1 to avoid conflicts)
                _adminAppUrl = "http://localhost:5174";
            }
            return _adminAppUrl;
        }
        finally
        {
            _viteServerLock.Release();
        }
    }

    /// <summary>
    /// Starts the Vite dev server for the Admin app.
    /// </summary>
    private async Task StartViteServerAsync(string apiUrl)
    {
        if (_viteProcess != null)
        {
            return; // Already started
        }

        // Get path to Admin directory
        var adminPath = Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(typeof(PlaywrightTestBase).Assembly.Location)!,
            "..", "..", "..", "..", "Admin"));

        // Start Vite dev server
        var startInfo = new ProcessStartInfo
        {
            FileName = "npm",
            Arguments = "run dev -- --port 5174 --strictPort",
            WorkingDirectory = adminPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        // Set API URL as environment variable for Vite
        startInfo.EnvironmentVariables["VITE_API_URL"] = apiUrl;

        _viteProcess = Process.Start(startInfo);
        
        if (_viteProcess == null)
        {
            throw new InvalidOperationException("Failed to start Vite dev server");
        }

        // Wait for Vite to be ready (look for "Local:" in output)
        var ready = false;
        var timeout = TimeSpan.FromSeconds(60);
        var start = DateTime.UtcNow;

        _ = Task.Run(async () =>
        {
            while (!_viteProcess.HasExited)
            {
                var line = await _viteProcess.StandardOutput.ReadLineAsync();
                if (line != null && (line.Contains("Local:") || line.Contains("ready in")))
                {
                    ready = true;
                    break;
                }
            }
        });

        while (!ready && DateTime.UtcNow - start < timeout)
        {
            if (_viteProcess.HasExited)
            {
                throw new InvalidOperationException($"Vite dev server exited with code {_viteProcess.ExitCode}");
            }
            await Task.Delay(500);
        }

        if (!ready)
        {
            _viteProcess.Kill();
            throw new TimeoutException("Vite dev server did not start within 60 seconds");
        }

        // Give it a bit more time to be fully ready
        await Task.Delay(2000);
    }

    /// <summary>
    /// Gets the API URL from Aspire.
    /// Waits up to 60 seconds for the API to become available.
    /// </summary>
    private async Task<string> GetApiUrlAsync()
    {
        var timeout = TimeSpan.FromSeconds(60);
        var start = DateTime.UtcNow;
        
        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                var client = AspireFixture.App.CreateHttpClient("api");
                var apiUrl = client.BaseAddress?.ToString().TrimEnd('/');
                
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    // Verify the API is actually responding
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    try
                    {
                        var response = await client.GetAsync("/health", cts.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            return apiUrl;
                        }
                    }
                    catch
                    {
                        // Service not ready yet, continue waiting
                    }
                }
            }
            catch
            {
                // Resource not allocated yet, continue waiting
            }
            
            await Task.Delay(1000); // Wait 1 second before retry
        }
        
        throw new TimeoutException($"API did not become available within {timeout.TotalSeconds} seconds");
    }

    public async ValueTask InitializeAsync()
    {
        // Note: Playwright browsers should be installed once via: pwsh bin/Debug/net10.0/playwright.ps1 install chromium

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

        // Note: We don't kill the Vite process here because it's shared across all tests
        // It will be cleaned up when the test process exits
    }
}
