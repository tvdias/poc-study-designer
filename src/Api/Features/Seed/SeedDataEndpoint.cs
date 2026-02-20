using Api.Data;
using Api.Features.Clients;
using Api.Features.CommissioningMarkets;
using Api.Features.ConfigurationQuestions;
using Api.Features.FieldworkMarkets;
using Api.Features.MetricGroups;
using Api.Features.Modules;
using Api.Features.Products;
using Api.Features.ProductTemplates;
using Api.Features.Projects;
using Api.Features.QuestionBank;
using Api.Features.Tags;
using Api.Features.Studies;
using Api.Features.ManagedLists;
using Api.Features.QuestionnaireLines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api.Features.Seed;

public static class SeedDataEndpoint
{
    public static void MapSeedDataEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/seed", async ([FromServices] ApplicationDbContext db) =>
        {
            db.Database.SetCommandTimeout(300); // allow more time for heavy inserts

            if (await db.Projects.AnyAsync())
            {
                return Results.Ok(new { Message = "Database already seeded" });
            }

            var now = DateTime.UtcNow;
            const string seedUser = "SeedData";

            // 1. Tags
            var tags = new[] { new Tag { Name = "Global", CreatedOn = now, CreatedBy = seedUser } };
            db.Tags.AddRange(tags);

            // 2. Commissioning Markets
            var commMarkets = new[] { new CommissioningMarket { Name = "UK", IsoCode = "GB", CreatedOn = now, CreatedBy = seedUser } };
            db.CommissioningMarkets.AddRange(commMarkets);

            // 3. Fieldwork Markets
            var fieldMarkets = new[] { new FieldworkMarket { Name = "UK", IsoCode = "GB", CreatedOn = now, CreatedBy = seedUser } };
            db.FieldworkMarkets.AddRange(fieldMarkets);

            // 4. Metric Groups
            var metricGroup = new MetricGroup { Name = "General", CreatedOn = now, CreatedBy = seedUser };
            db.MetricGroups.Add(metricGroup);
            await db.SaveChangesAsync();

            // 5. Clients
            var clientA = new Client { AccountName = "Acme Corp", CreatedOn = now, CreatedBy = seedUser };
            db.Clients.Add(clientA);

            // 6. Products
            var product = new Product { Name = "Standard", CreatedOn = now, CreatedBy = seedUser };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            // 7. Question Bank
            var qBankItems = new List<QuestionBankItem>();
            for (int i = 1; i <= 300; i++)
            {
                var q = new QuestionBankItem
                {
                    Id = Guid.NewGuid(),
                    VariableName = $"Q_BANK_{i}",
                    QuestionText = $"Question text {i}",
                    QuestionType = "MultiChoice",
                    Version = 1,
                    MetricGroupId = metricGroup.Id,
                    CreatedOn = now, CreatedBy = seedUser
                };
                qBankItems.Add(q);
            }
            db.QuestionBankItems.AddRange(qBankItems);
            
            var answers = new List<QuestionAnswer>();
            foreach (var q in qBankItems)
            {
                for (int a = 1; a <= 10; a++)
                {
                    answers.Add(new QuestionAnswer
                    {
                        Id = Guid.NewGuid(),
                        QuestionBankItemId = q.Id,
                        AnswerText = $"Answer {a}",
                        AnswerCode = a.ToString()
                    });
                }
            }
            db.QuestionAnswers.AddRange(answers);
            await db.SaveChangesAsync();

            // 8. Modules
            var modules = new List<Module>();
            int qIndex = 0;
            for (int i = 0; i < 50; i++)
            {
                int qCount = 5;
                if (i == 47) qCount = 10; // medium
                else if (i == 48) qCount = 20; // big
                else if (i == 49) qCount = 50; // bigger

                var mod = new Module
                {
                    Id = Guid.NewGuid(),
                    VariableName = $"MOD_{i}",
                    Label = $"Module {i}",
                    CreatedOn = now, CreatedBy = seedUser
                };
                db.Modules.Add(mod);
                
                for (int j = 0; j < qCount; j++)
                {
                    db.ModuleQuestions.Add(new ModuleQuestion
                    {
                        Id = Guid.NewGuid(),
                        ModuleId = mod.Id,
                        QuestionBankItemId = qBankItems[qIndex % 300].Id
                    });
                    qIndex++;
                }
            }
            await db.SaveChangesAsync();

            // 9. Projects
            var random = new Random(42);
            db.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int batch = 0; batch < 10; batch++)
            {
                for (int p = 0; p < 100; p++)
                {
                    int projectIndex = batch * 100 + p;
                    int numQ = 50, numS = 10, numML = 10;
                    
                    if (projectIndex == 0) { numQ = 100; numS = 20; numML = 15; } // medium
                    else if (projectIndex == 1) { numQ = 150; numS = 30; numML = 20; } // big
                    else if (projectIndex == 2) { numQ = 250; numS = 50; numML = 30; } // bigger

                    var project = new Project
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Project {projectIndex + 1}",
                        ClientId = clientA.Id,
                        ProductId = product.Id,
                        CommissioningMarketId = commMarkets[0].Id,
                        Owner = "seed@user.com",
                        CreatedOn = now, CreatedBy = seedUser
                    };
                    db.Projects.Add(project);

                    var projectQLineIds = new List<Guid>();
                    for (int q = 0; q < numQ; q++)
                    {
                        var qlId = Guid.NewGuid();
                        db.QuestionnaireLines.Add(new QuestionnaireLine
                        {
                            Id = qlId,
                            ProjectId = project.Id,
                            QuestionBankItemId = qBankItems[q % 300].Id,
                            VariableName = $"V{projectIndex}_{q}"
                        });
                        projectQLineIds.Add(qlId);
                    }

                    var studyIds = new List<Guid>();
                    var studyQLines = new List<Guid>();
                    for (int s = 0; s < numS; s++)
                    {
                        var stdId = Guid.NewGuid();
                        db.Studies.Add(new Study
                        {
                            Id = stdId,
                            Name = $"Study {s + 1}",
                            Version = 1,
                            ProjectId = project.Id,
                            Category = "Standard",
                            FieldworkMarketId = fieldMarkets[0].Id,
                            MaconomyJobNumber = $"MAC-{projectIndex}-{s}",
                            ProjectOperationsUrl = "https://ops.example.com",
                            MasterStudyId = stdId
                        });
                        studyIds.Add(stdId);

                        var sqlId = Guid.NewGuid();
                        db.StudyQuestionnaireLines.Add(new StudyQuestionnaireLine
                        {
                            Id = sqlId,
                            StudyId = stdId,
                            VariableName = $"V{s}",
                            QuestionBankItemId = qBankItems[0].Id
                        });
                        studyQLines.Add(sqlId);
                    }

                    for (int ml = 0; ml < numML; ml++)
                    {
                        var mListId = Guid.NewGuid();
                        db.ManagedLists.Add(new ManagedList
                        {
                            Id = mListId,
                            ProjectId = project.Id,
                            Name = $"ML {ml + 1}"
                        });

                        for (int mli = 0; mli < 20; mli++)
                        {
                            db.ManagedListItems.Add(new ManagedListItem
                            {
                                Id = Guid.NewGuid(),
                                ManagedListId = mListId,
                                Code = $"C{mli}",
                                Label = $"Item {mli}"
                            });
                        }

                        if (projectQLineIds.Any() && random.NextDouble() > 0.5)
                        {
                            db.QuestionManagedLists.Add(new QuestionManagedList
                            {
                                Id = Guid.NewGuid(),
                                QuestionnaireLineId = projectQLineIds[random.Next(projectQLineIds.Count)],
                                ManagedListId = mListId
                            });
                        }

                        if (studyQLines.Any() && studyIds.Any() && random.NextDouble() > 0.5)
                        {
                            db.StudyManagedListAssignments.Add(new StudyManagedListAssignment
                            {
                                Id = Guid.NewGuid(),
                                StudyId = studyIds[random.Next(studyIds.Count)],
                                StudyQuestionnaireLineId = studyQLines[random.Next(studyQLines.Count)],
                                ManagedListId = mListId
                            });
                        }
                    }
                }
                await db.SaveChangesAsync();
            }

            db.ChangeTracker.AutoDetectChangesEnabled = true;

            return Results.Ok(new { Message = "Database seeded successfully" });
        })
        .WithName("SeedDatabase")
        .WithTags("Utilities")
        .WithDescription("Seeds the database with high volume development data")
        .WithOpenApi();
    }
}
