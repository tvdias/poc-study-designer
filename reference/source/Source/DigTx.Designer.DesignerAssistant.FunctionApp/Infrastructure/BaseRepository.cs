namespace DigTx.Designer.FunctionApp.Infrastructure;

using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : Entity
{
    private readonly ILogger<object> _logger;
    private readonly ServiceClient _serviceClient;
    private readonly ICurrentUser _currentUser;
    private readonly string _entityLogicalName;
    private readonly ExecuteTransactionRequest _executeTransactionRequest;

    public BaseRepository(
      ILogger<object> logger,
      ServiceClient serviceClient,
      ICurrentUser currentUser,
      ExecuteTransactionRequest transactionRequest,
      string entityLogicalName)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _executeTransactionRequest = transactionRequest;
        _entityLogicalName = entityLogicalName ?? throw new ArgumentNullException(nameof(entityLogicalName));

        if (_currentUser.AadObjectId is Guid oid)
        {
            _serviceClient.CallerAADObjectId = oid;
        }
    }

    public async Task<Guid[]> CreateRecordsInParallel(List<TEntity> entityList)
    {
        ConcurrentBag<Guid> ids = [];

        // Disable affinity cookie
        _serviceClient.EnableAffinityCookie = false;

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism =
            _serviceClient.RecommendedDegreesOfParallelism
        };

        await Parallel.ForEachAsync(
            source: entityList,
            parallelOptions: parallelOptions,
            async (entity, token) =>
            {
                ids.Add(await _serviceClient.CreateAsync(entity, token));
            });

        return [.. ids];
    }

    /// <summary>
    /// Get Account by its unique identifier and ensure it is active.
    /// </summary>
    /// <param name="id"> Identifier.</param>
    /// <returns>Return a XRM Entity object if exists.</returns>
    public virtual async Task<TEntity?> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _serviceClient.RetrieveAsync(
                _entityLogicalName,
                id,
                new ColumnSet(true));

            return entity.ToEntity<TEntity>();
        }
        catch (FaultException ex)
        {
            if (ex.Message.Contains("Does Not Exist")) // Object does not exist
            {
                _logger.LogWarning("{Entity} with ID {Id} not found.", typeof(TEntity), id);
                return null;
            }
            throw;
        }
    }

    /// <summary>
    /// Create a CreateRequest to be executed in a transaction.
    /// </summary>
    /// <param name="entity"> XRM Entity. </param>
    public virtual void CreateResquestCreation(TEntity entity)
    {
        var createRequest = new CreateRequest { Target = entity };
        _executeTransactionRequest.Requests.Add(createRequest);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var query = new QueryExpression(typeof(TEntity).Name.ToLower())
        {
            ColumnSet = new ColumnSet(true)
        };
        var result = await Task.Run(() => _serviceClient.RetrieveMultiple(query));
        return result.Entities.Cast<TEntity>();
    }

    public async Task CreateAsync(TEntity entity)
    {
        await Task.Run(() => _serviceClient.Create(entity));
    }

    public async Task UpdateAsync(TEntity entity)
    {
        await Task.Run(() => _serviceClient.Update(entity));
    }

    public async Task DeleteAsync(Guid id)
    {
        await Task.Run(() => _serviceClient.Delete(typeof(TEntity).Name.ToLower(), id));
    }
}
