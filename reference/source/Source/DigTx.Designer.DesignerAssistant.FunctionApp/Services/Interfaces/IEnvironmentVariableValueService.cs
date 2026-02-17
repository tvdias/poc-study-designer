namespace DigTx.Designer.FunctionApp.Services.Interfaces;

using System;
using System.Threading.Tasks;

public interface IEnvironmentVariableValueService
{
    Task<string> GetProjectUrlAsync(Guid projectId);
}