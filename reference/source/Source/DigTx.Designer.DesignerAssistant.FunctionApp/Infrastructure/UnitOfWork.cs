namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

public class UnitOfWork : IDisposable, IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;
    private IProjectRepository _projectRepository;
    private IClientRepository _clienteService;
    private ICommissioningMarketRepository _commissioningMarketService;
    private IProductRepository _productRepository;
    private IProductTemplateRepository _productTemplateRepository;
    private IQuestionBankRepository _questionBankRepository;
    private IModuleRepository _moduleRepository;
    private IQuestionnaireLinesRepository _questionnaireLinesRepository;
    private IQuestionnaireLinesAnswerListRepository _questionnaireLinesAnswerListRepository;
    private IQuestionAnswerListRepository _questionAnswerListRepository;
    private IEnvironmentVariableValueRepository _environmentVariableValueRepository;
    private ISystemUserRepository _systemUserRepository;
    private ExecuteTransactionRequest _executeTransactionRequest;
    private bool _disposed = false;

    public UnitOfWork(
        ILoggerFactory loggerFactory,
        ServiceClient serviceClient,
        ICurrentUser currentUser)
    {
        _loggerFactory = loggerFactory
            ?? throw new ArgumentNullException(nameof(ILoggerFactory));
        _serviceClient = serviceClient
            ?? throw new ArgumentNullException(nameof(ServiceClient));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(ICurrentUser));
        _logger = loggerFactory.CreateLogger<UnitOfWork>();
    }

    public ISystemUserRepository SystemUserRepository
    {
        get
        {
            this._systemUserRepository ??= new SystemUserRepository(
                    _loggerFactory.CreateLogger<SystemUserRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _systemUserRepository;
        }
    }

    public IEnvironmentVariableValueRepository EnvironmentVariableValueRepository
    {
        get
        {
            this._environmentVariableValueRepository ??= new EnvironmentVariableValueRepository(_serviceClient);
            return _environmentVariableValueRepository;
        }
    }

    public IProjectRepository ProjectRepository
    {
        get
        {
            this._projectRepository ??= new ProjectRepository(
                    _loggerFactory.CreateLogger<ProjectRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _projectRepository;
        }
    }

    public IClientRepository ClientRepository
    {
        get
        {
            this._clienteService ??= new ClientRepository(
                    _loggerFactory.CreateLogger<ClientRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _clienteService;
        }
    }

    public ICommissioningMarketRepository CommissioningMarketRepository
    {
        get
        {
            this._commissioningMarketService ??= new CommissioningMarketRepository(
                    _loggerFactory.CreateLogger<CommissioningMarketRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _commissioningMarketService;
        }
    }

    public IProductRepository ProductRepository
    {
        get
        {
            this._productRepository ??= new ProductRepository(
                    _loggerFactory.CreateLogger<ProductRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _productRepository;
        }
    }

    public IProductTemplateRepository ProductTemplateRepository
    {
        get
        {
            this._productTemplateRepository ??= new ProductTemplateRepository(
                    _loggerFactory.CreateLogger<ProductTemplateRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _productTemplateRepository;
        }
    }

    public IQuestionBankRepository QuestionBankRepository
    {
        get
        {
            this._questionBankRepository ??= new QuestionBankRepository(
                    _loggerFactory.CreateLogger<QuestionBankRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _questionBankRepository;
        }
    }

    public IModuleRepository ModuleRepository
    {
        get
        {
            this._moduleRepository ??= new ModuleRepository(
                    _loggerFactory.CreateLogger<ModuleRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _moduleRepository;
        }
    }

    public IQuestionnaireLinesRepository QuestionnaireLinesRepository
    {
        get
        {
            this._questionnaireLinesRepository ??= new QuestionnaireLinesRepository(
                    _loggerFactory.CreateLogger<QuestionnaireLinesRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _questionnaireLinesRepository;
        }
    }

    public IQuestionnaireLinesAnswerListRepository QuestionnaireLinesAnswerListRepository
    {
        get
        {
            this._questionnaireLinesAnswerListRepository ??= new QuestionnaireLinesAnswerListRepository(
                    _loggerFactory.CreateLogger<QuestionnaireLinesAnswerListRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _questionnaireLinesAnswerListRepository;
        }
    }

    public IQuestionAnswerListRepository QuestionAnswerListRepository
    {
        get
        {
            this._questionAnswerListRepository ??= new QuestionAnswerListRepository(
                    _loggerFactory.CreateLogger<QuestionAnswerListRepository>(),
                    _serviceClient,
                    _currentUser,
                    _executeTransactionRequest);
            return _questionAnswerListRepository;
        }
    }

    public void Rollback()
    {
        _executeTransactionRequest?.Requests.Clear();
    }

    public void BeginTransactionRequest()
    {
        _executeTransactionRequest = new ExecuteTransactionRequest
        {
            Requests = [],
            ReturnResponses = true,
        };
    }

    public async Task<OrganizationResponse> CommitAsync()
    {
        return await _serviceClient.ExecuteAsync(_executeTransactionRequest);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                _executeTransactionRequest?.Requests.Clear();
            }
        }
        this._disposed = true;
    }
}
