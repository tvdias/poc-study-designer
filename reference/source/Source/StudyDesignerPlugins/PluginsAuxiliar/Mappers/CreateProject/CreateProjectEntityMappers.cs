using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Mappers.CreateProject
{
    public static class CreateProjectEntityMappers
    {
        public static KT_Project MapToEntity(this CreateProjectRequest request)
        {
            var project = new KT_Project
            {
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

        public static CreateProjectRequest MapToRequest(
            Guid clientId,
            Guid commissioningMarketId,
            string description,
            Guid productId,
            Guid productTemplateId,
            string projectName,
            IList<QuestionRequest> questionRequest)
        {
            return new CreateProjectRequest
            {
                ClientId = clientId,
                CommissioningMarketId = commissioningMarketId,
                Description = description,
                ProductId = productId,
                ProductTemplateId = productTemplateId,
                ProjectName = projectName,
                Questions = questionRequest,
            };
        }
    }
}
