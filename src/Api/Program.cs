using Api.Data;
using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Modules;
using Api.Features.Clients;
using Api.Features.ConfigurationQuestions;
using Api.Features.Products;
using Api.Features.ProductTemplates;
using Api.Features.QuestionBank;
using Api.Features.MetricGroups;
using Api.Features.Projects;
using Api.Features.ProjectQuestionnaires;
using Api.Features.Seed;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<ApplicationDbContext>("studydb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

var api = app.MapGroup("/api");

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    api.MapSeedDataEndpoint();
}

// Clients
api.MapCreateClientEndpoint();
api.MapGetClientsEndpoint();
api.MapGetClientByIdEndpoint();
api.MapUpdateClientEndpoint();
api.MapDeleteClientEndpoint();

// Commissioning Markets
api.MapCreateCommissioningMarketEndpoint();
api.MapGetCommissioningMarketsEndpoint();
api.MapGetCommissioningMarketByIdEndpoint();
api.MapUpdateCommissioningMarketEndpoint();
api.MapDeleteCommissioningMarketEndpoint();

// Configuration Answers
api.MapCreateConfigurationAnswerEndpoint();
api.MapGetConfigurationAnswersEndpoint();
api.MapGetConfigurationAnswerByIdEndpoint();
api.MapUpdateConfigurationAnswerEndpoint();
api.MapDeleteConfigurationAnswerEndpoint();

// Configuration Questions
api.MapCreateConfigurationQuestionEndpoint();
api.MapGetConfigurationQuestionsEndpoint();
api.MapGetConfigurationQuestionByIdEndpoint();
api.MapUpdateConfigurationQuestionEndpoint();
api.MapDeleteConfigurationQuestionEndpoint();

// Dependency Rules
api.MapCreateDependencyRuleEndpoint();
api.MapGetDependencyRulesEndpoint();
api.MapGetDependencyRuleByIdEndpoint();
api.MapUpdateDependencyRuleEndpoint();
api.MapDeleteDependencyRuleEndpoint();

// Fieldwork Markets
api.MapCreateFieldworkMarketEndpoint();
api.MapGetFieldworkMarketsEndpoint();
api.MapGetFieldworkMarketByIdEndpoint();
api.MapUpdateFieldworkMarketEndpoint();
api.MapDeleteFieldworkMarketEndpoint();

// Metric Groups
api.MapCreateMetricGroupEndpoint();
api.MapGetMetricGroupsEndpoint();
api.MapGetMetricGroupByIdEndpoint();
api.MapUpdateMetricGroupEndpoint();
api.MapDeleteMetricGroupEndpoint();

// Modules
api.MapCreateModuleEndpoint();
api.MapGetModulesEndpoint();
api.MapGetModuleByIdEndpoint();
api.MapUpdateModuleEndpoint();
api.MapDeleteModuleEndpoint();

// Module Questions
api.MapCreateModuleQuestionEndpoint();
api.MapGetModuleQuestionsEndpoint();
api.MapGetModuleQuestionByIdEndpoint();
api.MapUpdateModuleQuestionEndpoint();
api.MapDeleteModuleQuestionEndpoint();

// Products
api.MapCreateProductEndpoint();
api.MapGetProductsEndpoint();
api.MapGetProductByIdEndpoint();
api.MapUpdateProductEndpoint();
api.MapDeleteProductEndpoint();

// Product Config Questions
api.MapCreateProductConfigQuestionEndpoint();
api.MapGetProductConfigQuestionByIdEndpoint();
api.MapUpdateProductConfigQuestionEndpoint();
api.MapDeleteProductConfigQuestionEndpoint();

// Product Config Question Display Rules
api.MapCreateProductConfigQuestionDisplayRuleEndpoint();
api.MapGetProductConfigQuestionDisplayRulesEndpoint();
api.MapGetProductConfigQuestionDisplayRuleByIdEndpoint();
api.MapUpdateProductConfigQuestionDisplayRuleEndpoint();
api.MapDeleteProductConfigQuestionDisplayRuleEndpoint();

// Product Templates
api.MapCreateProductTemplateEndpoint();
api.MapGetProductTemplatesEndpoint();
api.MapGetProductTemplateByIdEndpoint();
api.MapUpdateProductTemplateEndpoint();
api.MapDeleteProductTemplateEndpoint();

// Product Template Lines
api.MapCreateProductTemplateLineEndpoint();
api.MapGetProductTemplateLinesEndpoint();
api.MapGetProductTemplateLineByIdEndpoint();
api.MapUpdateProductTemplateLineEndpoint();
api.MapDeleteProductTemplateLineEndpoint();

// Question Answers
api.MapCreateQuestionAnswerEndpoint();
api.MapUpdateQuestionAnswerEndpoint();
api.MapDeleteQuestionAnswerEndpoint();

// Question Bank Items
api.MapCreateQuestionBankItemEndpoint();
api.MapGetQuestionBankItemsEndpoint();
api.MapGetQuestionBankItemByIdEndpoint();
api.MapUpdateQuestionBankItemEndpoint();
api.MapDeleteQuestionBankItemEndpoint();

// Tags
api.MapCreateTagEndpoint();
api.MapGetTagsEndpoint();
api.MapGetTagByIdEndpoint();
api.MapUpdateTagEndpoint();
api.MapDeleteTagEndpoint();

// Projects
api.MapCreateProjectEndpoint();
api.MapGetProjectsEndpoint();
api.MapGetProjectByIdEndpoint();
api.MapUpdateProjectEndpoint();
api.MapDeleteProjectEndpoint();

// Project Questionnaires
api.MapAddProjectQuestionnaireEndpoint();
api.MapGetProjectQuestionnairesEndpoint();
api.MapUpdateProjectQuestionnairesSortOrderEndpoint();
api.MapDeleteProjectQuestionnaireEndpoint();

app.MapDefaultEndpoints();
app.UseFileServer();
app.Run();

public partial class Program { }
