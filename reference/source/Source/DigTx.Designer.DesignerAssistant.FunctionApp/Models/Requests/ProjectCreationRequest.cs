namespace DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;

using System;
using System.Collections.Generic;

public class ProjectCreationRequest
{
    public required Guid ClientId { get; set; }

    public required Guid CommissioningMarketId { get; set; }

    public Guid ProductId { get; set; }

    public Guid ProductTemplateId { get; set; }

    public required string Description { get; set; }

    public required string ProjectName { get; set; }

    public List<QuestionCreationRequest> Questions { get; set; } = [];
}
