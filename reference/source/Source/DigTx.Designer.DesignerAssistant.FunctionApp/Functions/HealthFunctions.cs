namespace DigTx.Designer.FunctionApp.Functions;

using System.Net;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Mappers;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class HealthFunctions
{
    private readonly HealthCheckService _healthService;

    public HealthFunctions(HealthCheckService healthService)
    {
        _healthService = healthService
            ?? throw new ArgumentNullException(nameof(HealthCheckService));
    }

    [Function("HealthCheck")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(KT_Project),
        Description = "HealthCheck")]
    public async Task<IActionResult> HealthCheckAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
        var result = await _healthService.CheckHealthAsync();

        var healthStatus = result.ToResponse();

        return new ObjectResult(healthStatus)
        {
            StatusCode = result.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable
        };
    }
}
