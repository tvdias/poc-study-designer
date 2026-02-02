using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class ModulesEndpoints
{
    public static void MapModulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/modules").WithTags("Modules");

        // GET all modules
        group.MapGet("/", async (AdminDbContext db, string? search, string? status) =>
        {
            var query = db.Modules
                .Include(m => m.ModuleQuestions)
                    .ThenInclude(mq => mq.Question)
                .Where(m => m.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => 
                    m.VariableName.Contains(search) || 
                    m.Label.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.Status == status);
            }

            return await query.OrderBy(m => m.VariableName).ToListAsync();
        });

        // GET module by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var module = await db.Modules
                .Include(m => m.ModuleQuestions.OrderBy(mq => mq.DisplayOrder))
                    .ThenInclude(mq => mq.Question)
                .Include(m => m.ParentModule)
                .Include(m => m.ChildModules.Where(c => c.IsActive))
                .FirstOrDefaultAsync(m => m.Id == id);

            return module is not null ? Results.Ok(module) : Results.NotFound();
        });

        // POST create module
        group.MapPost("/", async (Module module, AdminDbContext db) =>
        {
            db.Modules.Add(module);
            await db.SaveChangesAsync();
            return Results.Created($"/api/modules/{module.Id}", module);
        });

        // PUT update module
        group.MapPut("/{id}", async (int id, Module updatedModule, AdminDbContext db) =>
        {
            var module = await db.Modules.FindAsync(id);
            if (module is null) return Results.NotFound();

            module.VariableName = updatedModule.VariableName;
            module.Label = updatedModule.Label;
            module.Description = updatedModule.Description;
            module.Instructions = updatedModule.Instructions;
            module.ParentModuleId = updatedModule.ParentModuleId;
            module.Status = updatedModule.Status;
            module.Version = updatedModule.Version;
            module.IsActive = updatedModule.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(module);
        });

        // DELETE module (soft delete)
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var module = await db.Modules.FindAsync(id);
            if (module is null) return Results.NotFound();

            module.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // GET module questions
        group.MapGet("/{id}/questions", async (int id, AdminDbContext db) =>
        {
            return await db.ModuleQuestions
                .Include(mq => mq.Question)
                .Where(mq => mq.ModuleId == id && mq.Question.IsActive)
                .OrderBy(mq => mq.DisplayOrder)
                .ToListAsync();
        });

        // POST add question to module
        group.MapPost("/{id}/questions/{questionId}", async (int id, int questionId, int displayOrder, AdminDbContext db) =>
        {
            var module = await db.Modules.FindAsync(id);
            if (module is null) return Results.NotFound("Module not found");

            var question = await db.Questions.FindAsync(questionId);
            if (question is null) return Results.NotFound("Question not found");

            var moduleQuestion = new ModuleQuestion
            {
                ModuleId = id,
                QuestionId = questionId,
                DisplayOrder = displayOrder
            };

            db.ModuleQuestions.Add(moduleQuestion);
            await db.SaveChangesAsync();
            return Results.Ok(moduleQuestion);
        });

        // DELETE remove question from module
        group.MapDelete("/{id}/questions/{questionId}", async (int id, int questionId, AdminDbContext db) =>
        {
            var moduleQuestion = await db.ModuleQuestions
                .FirstOrDefaultAsync(mq => mq.ModuleId == id && mq.QuestionId == questionId);

            if (moduleQuestion is null) return Results.NotFound();

            db.ModuleQuestions.Remove(moduleQuestion);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
