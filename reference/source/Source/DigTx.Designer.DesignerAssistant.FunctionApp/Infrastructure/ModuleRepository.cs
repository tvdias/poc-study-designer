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

public class ModuleRepository : BaseRepository<KT_Module>, IModuleRepository
{
    private readonly ILogger<ModuleRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public ModuleRepository(
    ILogger<ModuleRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KT_Module.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get Module with its related Question Banks.
    /// </summary>
    /// <param name="id"> Module Id.</param>
    /// <returns> Return a list of Modules.</returns>
    public async Task<IList<KT_Module>> GetWithQuestionAsync(Guid id)
    {
        var query = new QueryExpression()
        {
            EntityName = KT_Module.EntityLogicalName,
            ColumnSet = new ColumnSet(true),
        };

        query.Criteria.AddCondition(
            KT_Module.Fields.StatusCode,
            ConditionOperator.Equal,
            (int)KT_Module_StatusCode.Active);

        query.Criteria.AddCondition(KT_Module.Fields.Id, ConditionOperator.Equal, id);

        // INNER JOIN ModuleQuestionBank
        var moduleQuestionBankLink = query.AddLink(
            KTR_ModuleQuestionBank.EntityLogicalName,
            KT_Module.Fields.Id,
            KTR_ModuleQuestionBank.Fields.KTR_Module,
            JoinOperator.Inner
        );

        moduleQuestionBankLink.Columns = new ColumnSet(KTR_ModuleQuestionBank.Fields.KTR_QuestionBank);
        moduleQuestionBankLink.EntityAlias = KTR_ModuleQuestionBank.EntityLogicalName;

        moduleQuestionBankLink.LinkCriteria.AddCondition(
            KTR_ModuleQuestionBank.Fields.StatusCode,
            ConditionOperator.Equal,
            (int)KTR_ModuleQuestionBank_StatusCode.Active);

        var results = await _serviceClient.RetrieveMultipleAsync(query);

        return [.. results.Entities.Select(e => e.ToEntity<KT_Module>())];
    }
}
