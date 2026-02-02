using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class ConfigurationQuestionsEndpoints
{
    public static void MapConfigurationQuestionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/configuration-questions").WithTags("Configuration Questions");

        // GET all configuration questions
        group.MapGet("/", async (AdminDbContext db, string? search, string? status) =>
        {
            var query = db.ConfigurationQuestions
                .Include(cq => cq.Answers)
                .Where(cq => cq.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(cq => cq.Question.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(cq => cq.Status == status);
            }

            return await query.OrderBy(cq => cq.Question).ToListAsync();
        });

        // GET configuration question by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var configQuestion = await db.ConfigurationQuestions
                .Include(cq => cq.Answers.Where(a => a.IsActive))
                .FirstOrDefaultAsync(cq => cq.Id == id);

            return configQuestion is not null ? Results.Ok(configQuestion) : Results.NotFound();
        });

        // POST create configuration question
        group.MapPost("/", async (ConfigurationQuestion configQuestion, AdminDbContext db) =>
        {
            db.ConfigurationQuestions.Add(configQuestion);
            await db.SaveChangesAsync();
            return Results.Created($"/api/configuration-questions/{configQuestion.Id}", configQuestion);
        });

        // PUT update configuration question
        group.MapPut("/{id}", async (int id, ConfigurationQuestion updatedQuestion, AdminDbContext db) =>
        {
            var configQuestion = await db.ConfigurationQuestions.FindAsync(id);
            if (configQuestion is null) return Results.NotFound();

            configQuestion.Question = updatedQuestion.Question;
            configQuestion.Rule = updatedQuestion.Rule;
            configQuestion.AiPrompt = updatedQuestion.AiPrompt;
            configQuestion.DependencyRules = updatedQuestion.DependencyRules;
            configQuestion.Status = updatedQuestion.Status;
            configQuestion.Version = updatedQuestion.Version;
            configQuestion.IsActive = updatedQuestion.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(configQuestion);
        });

        // DELETE configuration question (soft delete)
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var configQuestion = await db.ConfigurationQuestions.FindAsync(id);
            if (configQuestion is null) return Results.NotFound();

            configQuestion.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // GET configuration question answers
        group.MapGet("/{id}/answers", async (int id, AdminDbContext db) =>
        {
            return await db.ConfigurationQuestionAnswers
                .Where(a => a.ConfigurationQuestionId == id && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
        });

        // POST add answer to configuration question
        group.MapPost("/{id}/answers", async (int id, ConfigurationQuestionAnswer answer, AdminDbContext db) =>
        {
            var configQuestion = await db.ConfigurationQuestions.FindAsync(id);
            if (configQuestion is null) return Results.NotFound();

            answer.ConfigurationQuestionId = id;
            db.ConfigurationQuestionAnswers.Add(answer);
            await db.SaveChangesAsync();
            return Results.Created($"/api/configuration-questions/{id}/answers/{answer.Id}", answer);
        });
    }
}
