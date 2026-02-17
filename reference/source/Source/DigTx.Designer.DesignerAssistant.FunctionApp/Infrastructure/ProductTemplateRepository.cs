namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class ProductTemplateRepository : BaseRepository<KTR_ProductTemplate>, IProductTemplateRepository
{
    private readonly ILogger<ProductTemplateRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public ProductTemplateRepository(
    ILogger<ProductTemplateRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KTR_ProductTemplate.EntityLogicalName)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        _currentUser = currentUser;
    }
}
