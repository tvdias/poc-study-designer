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
            var tags = new[]
            {
                new Tag { Name = "Global" },
                new Tag { Name = "Retail" },
                new Tag { Name = "Technology" },
                new Tag { Name = "Healthcare" },
                new Tag { Name = "Finance" }
            };
            db.Tags.AddRange(tags);

            // 2. Commissioning Markets
            var commMarkets = new[]
            {
                new CommissioningMarket { Name = "United Kingdom", IsoCode = "GB" },
                new CommissioningMarket { Name = "United States", IsoCode = "US" },
                new CommissioningMarket { Name = "Germany", IsoCode = "DE" },
                new CommissioningMarket { Name = "France", IsoCode = "FR" }
            };
            db.CommissioningMarkets.AddRange(commMarkets);

            // 3. Fieldwork Markets
            var fieldMarkets = new[]
            {
                new FieldworkMarket { Name = "United Kingdom", IsoCode = "GB" },
                new FieldworkMarket { Name = "United States", IsoCode = "US" },
                new FieldworkMarket { Name = "Germany", IsoCode = "DE" },
                new FieldworkMarket { Name = "China", IsoCode = "CN" },
                new FieldworkMarket { Name = "Japan", IsoCode = "JP" }
            };
            db.FieldworkMarkets.AddRange(fieldMarkets);

            // 4. Metric Groups
            var metricGroups = new[]
            {
                new MetricGroup { Name = "Brand Awareness" },
                new MetricGroup { Name = "Ad Recall" },
                new MetricGroup { Name = "Purchase Intent" },
                new MetricGroup { Name = "NPS" }
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
                MetricGroupId = metricGroups[0].Id
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
                MetricGroupId = metricGroups[3].Id
            };

            db.QuestionBankItems.AddRange(qGrid, qOpen);
            await db.SaveChangesAsync();

            // Answers for Q_BRAND_AWARENESS
            var answers = new[]
            {
                new QuestionAnswer { QuestionBankItemId = qGrid.Id, AnswerText = "Brand A", AnswerCode = "1", AnswerLocation = "Row" },
                new QuestionAnswer { QuestionBankItemId = qGrid.Id, AnswerText = "Brand B", AnswerCode = "2", AnswerLocation = "Row" },
                new QuestionAnswer { QuestionBankItemId = qGrid.Id, AnswerText = "Brand C", AnswerCode = "3", AnswerLocation = "Row" },
                new QuestionAnswer { QuestionBankItemId = qGrid.Id, AnswerText = "None of the above", AnswerCode = "99", AnswerLocation = "Row" }
            };
            db.QuestionAnswers.AddRange(answers);

            // 6. Clients
            var clientA = new Client 
            { 
                AccountName = "Acme Corp", 
                CompanyNumber = "12345678", 
                CustomerNumber = "CUST001", 
                CompanyCode = "ACME" 
            };
            var clientB = new Client 
            { 
                AccountName = "Globex Corporation", 
                CompanyNumber = "87654321", 
                CustomerNumber = "CUST002", 
                CompanyCode = "GLOBEX" 
            };
            db.Clients.AddRange(clientA, clientB);

            // 7. Configuration Questions
            var configQ1 = new ConfigurationQuestion
            {
                Question = "Is this a tracking study?",
                RuleType = Api.Features.ConfigurationQuestions.RuleType.SingleCoded
            };
            db.ConfigurationQuestions.Add(configQ1);
            await db.SaveChangesAsync();

            var configA1 = new ConfigurationAnswer { Name = "Yes", ConfigurationQuestionId = configQ1.Id };
            var configA2 = new ConfigurationAnswer { Name = "No", ConfigurationQuestionId = configQ1.Id };
            db.ConfigurationAnswers.AddRange(configA1, configA2);

            // 8. Products
            var product = new Product
            {
                Name = "Brand Tracker Standard",
                Description = "Standard monthly brand tracking survey"
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Product Config Questions
            var prodConfigQ = new ProductConfigQuestion
            {
                ProductId = product.Id,
                ConfigurationQuestionId = configQ1.Id
            };
            db.ProductConfigQuestions.Add(prodConfigQ);

            // 9. Modules (Hierarchy)
            var section1 = new Module
            {
                VariableName = "SEC_DEMO",
                Label = "Demographics",
                Description = "Standard demographic questions",
                Instructions = "Ask all respondents"
            };
            db.Modules.Add(section1);
            await db.SaveChangesAsync();

            var subModule = new Module
            {
                VariableName = "SUB_AGE_GENDER",
                Label = "Age and Gender",
                ParentModuleId = section1.Id,
                Description = "Basic demographics"
            };
            db.Modules.Add(subModule);
            await db.SaveChangesAsync();

            // Link Question to Module
            var modQ = new ModuleQuestion
            {
                ModuleId = subModule.Id,
                QuestionBankItemId = qGrid.Id
            };
            db.ModuleQuestions.Add(modQ);

            // 10. Product Templates
            var template = new ProductTemplate
            {
                Name = "2024 Base Template",
                Version = 1,
                ProductId = product.Id
            };
            db.ProductTemplates.Add(template);
            await db.SaveChangesAsync();

            // Template Lines
            db.ProductTemplateLines.Add(new ProductTemplateLine
            {
                ProductTemplateId = template.Id,
                Name = "Demographics Section",
                Type = "Module",
                ModuleId = section1.Id
            });

            // 11. Projects
            var project1 = new Project
            {
                Name = "Acme Q1 Tracker",
                Description = "Q1 2024 Brand Tracker for Acme Corp",
                ClientId = clientA.Id,
                ProductId = product.Id,
                Owner = "john.doe@example.com",
                Status = Api.Features.Projects.ProjectStatus.OnHold
            };
            
            var project2 = new Project
            {
                Name = "Globex Annual Review",
                Description = "Annual brand review study",
                ClientId = clientB.Id,
                ProductId = product.Id,
                Owner = "jane.smith@example.com",
                Status = Api.Features.Projects.ProjectStatus.Active
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
