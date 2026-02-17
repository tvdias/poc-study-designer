namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;

public class ClientRepository : BaseRepository<Account>, IClientRepository
{
    private readonly ILogger<ClientRepository> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;

    public ClientRepository(
    ILogger<ClientRepository> logger,
    ServiceClient serviceClient,
    ICurrentUser currentUser,
    ExecuteTransactionRequest transactionRequest)
        : base(logger, serviceClient, currentUser, transactionRequest, Account.EntityLogicalName)
    {
        _logger = logger;
        _serviceClient = serviceClient;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get Account by its unique identifier and ensure it is active.
    /// </summary>
    /// <param name="id"> Identifier.</param>
    /// <returns>Return a account object if exists.</returns>
    public override async Task<Account?> GetByIdAsync(Guid id)
    {
        var account = await base.GetByIdAsync(id);

        if (account is not null && account.StatusCode != Account_StatusCode.Active)
        {
            _logger.LogWarning("Account Client with ID {Id} is not active.", id);
            return null;
        }

        return account;
    }
}
