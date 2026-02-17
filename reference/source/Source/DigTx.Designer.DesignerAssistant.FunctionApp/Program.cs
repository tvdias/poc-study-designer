namespace DigTx.Designer.FunctionApp;

using System.Text.Json;
using DigTx.Designer.DesignerAssistant.FunctionApp.Exceptions;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Core.Extensions;
using DigTx.Designer.FunctionApp.Core.Middleware;
using DigTx.Designer.FunctionApp.Models;
using DigTx.IdGeneratorService.FunctionApp.Core.Extensions;
using DigTx.IdGeneratorService.FunctionApp.Core.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    private static async Task Main(string[] args)
    {
        var host = new HostBuilder()
           .ConfigureFunctionsWebApplication((IFunctionsWorkerApplicationBuilder builder) =>
           {
               var services = builder.Services;
               var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
               services.AddOptionsDependecies(configuration);

               // GlobalExceptionHandlerMiddleware must come frist
               builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
               builder.UseMiddleware<AuthenticationMiddleware>();
               builder.Use(next => async context =>
               {
                   await context.AddAuthorizationAsync(services);

                   await next(context);
               });
               builder.Services.Configure<JsonSerializerOptions>(o =>
               {
                   o.Converters.Add(new Core.Converts.CustomJsonStringEnumConverter<OriginType>());
                   o.Converters.Add(new Core.Converts.CustomJsonStringEnumConverter<AnswerType>());
                   o.Converters.Add(new Core.Converts.CustomJsonStringEnumConverter<QuestionType>());
                   o.Converters.Add(new Core.Converts.CustomJsonStringEnumConverter<StandardOrCustomType>());
                   o.WriteIndented = true;
               });
           })
           .ConfigureServices((context, services) =>
           {
               services.AddApplicationInsightsTelemetryWorkerService();
               services.ConfigureFunctionsApplicationInsights();
               services.AddServiceCollection(context.Configuration);
               services.AddHealthChecks(context.Configuration);
               services.AddSingleton<IFunctionContextAccessor, FunctionContextAccessor>();
           })
           .ConfigureOpenApi()
           .Build();

        await host.RunAsync();
    }
}
