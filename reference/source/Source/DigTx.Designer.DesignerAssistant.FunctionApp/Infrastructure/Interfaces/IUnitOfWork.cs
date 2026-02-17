namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

public interface IUnitOfWork
{
    IClientRepository ClientRepository { get; }
    ICommissioningMarketRepository CommissioningMarketRepository { get; }
    IModuleRepository ModuleRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductTemplateRepository ProductTemplateRepository { get; }
    IProjectRepository ProjectRepository { get; }
    IQuestionBankRepository QuestionBankRepository { get; }
    IQuestionnaireLinesRepository QuestionnaireLinesRepository { get; }
    IQuestionnaireLinesAnswerListRepository QuestionnaireLinesAnswerListRepository { get; }
    IQuestionAnswerListRepository QuestionAnswerListRepository { get; }
    IEnvironmentVariableValueRepository EnvironmentVariableValueRepository { get; }
    ISystemUserRepository SystemUserRepository { get; }

    void BeginTransactionRequest();
    Task<OrganizationResponse> CommitAsync();
    void Dispose();
    void Rollback();
}
