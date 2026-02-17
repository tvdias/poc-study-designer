namespace DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;

using DigTx.Designer.FunctionApp.Models;

public class AnswerCreationRequest
{
    public string Name { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public AnswerType? Location { get; set; }
}
