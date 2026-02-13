using Api.Features.Clients;
using System.Net.Http.Json;

namespace Api.E2ETests;

/// <summary>
/// E2E tests for Clients management in the Admin app.
/// These tests cover the full stack: Frontend UI -> API -> Database
/// </summary>
public class AdminClientsE2ETests(E2ETestFixture fixture)
{
    [Fact]
    public async Task CreateClient_ThroughUI_ShouldPersistInDatabase()
    {
        // Arrange
        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();
        var accountName = $"E2E Test Client {Guid.NewGuid()}";
        var companyNumber = $"CN-{Guid.NewGuid().ToString()[..8]}";
        var customerNumber = $"CUST-{Guid.NewGuid().ToString()[..8]}";
        var companyCode = $"CC-{Guid.NewGuid().ToString()[..8]}";

        try
        {
            // Navigate directly to the Clients page
            await page.GotoAsync($"{adminUrl}/clients", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render (proves API call completed)
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Click "New" button in the command bar
            await page.Locator("button.cmd-btn.primary:has-text('New')").ClickAsync();

            // Wait for the side panel form to open and fill in client details
            var accountNameInput = page.Locator("#accountName");
            await accountNameInput.WaitForAsync(new() { Timeout = 5000 });
            await accountNameInput.FillAsync(accountName);
            await page.Locator("#companyNumber").FillAsync(companyNumber);
            await page.Locator("#customerNumber").FillAsync(customerNumber);
            await page.Locator("#companyCode").FillAsync(companyCode);

            // Click Save button
            await page.Locator("button:has-text('Save')").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the client appears in the list
            var clientCell = page.Locator($"td:has-text('{accountName}')");
            await clientCell.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await clientCell.IsVisibleAsync(), $"Expected client '{accountName}' to be visible in the list");

            // Verify in database via API
            using var apiClient = fixture.CreateApiClient();
            var response = await apiClient.GetAsync("/api/clients", TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<GetClientsResponse>>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(clients);
            Assert.Contains(clients, c => c.AccountName == accountName && c.CompanyNumber == companyNumber);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task EditClient_ThroughUI_ShouldUpdateInDatabase()
    {
        // Arrange - Create a client via API first
        using var apiClient = fixture.CreateApiClient();
        var originalAccountName = $"E2E Original Client {Guid.NewGuid()}";
        var originalCompanyNumber = $"ORIG-{Guid.NewGuid().ToString()[..8]}";
        var originalCustomerNumber = $"CUST-ORIG-{Guid.NewGuid().ToString()[..8]}";
        var originalCompanyCode = $"CC-ORIG-{Guid.NewGuid().ToString()[..8]}";
        var updatedAccountName = $"E2E Updated Client {Guid.NewGuid()}";
        var updatedCompanyNumber = $"UPDT-{Guid.NewGuid().ToString()[..8]}";

        var createResponse = await apiClient.PostAsJsonAsync("/api/clients",
            new CreateClientRequest(originalAccountName, originalCompanyNumber, originalCustomerNumber, originalCompanyCode),
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdClient);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Navigate directly to the Clients page
            await page.GotoAsync($"{adminUrl}/clients", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Find the client row and click Edit button (icon button with title="Edit")
            var clientRow = page.Locator($"tr:has-text('{originalAccountName}')");
            await clientRow.Locator("button[title='Edit']").ClickAsync();

            // Wait for the side panel form and update client details
            var accountNameInput = page.Locator("#accountName");
            await accountNameInput.WaitForAsync(new() { Timeout = 5000 });
            await accountNameInput.ClearAsync();
            await accountNameInput.FillAsync(updatedAccountName);

            var companyNumberInput = page.Locator("#companyNumber");
            await companyNumberInput.ClearAsync();
            await companyNumberInput.FillAsync(updatedCompanyNumber);

            // Click Save button
            await page.Locator("button:has-text('Save')").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the updated client appears in the list
            var updatedClientCell = page.Locator($"td:has-text('{updatedAccountName}')");
            await updatedClientCell.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await updatedClientCell.IsVisibleAsync(), $"Expected updated client '{updatedAccountName}' to be visible in the list");

            // Verify in database via API
            var getResponse = await apiClient.GetAsync($"/api/clients/{createdClient.Id}", TestContext.Current.CancellationToken);
            getResponse.EnsureSuccessStatusCode();
            var fetchedClient = await getResponse.Content.ReadFromJsonAsync<GetClientsResponse>(cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(fetchedClient);
            Assert.Equal(updatedAccountName, fetchedClient.AccountName);
            Assert.Equal(updatedCompanyNumber, fetchedClient.CompanyNumber);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SearchClients_ThroughUI_ShouldFilterResults()
    {
        // Arrange - Create multiple clients via API
        using var apiClient = fixture.CreateApiClient();
        var searchTerm = $"SearchableClient{Guid.NewGuid().ToString()[..8]}";
        var client1Name = $"{searchTerm} One";
        var client2Name = $"{searchTerm} Two";
        var client3Name = $"Different Client {Guid.NewGuid()}";

        await apiClient.PostAsJsonAsync("/api/clients",
            new CreateClientRequest(client1Name, "SC1", "CUST1", "CC1"),
            TestContext.Current.CancellationToken);
        await apiClient.PostAsJsonAsync("/api/clients",
            new CreateClientRequest(client2Name, "SC2", "CUST2", "CC2"),
            TestContext.Current.CancellationToken);
        await apiClient.PostAsJsonAsync("/api/clients",
            new CreateClientRequest(client3Name, "DC3", "CUST3", "CC3"),
            TestContext.Current.CancellationToken);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Navigate directly to the Clients page
            await page.GotoAsync($"{adminUrl}/clients", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // Enter search term
            var searchInput = page.GetByPlaceholder("Search clients...");
            await searchInput.FillAsync(searchTerm);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify filtered results
            var client1Cell = page.Locator($"td:has-text('{client1Name}')");
            var client2Cell = page.Locator($"td:has-text('{client2Name}')");
            var client3Cell = page.Locator($"td:has-text('{client3Name}')");

            await client1Cell.WaitForAsync(new() { Timeout = 10000 });
            Assert.True(await client1Cell.IsVisibleAsync(), $"Expected client1 '{client1Name}' to be visible");
            Assert.True(await client2Cell.IsVisibleAsync(), $"Expected client2 '{client2Name}' to be visible");
            Assert.False(await client3Cell.IsVisibleAsync(), $"Expected client3 '{client3Name}' to not be visible");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task DeleteClient_ThroughUI_ShouldRemoveFromDatabase()
    {
        // Arrange - Create a client via API first
        using var apiClient = fixture.CreateApiClient();
        var accountName = $"E2E Delete Client {Guid.NewGuid()}";
        var companyNumber = $"DEL-{Guid.NewGuid().ToString()[..8]}";
        var customerNumber = $"CUST-DEL-{Guid.NewGuid().ToString()[..8]}";
        var companyCode = $"CC-DEL-{Guid.NewGuid().ToString()[..8]}";

        var createResponse = await apiClient.PostAsJsonAsync("/api/clients",
            new CreateClientRequest(accountName, companyNumber, customerNumber, companyCode),
            TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();
        var createdClient = await createResponse.Content.ReadFromJsonAsync<CreateClientResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(createdClient);

        var page = await fixture.CreatePageAsync();
        var adminUrl = fixture.GetAdminAppUrl();

        try
        {
            // Navigate directly to the Clients page
            await page.GotoAsync($"{adminUrl}/clients", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for the table to render
            await page.Locator("table.details-list").WaitForAsync(new() { Timeout = 15000 });

            // The delete handler uses browser confirm() dialog — accept it automatically
            page.Dialog += async (_, dialog) => await dialog.AcceptAsync();

            // Find the client row and click Delete button (icon button with title="Delete")
            var clientRow = page.Locator($"tr:has-text('{accountName}')");
            await clientRow.Locator("button[title='Delete']").ClickAsync();

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the client no longer appears in the list
            await Expect(page.Locator($"td:has-text('{accountName}')")).ToHaveCountAsync(0);

            // Verify removed from database via API
            var getResponse = await apiClient.GetAsync($"/api/clients/{createdClient.Id}", TestContext.Current.CancellationToken);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static ILocatorAssertions Expect(ILocator locator)
        => Assertions.Expect(locator);
}
