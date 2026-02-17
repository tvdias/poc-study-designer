namespace DigTx.Designer.FunctionApp.Models.Responses;

using System;
using System.Collections.Generic;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public Dictionary<string, string> Dependencies { get; set; } = new();
}
