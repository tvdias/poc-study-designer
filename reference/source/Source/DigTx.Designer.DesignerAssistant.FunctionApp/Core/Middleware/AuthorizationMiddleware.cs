namespace DigTx.Designer.FunctionApp.Core.Middleware;

using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using DigTx.Designer.FunctionApp.Core.Extensions;
using DigTx.Designer.FunctionApp.Core.Options;
using DigTx.Designer.FunctionApp.Exceptions;
using DigTx.IdGeneratorService.FunctionApp.Core.Helpers;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

[ExcludeFromCodeCoverage]
internal static class AuthorizationMiddleware
{
    /// <summary>
    /// Add Authorization Middleware to valid JWT user against Dataverser user Business Role.
    /// </summary>
    /// <param name="context">FunctionContext.</param>
    /// <param name="services">IServiceCollection.</param>
    /// <returns>Task.</returns>
    /// <exception cref="UnauthorizedAccessException"> Thorw UnauthorizedAccessException if user does not have correct role.</exception>
    public static async Task AddAuthorizationAsync(this FunctionContext context, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestData = await context.GetHttpRequestDataAsync();

        var request = requestData!.FunctionContext.FunctionDefinition;

        var roles = request.EntryPoint.GetMethodRole();

        if (roles is null || roles.Length == 0)
        {
            return;
        }

        var user = await GetSystemUserFromTokenAsync(context, services);

        if (user.KTR_BusinessRole is null || !roles.Any(r => r.Equals(user.KTR_BusinessRoleName.ToString())))
        {
            throw new ForbiddenAccessException("User does not have the required role.");
        }

        return;
    }

    private static async Task<SystemUser> GetSystemUserFromTokenAsync(
        FunctionContext context,
        IServiceCollection services)
    {

        var oid = await GetUserIdAsync(context);

        if (oid == Guid.Empty)
        {
            throw new ForbiddenAccessException("User is not authenticated.");
        }

        using var client = GetServiceClient(services);

        return GetLoggedUser(client, oid);
    }

    private static SystemUser GetLoggedUser(ServiceClient serviceClient, Guid oid)
    {
        var query = new QueryExpression(SystemUser.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(
                SystemUser.Fields.SystemUserId,
                SystemUser.Fields.FullName,
                SystemUser.Fields.AzureActiveDirectoryObjectId,
                SystemUser.Fields.KTR_BusinessRole),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression(SystemUser.Fields.AzureActiveDirectoryObjectId, ConditionOperator.Equal, oid)
                }
            }
        };

        var users = serviceClient.RetrieveMultiple(query);

        var user = users.Entities.FirstOrDefault()
            ?? throw new UnauthorizedAccessException("User not found.");

        return user.ToEntity<SystemUser>();
    }

    private static ServiceClient GetServiceClient(IServiceCollection services)
    {
        var dataverseOptions = OptionsExtensions
            .GetOptionsValue<DataverseOptions>(services);

        return new ServiceClient(dataverseOptions.ConnectionString);
    }

    private static async Task<Guid> GetUserIdAsync(FunctionContext context)
    {
        var request = await context.GetHttpRequestDataAsync();
        if (request is null)
        {
            var http = context.GetHttpContext()?.Response;
            if (http is not null)
            {
                http.StatusCode = StatusCodes.Status400BadRequest;
            }
            return Guid.Empty;
        }
        if (!request.Headers.TryGetValues("Authorization", out var authHeaderValues))
        {
            throw new UnauthorizedAccessException("Authorization header is missing.");
        }

        var authHeader = authHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid or missing token.");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token))
        {
            throw new UnauthorizedAccessException("Invalid token format.");
        }

        var jwtToken = tokenHandler.ReadJwtToken(token);

        var oid = jwtToken.Claims.FirstOrDefault(c => c.Type == "oid")?.Value
            ?? throw new UnauthorizedAccessException("Authenticated user missing 'oid' claim.");

        return Guid.Parse(oid);
    }
}
