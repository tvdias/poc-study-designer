namespace DigTx.IdGeneratorService.FunctionApp.Core.Helpers;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using DigTx.Designer.FunctionApp.Core;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

[ExcludeFromCodeCoverage]
public static class AuthorizationHelper
{
    public static ClaimsPrincipal ValidateTokenAndGetPrincipal(
        [NotNull] this Microsoft.Azure.Functions.Worker.Http.HttpRequestData requestData,
        TokenValidationParameters validationParameters,
        IConfigurationManager<OpenIdConnectConfiguration> configManager)
    {
        if (!requestData.Headers.TryGetValues("Authorization", out var authHeaderValues))
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

        // Resolve signing keys from OIDC metadata (cached by ConfigurationManager)
        validationParameters.IssuerSigningKeyResolver = (t, st, kid, p) =>
        {
            var cfg = configManager.GetConfigurationAsync(default).GetAwaiter().GetResult();
            return cfg.SigningKeys;
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var _);
        return principal;
    }

    // Helper to read the AAD Object Id from a principal
    public static Guid? GetAadObjectId(this ClaimsPrincipal principal)
    {
        var oid =
            principal.FindFirst("oid")?.Value ??
            principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

        return Guid.TryParse(oid, out var g) ? g : null;
    }

    public static T? GetMethodCustomAttribute<T>(string methodFullName)
       where T : Attribute
    {
        ArgumentNullException.ThrowIfNull(methodFullName);

        var lastDotIndex = methodFullName.LastIndexOf('.');
        var typeName = methodFullName.Substring(0, lastDotIndex);
        var methodOnlyName = methodFullName.Substring(lastDotIndex + 1);

        var type = Type.GetType(typeName);
        ArgumentNullException.ThrowIfNull(type);

        var methodInfo = type.GetMethod(methodOnlyName);
        ArgumentNullException.ThrowIfNull(methodInfo);

        var att = methodInfo.GetCustomAttribute<T>();

        return att;
    }

    public static string[] GetMethodRole(this string methodFullName)
    {
        var att = GetMethodCustomAttribute<AuthorizationRoleAttribute>(methodFullName);

        return att?.Roles ?? [];
    }
}
