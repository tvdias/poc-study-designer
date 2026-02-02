using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class MarketsEndpoints
{
    public static void MapMarketsEndpoints(this IEndpointRouteBuilder app)
    {
        var commissioningGroup = app.MapGroup("/api/commissioning-markets").WithTags("Commissioning Markets");
        var fieldworkGroup = app.MapGroup("/api/fieldwork-markets").WithTags("Fieldwork Markets");

        // Commissioning Markets endpoints
        commissioningGroup.MapGet("/", async (AdminDbContext db) =>
        {
            return await db.CommissioningMarkets
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        });

        commissioningGroup.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var market = await db.CommissioningMarkets.FindAsync(id);
            return market is not null ? Results.Ok(market) : Results.NotFound();
        });

        commissioningGroup.MapPost("/", async (CommissioningMarket market, AdminDbContext db) =>
        {
            db.CommissioningMarkets.Add(market);
            await db.SaveChangesAsync();
            return Results.Created($"/api/commissioning-markets/{market.Id}", market);
        });

        commissioningGroup.MapPut("/{id}", async (int id, CommissioningMarket updatedMarket, AdminDbContext db) =>
        {
            var market = await db.CommissioningMarkets.FindAsync(id);
            if (market is null) return Results.NotFound();

            market.Name = updatedMarket.Name;
            market.IsoCode = updatedMarket.IsoCode;
            market.IsActive = updatedMarket.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(market);
        });

        commissioningGroup.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var market = await db.CommissioningMarkets.FindAsync(id);
            if (market is null) return Results.NotFound();

            market.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Fieldwork Markets endpoints
        fieldworkGroup.MapGet("/", async (AdminDbContext db) =>
        {
            return await db.FieldworkMarkets
                .Where(m => m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        });

        fieldworkGroup.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var market = await db.FieldworkMarkets.FindAsync(id);
            return market is not null ? Results.Ok(market) : Results.NotFound();
        });

        fieldworkGroup.MapPost("/", async (FieldworkMarket market, AdminDbContext db) =>
        {
            db.FieldworkMarkets.Add(market);
            await db.SaveChangesAsync();
            return Results.Created($"/api/fieldwork-markets/{market.Id}", market);
        });

        fieldworkGroup.MapPut("/{id}", async (int id, FieldworkMarket updatedMarket, AdminDbContext db) =>
        {
            var market = await db.FieldworkMarkets.FindAsync(id);
            if (market is null) return Results.NotFound();

            market.Name = updatedMarket.Name;
            market.IsoCode = updatedMarket.IsoCode;
            market.IsActive = updatedMarket.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(market);
        });

        fieldworkGroup.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var market = await db.FieldworkMarkets.FindAsync(id);
            if (market is null) return Results.NotFound();

            market.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
