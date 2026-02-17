namespace DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext;

using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using Microsoft.Azure.Functions.Worker;

public sealed class FunctionContextAccessor : IFunctionContextAccessor
{
    private static readonly AsyncLocal<FunctionContext?> _current = new();

    public FunctionContext? FunctionContext { get => _current.Value; set => _current.Value = value; }
}
