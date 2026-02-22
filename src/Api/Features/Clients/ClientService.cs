using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Clients;

public interface IClientService
{
    Task<CreateClientResponse> CreateClientAsync(
        CreateClientRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<UpdateClientResponse> UpdateClientAsync(
        Guid clientId,
        UpdateClientRequest request,
        string userId,
        CancellationToken cancellationToken = default);

    Task<GetClientByIdResponse?> GetClientByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);

    Task<GetClientsResponse> GetClientsAsync(
        CancellationToken cancellationToken = default);

    Task DeleteClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default);
}

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientService> _logger;

    public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CreateClientResponse> CreateClientAsync(
        CreateClientRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating client: {AccountName}", request.AccountName);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            AccountName = request.AccountName,
            CompanyNumber = request.CompanyNumber,
            CustomerNumber = request.CustomerNumber,
            CompanyCode = request.CompanyCode,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Clients.Add(client);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true
                || ex.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("Duplicate client name attempted: {AccountName}", request.AccountName);
                throw new InvalidOperationException($"Client '{request.AccountName}' already exists.", ex);
            }
            throw;
        }

        _logger.LogInformation("Successfully created client {ClientId}: {AccountName}", client.Id, client.AccountName);

        return new CreateClientResponse(
            client.Id,
            client.AccountName,
            client.CompanyNumber,
            client.CustomerNumber,
            client.CompanyCode,
            client.CreatedOn);
    }

    public async Task<UpdateClientResponse> UpdateClientAsync(
        Guid clientId,
        UpdateClientRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating client {ClientId}", clientId);

        var client = await _context.Clients
            .Where(c => c.IsActive)
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
        {
            throw new InvalidOperationException($"Client {clientId} not found or is inactive.");
        }

        client.AccountName = request.AccountName;
        client.CompanyNumber = request.CompanyNumber;
        client.CustomerNumber = request.CustomerNumber;
        client.CompanyCode = request.CompanyCode;
        client.ModifiedOn = DateTime.UtcNow;
        client.ModifiedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated client {ClientId}", clientId);

        return new UpdateClientResponse(
            client.Id,
            client.AccountName,
            client.CompanyNumber,
            client.CustomerNumber,
            client.CompanyCode,
            client.CreatedOn);
    }

    public async Task<GetClientByIdResponse?> GetClientByIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients
            .Where(c => c.IsActive)
            .Where(c => c.Id == clientId)
            .Select(c => new GetClientByIdResponse(
                c.Id,
                c.AccountName,
                c.CompanyNumber,
                c.CustomerNumber,
                c.CompanyCode,
                c.CreatedOn,
                c.CreatedBy,
                c.ModifiedOn,
                c.ModifiedBy))
            .FirstOrDefaultAsync(cancellationToken);

        return client;
    }

    public async Task<GetClientsResponse> GetClientsAsync(
        CancellationToken cancellationToken = default)
    {
        var clients = await _context.Clients
            .Where(c => c.IsActive)
            .OrderBy(c => c.AccountName)
            .Select(c => new ClientSummary(
                c.Id,
                c.AccountName,
                c.CompanyNumber,
                c.CustomerNumber,
                c.CompanyCode,
                c.CreatedOn,
                c.CreatedBy))
            .ToListAsync(cancellationToken);

        return new GetClientsResponse(clients);
    }

    public async Task DeleteClientAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting client {ClientId}", clientId);

        var client = await _context.Clients.FindAsync([clientId], cancellationToken);

        if (client == null)
        {
            throw new InvalidOperationException($"Client {clientId} not found.");
        }

        client.IsActive = false;
        client.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted client {ClientId}", clientId);
    }
}
