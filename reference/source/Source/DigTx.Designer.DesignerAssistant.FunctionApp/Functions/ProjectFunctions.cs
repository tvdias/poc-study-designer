namespace DigTx.Designer.DesignerAssistant.FunctionApp.Functions;

using System.Net;
using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Core;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

public class ProjectFunctions
{
    private readonly ILogger<ProjectFunctions> _logger;
    private readonly IProjectService _projectService;

    public ProjectFunctions(
        ILogger<ProjectFunctions> logger,
        IProjectService projectService)
    {
        _logger = logger
            ?? throw new ArgumentNullException(nameof(ILogger<ProjectFunctions>));
        _projectService = projectService
            ?? throw new ArgumentNullException(nameof(IProjectService));
    }

    [Function("GetProjectById")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(KT_Project),
        Description = "Friendly Project Id")]
    public async Task<IActionResult> GetProjectByIdAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "get",
            Route = "projects/{id}")] HttpRequest _,
        Guid id)
    {
        var response = await _projectService.GetByIdAsync(id);

        if (response is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(response);
    }

    [Function("CreateProject")]
    [AuthorizationRole([AuthRoles.Other, AuthRoles.KantarLibrarian, AuthRoles.KantarCSUSer])]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(ProjectCreationRequest),
        Description = "Create Project")]
    public async Task<IActionResult> CreateProjectAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "projects")] HttpRequest req,
        [FromBody] ProjectCreationRequest request)
    {
        var response = await _projectService.CreateAsync(request);

        return new OkObjectResult(response);
    }


}
