namespace DigTx.Designer.FunctionApp.Services.Interfaces;

using System;
using System.Threading.Tasks;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Models.Responses;
using Kantar.StudyDesignerLite.Plugins;

public interface IProjectService
{
    Task<KT_Project?> GetByIdAsync(Guid id);

    Task<ProjectCreationResponse> CreateAsync(ProjectCreationRequest request);
}
