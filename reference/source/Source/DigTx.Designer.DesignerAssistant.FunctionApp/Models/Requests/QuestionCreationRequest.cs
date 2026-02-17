namespace DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;

using System;
using System.Collections.Generic;
using DigTx.Designer.FunctionApp.Models;

public class QuestionCreationRequest
{
    public required OriginType Origin { get; set; }

    public required int DisplayOrder { get; set; }

    public Guid? Id { get; set; }

    public ModuleCreationRequest? Module { get; set; }

    public StandardOrCustomType? StandardOrCustom { get; set; }

    public string? VariableName { get; set; }

    public string? Title { get; set; }

    public string? Text { get; set; }

    public string? ScripterNotes { get; set; }

    public string? QuestionRationale { get; set; }

    public QuestionType? QuestionType { get; set; }

    public bool IsDummyQuestion { get; set; }

    public List<AnswerCreationRequest> Answers { get; set; } = [];
}
