namespace DigTx.IdGeneratorService.FunctionApp.Core.Middleware;

using System.Diagnostics.CodeAnalysis;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext;
using DigTx.Designer.DesignerAssistant.FunctionApp.Infrastructure.ExecutionContext.Interfaces;
using DigTx.IdGeneratorService.FunctionApp.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

[ExcludeFromCodeCoverage]
public sealed class AuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IFunctionContextAccessor _ctxAccessor;
    private readonly TokenValidationParameters _validationParams;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _oidcConfigManager;

    public AuthenticationMiddleware(
        IFunctionContextAccessor ctxAccessor,
        TokenValidationParameters validationParams,
        IConfigurationManager<OpenIdConnectConfiguration> oidcConfigManager)
    {
        _ctxAccessor = ctxAccessor;
        _validationParams = validationParams;
        _oidcConfigManager = oidcConfigManager;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        _ctxAccessor.FunctionContext = context;

        try
        {
            var request = await context.GetHttpRequestDataAsync();
            if (request is null)
            {
                var http = context.GetHttpContext()?.Response;
                if (http is not null)
                {
                    http.StatusCode = StatusCodes.Status400BadRequest;
                }
                return;
            }

            // Validate + get principal
            var principal = request.ValidateTokenAndGetPrincipal(_validationParams, _oidcConfigManager);

            // Extract AAD OID
            var oid = principal.GetAadObjectId()
                ?? throw new UnauthorizedAccessException("Authenticated user missing 'oid' claim.");

            context.Items ??= new Dictionary<object, object>();
            context.Items[Constants.AadOid] = oid;

            await next(context);
        }
        finally
        {
            _ctxAccessor.FunctionContext = null; // avoid leaks across invocations
        }
    }
}
