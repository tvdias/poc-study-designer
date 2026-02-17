namespace DigTx.Designer.FunctionApp.Tests.Mappers;

using System;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Mappers;
using Kantar.StudyDesignerLite.Plugins;
using Xunit;

public class ProjectMappersTests
{
    private static ProjectCreationRequest CreateBaseRequest(
        Guid? clientId = null,
        Guid? commissioningMarketId = null,
        Guid? productId = null,
        Guid? productTemplateId = null,
        string projectName = "Test Project",
        string description = "Test Description")
    {
        return new ProjectCreationRequest
        {
            ClientId = clientId ?? Guid.NewGuid(),
            CommissioningMarketId = commissioningMarketId ?? Guid.NewGuid(),
            ProductId = productId ?? Guid.NewGuid(),
            ProductTemplateId = productTemplateId ?? Guid.NewGuid(),
            ProjectName = projectName,
            Description = description
        };
    }

    [Fact]
    public void MapToEntity_AllFieldsProvided_MapsCorrectly()
    {
        var request = CreateBaseRequest();

        var project = request.MapToEntity();

        Assert.NotNull(project);
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(request.ProjectName, project.KT_Name);
        Assert.Equal(request.Description, project.KT_Description);
        Assert.Equal(request.ClientId, project.KTR_ClientAccount.Id);
        Assert.Equal(request.CommissioningMarketId, project.KT_CommissioningMarket.Id);
        Assert.True(project.KTR_CustomOrPReconfigured);
        Assert.False(project.KTR_AccessTeam);
        Assert.Equal(KT_Project_StatusCode.Active, project.StatusCode);
        Assert.Equal(request.ProductId, project.KTR_Product.Id);
        Assert.Equal(request.ProductTemplateId, project.KTR_ProductTemplate.Id);
    }

    [Fact]
    public void MapToEntity_OptionalProductFieldsOmitted_DoesNotSetEntityReferences()
    {
        var request = CreateBaseRequest(productId: Guid.Empty, productTemplateId: Guid.Empty);

        var project = request.MapToEntity();

        Assert.Null(project.KTR_Product);
        Assert.Null(project.KTR_ProductTemplate);
        Assert.Equal(request.ProjectName, project.KT_Name);
        Assert.Equal(request.ClientId, project.KTR_ClientAccount.Id);
        Assert.Equal(request.CommissioningMarketId, project.KT_CommissioningMarket.Id);
    }

    [Fact]
    public void MapToEntity_TwoRequests_YieldDifferentIds()
    {
        var request1 = CreateBaseRequest(projectName: "Proj1");
        var request2 = CreateBaseRequest(projectName: "Proj2");

        var project1 = request1.MapToEntity();
        var project2 = request2.MapToEntity();

        Assert.NotEqual(project1.Id, project2.Id);
    }

    [Fact]
    public void MapToResponse_ValidUrl_MapsCorrectly()
    {
        var url = "https://example/project/123";

        var response = url.MapToResponse();

        Assert.NotNull(response);
        Assert.Equal(url, response.ProjectUrl);
    }

    [Fact]
    public void MapToResponse_EmptyString_MapsEmpty()
    {
        var url = string.Empty;

        var response = url.MapToResponse();

        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.ProjectUrl);
    }
}
