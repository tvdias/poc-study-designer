using Api.Data;
using Api.Features.Clients;
using Api.Features.CommissioningMarkets;
using Api.Features.ConfigurationQuestions;
using Api.Features.FieldworkMarkets;
using Api.Features.ManagedLists;
using Api.Features.MetricGroups;
using Api.Features.Modules;
using Api.Features.ProductTemplates;
using Api.Features.Products;
using Api.Features.Projects;
using Api.Features.QuestionBank;
using Api.Features.QuestionnaireLines;
using Api.Features.Seed;
using Api.Features.Studies;
using Api.Features.Tags;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register feature services
builder.Services.AddClientsFeature();
builder.Services.AddProjectsFeature();
builder.Services.AddStudiesFeature();
builder.Services.AddManagedListsFeature();
builder.Services.AddTagsFeature();
builder.Services.AddQuestionBankFeature();
builder.Services.AddQuestionnaireLinesFeature();
builder.Services.AddScoped<IAutoAssociationService, AutoAssociationService>();

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

// Map feature endpoints
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    api.MapSeedEndpoints();
}

api.MapClientsEndpoints();
api.MapProjectsEndpoints();
api.MapStudiesEndpoints();
api.MapCommissioningMarketsEndpoints();
api.MapConfigurationQuestionsEndpoints();
api.MapFieldworkMarketsEndpoints();
api.MapMetricGroupsEndpoints();
api.MapModulesEndpoints();
api.MapProductsEndpoints();
api.MapProductTemplatesEndpoints();
api.MapQuestionBankEndpoints();
api.MapQuestionnaireLinesEndpoints();
api.MapManagedListsEndpoints();
api.MapTagsEndpoints();

app.MapDefaultEndpoints();
app.UseFileServer();
app.Run();

public partial class Program { }
