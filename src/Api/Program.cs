using Api.Data;
using Api.Features.Tags;
using Api.Features.CommissioningMarkets;
using Api.Features.FieldworkMarkets;
using Api.Features.Modules;
using Api.Features.Clients;
using Api.Features.ConfigurationQuestions;
using Api.Features.Products;
using Api.Features.ProductTemplates;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<ApplicationDbContext>("studydb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseOutputCache();

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

var api = app.MapGroup("/api");
api.MapCreateTagEndpoint();
api.MapGetTagsEndpoint();
api.MapGetTagByIdEndpoint();
api.MapUpdateTagEndpoint();
api.MapDeleteTagEndpoint();
api.MapCreateCommissioningMarketEndpoint();
api.MapGetCommissioningMarketsEndpoint();
api.MapGetCommissioningMarketByIdEndpoint();
api.MapUpdateCommissioningMarketEndpoint();
api.MapDeleteCommissioningMarketEndpoint();
api.MapCreateFieldworkMarketEndpoint();
api.MapGetFieldworkMarketsEndpoint();
api.MapGetFieldworkMarketByIdEndpoint();
api.MapUpdateFieldworkMarketEndpoint();
api.MapDeleteFieldworkMarketEndpoint();
api.MapCreateModuleEndpoint();
api.MapGetModulesEndpoint();
api.MapGetModuleByIdEndpoint();
api.MapUpdateModuleEndpoint();
api.MapDeleteModuleEndpoint();
api.MapCreateClientEndpoint();
api.MapGetClientsEndpoint();
api.MapGetClientByIdEndpoint();
api.MapUpdateClientEndpoint();
api.MapDeleteClientEndpoint();
api.MapCreateConfigurationQuestionEndpoint();
api.MapGetConfigurationQuestionsEndpoint();
api.MapGetConfigurationQuestionByIdEndpoint();
api.MapUpdateConfigurationQuestionEndpoint();
api.MapDeleteConfigurationQuestionEndpoint();
api.MapCreateConfigurationAnswerEndpoint();
api.MapGetConfigurationAnswersEndpoint();
api.MapGetConfigurationAnswerByIdEndpoint();
api.MapUpdateConfigurationAnswerEndpoint();
api.MapDeleteConfigurationAnswerEndpoint();
api.MapCreateDependencyRuleEndpoint();
api.MapGetDependencyRulesEndpoint();
api.MapGetDependencyRuleByIdEndpoint();
api.MapUpdateDependencyRuleEndpoint();
api.MapDeleteDependencyRuleEndpoint();
api.MapCreateProductEndpoint();
api.MapGetProductsEndpoint();
api.MapGetProductByIdEndpoint();
api.MapUpdateProductEndpoint();
api.MapDeleteProductEndpoint();
api.MapCreateProductTemplateEndpoint();
api.MapGetProductTemplatesEndpoint();
api.MapGetProductTemplateByIdEndpoint();
api.MapUpdateProductTemplateEndpoint();
api.MapDeleteProductTemplateEndpoint();
api.MapCreateProductConfigQuestionEndpoint();
api.MapGetProductConfigQuestionByIdEndpoint();
api.MapUpdateProductConfigQuestionEndpoint();
api.MapDeleteProductConfigQuestionEndpoint();
api.MapGet("weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(5)))
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program { }
