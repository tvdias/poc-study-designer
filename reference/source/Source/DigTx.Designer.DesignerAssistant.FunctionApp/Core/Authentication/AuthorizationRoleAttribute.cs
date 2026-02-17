namespace DigTx.Designer.FunctionApp.Core;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizationRoleAttribute : Attribute
{
    public AuthorizationRoleAttribute(string[] roles)
    {
        Roles = roles;
    }

    public string[] Roles { get; }
}
