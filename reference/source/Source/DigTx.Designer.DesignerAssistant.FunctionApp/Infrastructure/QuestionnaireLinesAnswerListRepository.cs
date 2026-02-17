namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class QuestionnaireLinesAnswerListRepository
    : BaseRepository<KTR_QuestionnaireLinesAnswerList>, IQuestionnaireLinesAnswerListRepository
{
    public QuestionnaireLinesAnswerListRepository(
        ILogger<QuestionnaireLinesAnswerListRepository> logger,
        ServiceClient serviceClient,
        ICurrentUser currentUser,
        ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KTR_QuestionnaireLinesAnswerList.EntityLogicalName)
    {
    }
}
