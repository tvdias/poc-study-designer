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

public class QuestionBankRepository : BaseRepository<KT_QuestionBank>, IQuestionBankRepository
{
    private readonly ILogger<QuestionBankRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public QuestionBankRepository(
    ILogger<QuestionBankRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KT_QuestionBank.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get Question Banks by a list of Ids and ensure they are active.
    /// </summary>
    /// <param name="ids">List of questions Id.</param>
    /// <returns> Returna a list of active question bank.</returns>
    public async Task<IList<KT_QuestionBank>> GetByIdsAsync(IList<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return [];
        }

        var query = new QueryExpression()
        {
            EntityName = KT_QuestionBank.EntityLogicalName,
            ColumnSet = new ColumnSet(true),
            Distinct = true,
        };

        query.Criteria.AddCondition(
            KT_QuestionBank.Fields.StatusCode,
            ConditionOperator.Equal,
            (int)KT_QuestionBank_StatusCode.Active);

        query.Criteria.AddCondition(KT_QuestionBank.Fields.Id, ConditionOperator.In, [.. ids.Cast<object>()]);

        var results = await _serviceClient.RetrieveMultipleAsync(query);

        return [.. results.Entities.Select(e => e.ToEntity<KT_QuestionBank>())];
    }
}
