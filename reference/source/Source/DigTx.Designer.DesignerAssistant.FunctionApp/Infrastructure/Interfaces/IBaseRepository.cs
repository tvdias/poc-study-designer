namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

public interface IBaseRepository<TEntity> 
    where TEntity : Entity
{
    Task<Guid[]> CreateRecordsInParallel(List<TEntity> entityList);
    Task CreateAsync(TEntity entity);
    void CreateResquestCreation(TEntity entity);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task UpdateAsync(TEntity entity);
}
