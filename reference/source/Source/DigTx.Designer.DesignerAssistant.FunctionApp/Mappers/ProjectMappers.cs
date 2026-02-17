namespace DigTx.Designer.FunctionApp.Mappers;

using System;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Models.Responses;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;

public static class ProjectMappers
{
    public static KT_Project MapToEntity(this ProjectCreationRequest request)
    {
        var project = new KT_Project
        {
            Id = Guid.NewGuid(),
            KT_Name = request.ProjectName,
            KT_Description = request.Description,
            KTR_ClientAccount = new EntityReference(Account.EntityLogicalName, request.ClientId),
            KT_CommissioningMarket = new EntityReference(KT_CommissioningMarket.EntityLogicalName, request.CommissioningMarketId),
            KTR_CustomOrPReconfigured = true,
            KTR_AccessTeam = false,
            StatusCode = KT_Project_StatusCode.Active
        };

        if (request.ProductId != Guid.Empty)
        {
            project.KTR_Product = new EntityReference(KTR_Product.EntityLogicalName, request.ProductId);
        }
        if (request.ProductTemplateId != Guid.Empty)
        {
            project.KTR_ProductTemplate = new EntityReference(KTR_ProductTemplate.EntityLogicalName, request.ProductTemplateId);
        }

        return project;
    }

    public static ProjectCreationResponse MapToResponse(this string projectUrl)
    {
        return new ProjectCreationResponse
        {
            ProjectUrl = projectUrl
        };
    }
}
