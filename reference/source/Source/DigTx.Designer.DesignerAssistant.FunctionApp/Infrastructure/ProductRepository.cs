namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class ProductRepository : BaseRepository<KTR_Product>, IProductRepository
{
    private readonly ILogger<ProductRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public ProductRepository(
    ILogger<ProductRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KTR_Product.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }
}
