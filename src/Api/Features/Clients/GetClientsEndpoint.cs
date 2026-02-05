using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Clients;

public static class GetClientsEndpoint
{
    public static void MapGetClientsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/clients", HandleAsync)
            .WithName("GetClients")
            .WithSummary("Get Clients")
            .WithTags("Clients");
    }

    public static async Task<List<GetClientsResponse>> HandleAsync(
        string? query,
        ApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var clientsQuery = db.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            clientsQuery = clientsQuery.Where(c => EF.Functions.ILike(c.AccountName, pattern));
        }

        return await clientsQuery
            .Select(c => new GetClientsResponse(c.Id, c.AccountName, c.CompanyNumber, c.CustomerNumber, c.CompanyCode, c.IsActive))
            .ToListAsync(cancellationToken);
    }
}
