using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class ProductsEndpoints
{
    public static void MapProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // GET all products
        group.MapGet("/", async (AdminDbContext db, string? search, string? status) =>
        {
            var query = db.Products
                .Include(p => p.ProductTemplates)
                .Include(p => p.ProductConfigurationQuestions)
                    .ThenInclude(pcq => pcq.ConfigurationQuestion)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.OrderBy(p => p.Name).ToListAsync();
        });

        // GET product by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var product = await db.Products
                .Include(p => p.ProductTemplates.Where(pt => pt.IsActive))
                .Include(p => p.ProductConfigurationQuestions.OrderBy(pcq => pcq.DisplayOrder))
                    .ThenInclude(pcq => pcq.ConfigurationQuestion)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        // POST create product
        group.MapPost("/", async (Product product, AdminDbContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{product.Id}", product);
        });

        // PUT update product
        group.MapPut("/{id}", async (int id, Product updatedProduct, AdminDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Rules = updatedProduct.Rules;
            product.Status = updatedProduct.Status;
            product.Version = updatedProduct.Version;
            product.IsActive = updatedProduct.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(product);
        });

        // DELETE product (soft delete)
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
