namespace DigTx.Designer.FunctionApp.Infrastructure;

using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class QuestionnaireLinesRepository : BaseRepository<KT_QuestionnaireLines>, IQuestionnaireLinesRepository
{
    public QuestionnaireLinesRepository(
        ILogger<QuestionnaireLinesRepository> logger,
        ServiceClient serviceClient,
        ICurrentUser currentUser,
        ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KT_QuestionnaireLines.EntityLogicalName)
    {
    }
}
