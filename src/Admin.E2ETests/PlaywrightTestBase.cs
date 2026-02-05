using System.Diagnostics;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Playwright;

namespace Admin.E2ETests;

/// <summary>
/// Base class for E2E tests using Playwright.
/// Handles Playwright browser initialization and manually starts the Vite dev server.
/// The Admin Vite app is NOT started by Aspire in AdminE2E mode to avoid slow npm operations.
/// Instead, we manually start it here with the API URL from Aspire.
/// 
/// Note: The Vite server is started once and shared across all tests via static fields.
/// It will be cleaned up when the test process exits. This is safe because all tests
/// in the AdminE2E collection run sequentially (xUnit collection fixtures enforce this).
/// </summary>
public class PlaywrightTestBase : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    
    // Static fields for shared Vite server across all tests in the collection
    // Safe because xUnit collection fixtures ensure tests run sequentially
    private static Process? _viteProcess;
    private static readonly SemaphoreSlim _viteStartLock = new(1, 1);
    private static TaskCompletionSource<bool>? _viteReadyTcs;
    private const string ViteUrl = "http://localhost:5174";
    private const int VitePort = 5174;
    private const int ViteStartupTimeoutSeconds = 60;
    private const int ViteStabilizationDelayMs = 2000; // Additional wait after Vite reports ready to ensure full stability

    protected AspireAppHostFixture AspireFixture { get; }
    protected IBrowserContext? Context { get; private set; }
    protected IPage? Page { get; private set; }

    protected PlaywrightTestBase(AspireAppHostFixture aspireFixture)
    {
        AspireFixture = aspireFixture;
    }

    /// <summary>
    /// Gets the base URL of the Admin application (manually started Vite server).
    /// </summary>
    protected string GetAdminAppUrl()
    {
        return ViteUrl;
    }

    /// <summary>
    /// Starts the Vite dev server manually if not already running.
    /// Gets the API URL from Aspire and passes it to Vite as an environment variable.
    /// </summary>
    private async Task StartViteServerAsync()
    {
        await _viteStartLock.WaitAsync();
        try
        {
            // If already started, wait for it to be ready and return
            if (_viteProcess != null && _viteReadyTcs != null)
            {
                await _viteReadyTcs.Task;
                return;
            }

            // Get API URL from Aspire
            var apiClient = AspireFixture.App.CreateHttpClient("api");
            var apiUrl = apiClient.BaseAddress?.ToString().TrimEnd('/') ?? throw new InvalidOperationException("API URL not found");

            _viteReadyTcs = new TaskCompletionSource<bool>();

            var adminPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Admin"));
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "run dev",
                WorkingDirectory = adminPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Pass API URL to Vite
            startInfo.Environment["VITE_API_URL"] = apiUrl;

            _viteProcess = new Process { StartInfo = startInfo };

            // Capture output to detect when server is ready
            _viteProcess.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    Console.WriteLine($"[Vite] {args.Data}");
                    if (args.Data.Contains("Local:") && args.Data.Contains(VitePort.ToString()))
                    {
                        _viteReadyTcs?.TrySetResult(true);
                    }
                }
            };

            _viteProcess.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                {
                    Console.WriteLine($"[Vite Error] {args.Data}");
                }
            };

            _viteProcess.Start();
            _viteProcess.BeginOutputReadLine();
            _viteProcess.BeginErrorReadLine();

            // Wait for Vite to be ready
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(ViteStartupTimeoutSeconds));
            var completedTask = await Task.WhenAny(_viteReadyTcs.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                _viteProcess?.Kill();
                throw new TimeoutException($"Vite dev server failed to start within {ViteStartupTimeoutSeconds} seconds");
            }

            // Additional wait to ensure server is fully ready to accept connections
            await Task.Delay(ViteStabilizationDelayMs);

            Console.WriteLine($"Vite dev server started successfully at {ViteUrl} with API URL {apiUrl}");
        }
        finally
        {
            _viteStartLock.Release();
        }
    }

    public async ValueTask InitializeAsync()
    {
        // Note: Playwright browsers should be installed once via: pwsh bin/Debug/net10.0/playwright.ps1 install chromium

        // Start Vite server if not already running
        await StartViteServerAsync();

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
