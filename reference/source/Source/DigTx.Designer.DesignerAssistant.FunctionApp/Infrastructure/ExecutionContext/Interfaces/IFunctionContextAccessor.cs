namespace DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;

using Microsoft.Azure.Functions.Worker;

public interface IFunctionContextAccessor
{
    FunctionContext? FunctionContext { get; set; }
}
