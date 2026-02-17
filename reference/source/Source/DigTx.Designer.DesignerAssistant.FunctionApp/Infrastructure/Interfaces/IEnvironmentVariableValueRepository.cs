namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

public interface IEnvironmentVariableValueRepository
{
    Task<string> GetEnvironmentVariableNameOrgUrlAsync();
    Task<string> GetEnvironmentVariableNameAppIdAsync();
}
