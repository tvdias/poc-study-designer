namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

public class QuestionAnswerListRepository : BaseRepository<KTR_QuestionAnswerList>, IQuestionAnswerListRepository
{
    private readonly ILogger<QuestionAnswerListRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public QuestionAnswerListRepository(
    ILogger<QuestionAnswerListRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KTR_QuestionAnswerList.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }

    public async Task<IList<KTR_QuestionAnswerList>> GetByQuestionIdsAsync(IList<Guid> questionIds)
    {
        if (questionIds == null || questionIds.Count == 0)
        {
            return [];
        }

        var query = new QueryExpression()
        {
            EntityName = KTR_QuestionAnswerList.EntityLogicalName,
            ColumnSet = new ColumnSet(true),
            Distinct = true,
        };

        query.Criteria
            .AddCondition(
            KTR_QuestionAnswerList.Fields.StatusCode,
            ConditionOperator.Equal,
            (int)KTR_QuestionAnswerList_StatusCode.Active);

        query.Criteria
            .AddCondition(
            KTR_QuestionAnswerList.Fields.KTR_KT_QuestionBank,
            ConditionOperator.In,
            [.. questionIds.Cast<object>()]);

        var results = await _serviceClient.RetrieveMultipleAsync(query);

        return [.. results.Entities.Select(e => e.ToEntity<KTR_QuestionAnswerList>())];
    }
}
