using Microsoft.AspNetCore.Http.HttpResults;

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

    public static async Task<Results<Ok<GetClientsResponse>, BadRequest<string>>> HandleAsync(
        string? query,
        IClientService clientService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await clientService.GetClientsAsync(cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var pattern = query.Trim().ToLower();
                var filtered = response.Clients
                    .Where(c => c.AccountName.ToLower().Contains(pattern))
                    .ToList();
                return TypedResults.Ok(new GetClientsResponse(filtered));
            }

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
