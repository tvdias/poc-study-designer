using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class QuestionsEndpoints
{
    public static void MapQuestionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/questions").WithTags("Questions");

        // GET all questions with search and filtering
        group.MapGet("/", async (AdminDbContext db, string? search, string? type, string? status) =>
        {
            var query = db.Questions
                .Include(q => q.Answers)
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Where(q => q.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(q => 
                    q.VariableName.Contains(search) || 
                    q.Title.Contains(search) || 
                    q.Text.Contains(search));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(q => q.Type == type);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(q => q.Status == status);
            }

            return await query.OrderBy(q => q.VariableName).ToListAsync();
        });

        // GET question by id
        group.MapGet("/{id}", async (int id, AdminDbContext db) =>
        {
            var question = await db.Questions
                .Include(q => q.Answers.Where(a => a.IsActive))
                .Include(q => q.QuestionTags)
                    .ThenInclude(qt => qt.Tag)
                .Include(q => q.ParentQuestion)
                .Include(q => q.ChildQuestions.Where(c => c.IsActive))
                .FirstOrDefaultAsync(q => q.Id == id);

            return question is not null ? Results.Ok(question) : Results.NotFound();
        });

        // POST create question
        group.MapPost("/", async (Question question, AdminDbContext db) =>
        {
            db.Questions.Add(question);
            await db.SaveChangesAsync();
            return Results.Created($"/api/questions/{question.Id}", question);
        });

        // PUT update question
        group.MapPut("/{id}", async (int id, Question updatedQuestion, AdminDbContext db) =>
        {
            var question = await db.Questions.FindAsync(id);
            if (question is null) return Results.NotFound();

            question.VariableName = updatedQuestion.VariableName;
            question.Title = updatedQuestion.Title;
            question.Text = updatedQuestion.Text;
            question.Type = updatedQuestion.Type;
            question.Methodology = updatedQuestion.Methodology;
            question.IsStandard = updatedQuestion.IsStandard;
            question.IsDummy = updatedQuestion.IsDummy;
            question.ScriptNotes = updatedQuestion.ScriptNotes;
            question.MetricGroup = updatedQuestion.MetricGroup;
            question.DataQualityTags = updatedQuestion.DataQualityTags;
            question.TableNotes = updatedQuestion.TableNotes;
            question.Scale = updatedQuestion.Scale;
            question.DisplayType = updatedQuestion.DisplayType;
            question.Restrictions = updatedQuestion.Restrictions;
            question.Facets = updatedQuestion.Facets;
            question.ParentQuestionId = updatedQuestion.ParentQuestionId;
            question.IsTranslatable = updatedQuestion.IsTranslatable;
            question.IsHidden = updatedQuestion.IsHidden;
            question.Status = updatedQuestion.Status;
            question.Version = updatedQuestion.Version;
            question.IsActive = updatedQuestion.IsActive;

            await db.SaveChangesAsync();
            return Results.Ok(question);
        });

        // DELETE question (soft delete)
        group.MapDelete("/{id}", async (int id, AdminDbContext db) =>
        {
            var question = await db.Questions.FindAsync(id);
            if (question is null) return Results.NotFound();

            question.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // GET question answers
        group.MapGet("/{id}/answers", async (int id, AdminDbContext db) =>
        {
            return await db.QuestionAnswers
                .Where(a => a.QuestionId == id && a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
        });

        // POST add answer to question
        group.MapPost("/{id}/answers", async (int id, QuestionAnswer answer, AdminDbContext db) =>
        {
            var question = await db.Questions.FindAsync(id);
            if (question is null) return Results.NotFound();

            answer.QuestionId = id;
            db.QuestionAnswers.Add(answer);
            await db.SaveChangesAsync();
            return Results.Created($"/api/questions/{id}/answers/{answer.Id}", answer);
        });
    }
}
