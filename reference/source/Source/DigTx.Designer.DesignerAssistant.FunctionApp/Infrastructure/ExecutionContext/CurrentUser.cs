namespace DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext;
using System;
using System.Collections.Generic;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IFunctionContextAccessor _ctx;
    public CurrentUser(IFunctionContextAccessor ctx) => _ctx = ctx;

    public Guid? AadObjectId =>
        _ctx.FunctionContext?.Items is { } items
        && items.TryGetValue(Constants.AadOid, out var v)
        && v is Guid g ? g : null;
}
