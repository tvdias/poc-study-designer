namespace DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;

using System;

public interface ICurrentUser
{
    Guid? AadObjectId { get; }
}
