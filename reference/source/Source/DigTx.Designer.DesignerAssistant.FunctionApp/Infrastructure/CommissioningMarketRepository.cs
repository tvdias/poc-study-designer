namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class CommissioningMarketRepository : BaseRepository<KT_CommissioningMarket>, ICommissioningMarketRepository
{
    private readonly ILogger<CommissioningMarketRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public CommissioningMarketRepository(
    ILogger<CommissioningMarketRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, KT_CommissioningMarket.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get Account by its unique identifier and ensure it is active.
    /// </summary>
    /// <param name="id"> Identifier.</param>
    /// <returns>Return a Commission Market object if exists.</returns>
    public override async Task<KT_CommissioningMarket?> GetByIdAsync(Guid id)
    {
        var commissionMarket = await base.GetByIdAsync(id);

        if (commissionMarket is not null && commissionMarket.StatusCode != KT_CommissioningMarket_StatusCode.Active)
        {
            _logger.LogWarning("Commission Market with ID {Id} is not active.", id);
            return null;
        }

        return commissionMarket;
    }
}
