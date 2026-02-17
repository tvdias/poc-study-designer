namespace DigTx.Designer.FunctionApp.Tests.Functions;

using System.Reflection;
using DigTx.Designer.DesignerAssistant.FunctionApp.Functions;
using DigTx.Designer.FunctionApp.Core;

public class ProjectFunctionsTests
{

    [Fact]
    public void TargetMethod_ShouldHaveMyCustomAttribute_WithExpectedValues()
    {
        // Arrange
        var methodInfo = typeof(ProjectFunctions).GetMethod("CreateProjectAsync");

        // Act
        var attribute = methodInfo!.GetCustomAttribute<AuthorizationRoleAttribute>();

        // Assert
        Assert.NotNull(attribute);
        Assert.Contains(AuthRoles.Other, attribute!.Roles);
        Assert.DoesNotContain(AuthRoles.KantarScripter, attribute!.Roles);
        Assert.Contains(AuthRoles.KantarLibrarian, attribute!.Roles);
        Assert.Contains(AuthRoles.KantarCSUSer, attribute!.Roles);
    }

}
