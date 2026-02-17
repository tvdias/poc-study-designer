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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Seed;

public static class SeedDataEndpoint
{
    public static void MapSeedDataEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/seed", async ([FromServices] ApplicationDbContext db) =>
        {
            if (await db.Clients.AnyAsync())
            {
                return Results.Ok(new { Message = "Database already seeded" });
            }

            // 1. Tags
            var now = DateTime.UtcNow;
            const string seedUser = "SeedData";
            var tags = new[]
            {
                new Tag { Name = "Global", CreatedOn = now, CreatedBy = seedUser },
                new Tag { Name = "Retail", CreatedOn = now, CreatedBy = seedUser },
                new Tag { Name = "Technology", CreatedOn = now, CreatedBy = seedUser },
                new Tag { Name = "Healthcare", CreatedOn = now, CreatedBy = seedUser },
                new Tag { Name = "Finance", CreatedOn = now, CreatedBy = seedUser }
            };
            db.Tags.AddRange(tags);

            // 2. Commissioning Markets
            var commMarkets = new[]
            {
                new CommissioningMarket { Name = "United Kingdom", IsoCode = "GB", CreatedOn = now, CreatedBy = seedUser },
                new CommissioningMarket { Name = "United States", IsoCode = "US", CreatedOn = now, CreatedBy = seedUser },
                new CommissioningMarket { Name = "Germany", IsoCode = "DE", CreatedOn = now, CreatedBy = seedUser },
                new CommissioningMarket { Name = "France", IsoCode = "FR", CreatedOn = now, CreatedBy = seedUser }
            };
            db.CommissioningMarkets.AddRange(commMarkets);

            // 3. Fieldwork Markets
            var fieldMarkets = new[]
            {
                new FieldworkMarket { Name = "United Kingdom", IsoCode = "GB", CreatedOn = now, CreatedBy = seedUser },
                new FieldworkMarket { Name = "United States", IsoCode = "US", CreatedOn = now, CreatedBy = seedUser },
                new FieldworkMarket { Name = "Germany", IsoCode = "DE", CreatedOn = now, CreatedBy = seedUser },
                new FieldworkMarket { Name = "China", IsoCode = "CN", CreatedOn = now, CreatedBy = seedUser },
                new FieldworkMarket { Name = "Japan", IsoCode = "JP", CreatedOn = now, CreatedBy = seedUser }
            };
            db.FieldworkMarkets.AddRange(fieldMarkets);

            // 4. Metric Groups
            var metricGroups = new[]
            {
                new MetricGroup { Name = "Brand Awareness", CreatedOn = now, CreatedBy = seedUser },
                new MetricGroup { Name = "Ad Recall", CreatedOn = now, CreatedBy = seedUser },
                new MetricGroup { Name = "Purchase Intent", CreatedOn = now, CreatedBy = seedUser },
                new MetricGroup { Name = "NPS", CreatedOn = now, CreatedBy = seedUser }
            };
            db.MetricGroups.AddRange(metricGroups);
            await db.SaveChangesAsync(); // Save to get IDs for relationships

            // 5. Question Bank Items
            var qGrid = new QuestionBankItem
            {
                VariableName = "Q_BRAND_AWARENESS",
                QuestionText = "Which of the following brands have you heard of?",
                QuestionTitle = "Brand Awareness",
                QuestionType = "MultiChoice",
                Classification = "Standard",
                Status = "Active",
                Methodology = "Quantitative",
                Version = 1,
                MetricGroupId = metricGroups[0].Id,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            
            var qOpen = new QuestionBankItem
            {
                VariableName = "Q_NPS_REASON",
                QuestionText = "What is the primary reason for your score?",
                QuestionTitle = "NPS Reason",
                QuestionType = "OpenText",
                Classification = "Standard",
                Status = "Active",
                Methodology = "Quantitative",
                Version = 1,
                MetricGroupId = metricGroups[3].Id,
                CreatedOn = now,
                CreatedBy = seedUser
            };

            db.QuestionBankItems.AddRange(qGrid, qOpen);
            await db.SaveChangesAsync();

            // Answers for Q_BRAND_AWARENESS
            var answers = new[]
            {
                new QuestionAnswer { Id = Guid.NewGuid(), QuestionBankItemId = qGrid.Id, AnswerText = "Brand A", AnswerCode = "1", AnswerLocation = "Row", CreatedOn = now, CreatedBy = seedUser },
                new QuestionAnswer { Id = Guid.NewGuid(), QuestionBankItemId = qGrid.Id, AnswerText = "Brand B", AnswerCode = "2", AnswerLocation = "Row", CreatedOn = now, CreatedBy = seedUser },
                new QuestionAnswer { Id = Guid.NewGuid(), QuestionBankItemId = qGrid.Id, AnswerText = "Brand C", AnswerCode = "3", AnswerLocation = "Row", CreatedOn = now, CreatedBy = seedUser },
                new QuestionAnswer { Id = Guid.NewGuid(), QuestionBankItemId = qGrid.Id, AnswerText = "None of the above", AnswerCode = "99", AnswerLocation = "Row", CreatedOn = now, CreatedBy = seedUser }
            };
            db.QuestionAnswers.AddRange(answers);

            // 6. Clients
            var clientA = new Client 
            { 
                AccountName = "Acme Corp", 
                CompanyNumber = "12345678", 
                CustomerNumber = "CUST001", 
                CompanyCode = "ACME",
                CreatedOn = now,
                CreatedBy = seedUser
            };
            var clientB = new Client 
            { 
                AccountName = "Globex Corporation", 
                CompanyNumber = "87654321", 
                CustomerNumber = "CUST002", 
                CompanyCode = "GLOBEX",
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.Clients.AddRange(clientA, clientB);

            // 7. Configuration Questions
            var configQ1 = new ConfigurationQuestion
            {
                Question = "Is this a tracking study?",
                RuleType = Api.Features.ConfigurationQuestions.RuleType.SingleCoded,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.ConfigurationQuestions.Add(configQ1);
            await db.SaveChangesAsync();

            var configA1 = new ConfigurationAnswer { Id = Guid.NewGuid(), Name = "Yes", ConfigurationQuestionId = configQ1.Id, CreatedOn = now, CreatedBy = seedUser };
            var configA2 = new ConfigurationAnswer { Id = Guid.NewGuid(), Name = "No", ConfigurationQuestionId = configQ1.Id, CreatedOn = now, CreatedBy = seedUser };
            db.ConfigurationAnswers.AddRange(configA1, configA2);

            // 8. Products
            var product = new Product
            {
                Name = "Brand Tracker Standard",
                Description = "Standard monthly brand tracking survey",
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Product Config Questions
            var prodConfigQ = new ProductConfigQuestion
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ConfigurationQuestionId = configQ1.Id,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.ProductConfigQuestions.Add(prodConfigQ);

            // 9. Modules (Hierarchy)
            var section1 = new Module
            {
                VariableName = "SEC_DEMO",
                Label = "Demographics",
                Description = "Standard demographic questions",
                Instructions = "Ask all respondents",
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.Modules.Add(section1);
            await db.SaveChangesAsync();

            var subModule = new Module
            {
                VariableName = "SUB_AGE_GENDER",
                Label = "Age and Gender",
                ParentModuleId = section1.Id,
                Description = "Basic demographics",
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.Modules.Add(subModule);
            await db.SaveChangesAsync();

            // Link Question to Module
            var modQ = new ModuleQuestion
            {
                Id = Guid.NewGuid(),
                ModuleId = subModule.Id,
                QuestionBankItemId = qGrid.Id,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.ModuleQuestions.Add(modQ);

            // 10. Product Templates
            var template = new ProductTemplate
            {
                Name = "2024 Base Template",
                Version = 1,
                ProductId = product.Id,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            db.ProductTemplates.Add(template);
            await db.SaveChangesAsync();

            // Template Lines
            db.ProductTemplateLines.Add(new ProductTemplateLine
            {
                Id = Guid.NewGuid(),
                ProductTemplateId = template.Id,
                Name = "Demographics Section",
                Type = "Module",
                ModuleId = section1.Id,
                CreatedOn = now,
                CreatedBy = seedUser
            });

            // 11. Projects
            var project1 = new Project
            {
                Name = "Acme Q1 Tracker",
                Description = "Q1 2024 Brand Tracker for Acme Corp",
                ClientId = clientA.Id,
                ProductId = product.Id,
                Owner = "john.doe@example.com",
                Status = Api.Features.Projects.ProjectStatus.OnHold,
                CreatedOn = now,
                CreatedBy = seedUser
            };
            
            var project2 = new Project
            {
                Name = "Globex Annual Review",
                Description = "Annual brand review study",
                ClientId = clientB.Id,
                ProductId = product.Id,
                Owner = "jane.smith@example.com",
                Status = Api.Features.Projects.ProjectStatus.Active,
                CreatedOn = now,
                CreatedBy = seedUser
            };

            db.Projects.AddRange(project1, project2);

            await db.SaveChangesAsync();

            return Results.Ok(new { Message = "Database seeded successfully", Counts = new { 
                Clients = 2, 
                Projects = 2, 
                Products = 1, 
                Tags = tags.Length,
                Questions = 2
            }});
        })
        .WithName("SeedDatabase")
        .WithTags("Utilities")
        .WithDescription("Seeds the database with development data")
        .WithOpenApi();
    }
}
