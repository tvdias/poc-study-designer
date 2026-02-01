var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisClientBuilder("cache")
    .WithOutputCache();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseOutputCache();

// Simple Hello World API endpoints
var api = app.MapGroup("/api");

api.MapGet("hello", () => new { 
    message = "Hello World from Study Designer API!", 
    timestamp = DateTime.UtcNow 
})
.WithName("GetHello");

api.MapGet("studies", () => new[] {
    new { Id = 1, Name = "Study 1", Status = "Active" },
    new { Id = 2, Name = "Study 2", Status = "Draft" },
    new { Id = 3, Name = "Study 3", Status = "Completed" }
})
.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(5)))
.WithName("GetStudies");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();
