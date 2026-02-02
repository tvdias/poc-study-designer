using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class ClientsEndpoints
{
    public static void MapClientsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clients").WithTags("Clients");

        // GET all clients
        group.MapGet("/", async (AdminDbContext db) =>
        {
            return await db.Clients
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        });

        // GET client by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var client = await db.Clients.FindAsync(id);
            return client is not null ? Results.Ok(client) : Results.NotFound();
        });

        // POST create client
        group.MapPost("/", async (Client client, AdminDbContext db) =>
        {
            db.Clients.Add(client);
            await db.SaveChangesAsync();
            return Results.Created($"/api/clients/{client.Id}", client);
        });

        // PUT update client
        group.MapPut("/{id}", async (int id, Client updatedClient, AdminDbContext db) =>
        {
            var client = await db.Clients.FindAsync(id);
            if (client is null) return Results.NotFound();

            client.Name = updatedClient.Name;
            client.IntegrationProperties = updatedClient.IntegrationProperties;
            client.IsActive = updatedClient.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(client);
        });

        // DELETE client
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var client = await db.Clients.FindAsync(id);
            if (client is null) return Results.NotFound();

            client.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
