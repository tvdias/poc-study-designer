namespace DigTx.Designer.FunctionApp.Mappers;

using System;
using System.Linq;
using DigTx.Designer.FunctionApp.Models.Responses;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal static class HealthcheckMappers
{
    internal static HealthCheckResponse ToResponse(this HealthReport report)
    {
        return new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Dependencies = report.Entries.ToDictionary(
                e => e.Key,
                e => e.Value.Status.ToString())
        };
    }
}
