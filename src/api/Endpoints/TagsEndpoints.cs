using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class TagsEndpoints
{
    public static void MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags");

        // GET all tags
        group.MapGet("/", async (AdminDbContext db) =>
        {
            return await db.Tags
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        });

        // GET tag by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            return tag is not null ? Results.Ok(tag) : Results.NotFound();
        });

        // POST create tag
        group.MapPost("/", async (Tag tag, AdminDbContext db) =>
        {
            db.Tags.Add(tag);
            await db.SaveChangesAsync();
            return Results.Created($"/api/tags/{tag.Id}", tag);
        });

        // PUT update tag
        group.MapPut("/{id}", async (int id, Tag updatedTag, AdminDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            if (tag is null) return Results.NotFound();

            tag.Name = updatedTag.Name;
            tag.Description = updatedTag.Description;
            tag.IsActive = updatedTag.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(tag);
        });

        // DELETE tag
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var tag = await db.Tags.FindAsync(id);
            if (tag is null) return Results.NotFound();

            tag.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
