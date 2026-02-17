namespace DigTx.Designer.FunctionApp.Infrastructure;

using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class SystemUserRepository : BaseRepository<SystemUser>, ISystemUserRepository
{
    public SystemUserRepository(
        ILogger<SystemUserRepository> logger,
        ServiceClient serviceClient,
        ICurrentUser currentUser,
        ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, SystemUser.EntityLogicalName)
    {
    }
}
