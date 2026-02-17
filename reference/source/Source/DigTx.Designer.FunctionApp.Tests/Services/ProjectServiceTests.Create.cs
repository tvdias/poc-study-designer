namespace DigTx.Designer.FunctionApp.Tests.Services;

using DigTx.Designer.DesignerAssistant.FunctionApp.Models.Requests;
using DigTx.Designer.FunctionApp.Exceptions;
using DigTx.Designer.FunctionApp.Models;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;
using Moq;

public partial class ProjectServiceTests
{
    [Fact()]
    public async Task CreateAsync_Succeeds_FullFlow()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        // Act

        var response = await _projectService.CreateAsync(request);

        // Assert

        _clientRepoMock.Verify(r => r.GetByIdAsync(clientId), Times.Once);

        _commissioningMarketRepoMock.Verify(r => r.GetByIdAsync(commissioningMarketId), Times.Once);

        _productRepoMock.Verify(r => r.GetByIdAsync(productId), Times.Once);

        _productTemplateRepoMock.Verify(pt => pt.GetByIdAsync(productTemplateId), Times.Once);

        _questionBankRepoMock.Verify(q => q.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Count == 1 && ids.Contains(existingQuestionId))), Times.Once);

        _moduleRepoMock.Verify(m => m.GetWithQuestionAsync(moduleId), Times.Once);

        _projectRepoMock.Verify(r => r.CreateAsync(It.Is<KT_Project>(p => p.Id == projectId)), Times.Once);

        _questionnaireLinesRepoMock.Verify(r =>
            r.CreateRecordsInParallel(It.Is<List<KT_QuestionnaireLines>>(l => l.Count == 2)), Times.Once);

        _questionnaireLinesAnswerListRepoMock.Verify(r =>
            r.CreateRecordsInParallel(It.Is<List<KTR_QuestionnaireLinesAnswerList>>(a => a.Count >= 1)), Times.Once);

        _envVarServiceMock.Verify(s => s.GetProjectUrlAsync(projectId), Times.Once);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.ProjectUrl));
        Assert.Equal(projectUrlResponse, response.ProjectUrl);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_AccountDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Client with ID {request.ClientId} does not exist.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_AccountInactive_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(new Account { Id = clientId, StatusCode = Account_StatusCode.Inactive });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Client with ID {request.ClientId} is not active.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_CommissioningMarkeDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _commissioningMarketRepoMock
            .Setup(r => r.GetByIdAsync(commissioningMarketId))
            .ReturnsAsync((KT_CommissioningMarket?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Commissioning Market with ID {request.CommissioningMarketId} does not exist.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_CommissioningMarketInactive_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _commissioningMarketRepoMock.Setup(r => r.GetByIdAsync(commissioningMarketId))
            .ReturnsAsync(new KT_CommissioningMarket
            {
                Id = commissioningMarketId,
                StatusCode = KT_CommissioningMarket_StatusCode.Inactive
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Commissioning Market with ID {request.CommissioningMarketId} is not active.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ProductDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((KTR_Product)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Product with ID {request.ProductId} is not found.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ProductInactive_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new KTR_Product { Id = productId, StatusCode = KTR_Product_StatusCode.Inactive });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Product with ID {request.ProductId} is not active.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ProductTemplateWithoutProduct_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _productTemplateRepoMock
            .Setup(r => r.GetByIdAsync(productTemplateId))
            .ReturnsAsync((KTR_ProductTemplate)null!);

        request.ProductId = Guid.Empty;

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal("ProductId is required when ProducTemplateId is requested", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ProductTemplateDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _productTemplateRepoMock
            .Setup(r => r.GetByIdAsync(productTemplateId))
            .ReturnsAsync((KTR_ProductTemplate)null!);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Product Template with ID {request.ProductTemplateId} is not found.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ProductTemplateInactive_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _productTemplateRepoMock.Setup(r => r.GetByIdAsync(productTemplateId)).ReturnsAsync(
            new KTR_ProductTemplate { Id = productTemplateId, StatusCode = KTR_ProductTemplate_StatusCode.Inactive });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Product Template with ID {request.ProductTemplateId} is not active.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_QuestionBankDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _questionBankRepoMock.Setup(q => q.GetByIdsAsync(new List<Guid>() { existingQuestionId }))
            .ReturnsAsync([]);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal("Questions not found in Question Bank.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ModuleDoesNotExist_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _moduleRepoMock.Setup(r => r.GetWithQuestionAsync(moduleId))
            .ReturnsAsync([]);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Module {moduleId} not found.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ModuleDoesNotContainQuestion_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        _moduleRepoMock.Setup(r => r.GetWithQuestionAsync(moduleId))
            .ReturnsAsync([new KT_Module { Id = moduleId, StatusCode = KT_Module_StatusCode.Active, Attributes = [] }]);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Module with ID {moduleId} not found for Question {existingQuestionId}.", ex.Message);
    }

    [Fact()]
    public async Task CreateAsync_FailsValidation_ModuleContainQuestionButDoesNotMatch_ThrowException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var commissioningMarketId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productTemplateId = Guid.NewGuid();
        var existingQuestionId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var projectUrlResponse = $"http://project-url/{projectId}";

        var request = SetMocks(
            projectId,
            clientId,
            commissioningMarketId,
            productId,
            productTemplateId,
            existingQuestionId,
            moduleId,
            projectUrlResponse);

        var alias = new AliasedValue(
                   KTR_ModuleQuestionBank.EntityLogicalName,
                   $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}",
                   new EntityReference() { Id = Guid.NewGuid() });

        var attributes = new AttributeCollection
        {
            { $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}", alias },
        };

        _moduleRepoMock.Setup(r => r.GetWithQuestionAsync(moduleId))
            .ReturnsAsync([new KT_Module { Id = moduleId, StatusCode = KT_Module_StatusCode.Active, Attributes = attributes }]);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidRequestException>(() => _projectService.CreateAsync(request));
        Assert.Equal($"Module with ID {moduleId} not found for Question {existingQuestionId}.", ex.Message);
    }

    private static void SetEntityId(Microsoft.Xrm.Sdk.Entity entity, Guid id)
    {
        entity.Id = id;
    }

    private ProjectCreationRequest SetMocks(
        Guid projectId,
        Guid clientId,
        Guid commissioningMarketId,
        Guid productId,
        Guid productTemplateId,
        Guid existingQuestionId,
        Guid moduleId,
        string projectUrlResponse)
    {
        var newQuestionVariable = "NEW_Q1";
        var existingQuestionVariable = "EXISTING_Q1";

        var request = new ProjectCreationRequest
        {
            ClientId = clientId,
            CommissioningMarketId = commissioningMarketId,
            ProductId = productId,
            ProductTemplateId = productTemplateId,
            Description = "A test project",
            ProjectName = "Test Project",
            Questions =
            [
                new QuestionCreationRequest
                {
                    DisplayOrder = 1,
                    Origin = OriginType.QuestionBank,
                    VariableName = existingQuestionVariable,
                    Id = existingQuestionId,
                    Module = new ModuleCreationRequest { Id = moduleId }
                },
                new QuestionCreationRequest
                {
                    StandardOrCustom = StandardOrCustomType.Custom,
                    QuestionType = QuestionType.SingleChoice,
                    Title = "New Question Title",
                    Text = "New Question Text",
                    ScripterNotes = "New Scripter Notes",
                    QuestionRationale = "New Rationale",
                    DisplayOrder = 2,
                    Origin = OriginType.New,
                    VariableName = newQuestionVariable,
                    Answers =
                    [
                        new AnswerCreationRequest { Name = "1", Text = "Yes" },
                        new AnswerCreationRequest { Name = "2", Text = "No" }
                    ]
                }
            ]
        };

        var question = new KT_QuestionBank() { Id = existingQuestionId };

        _clientRepoMock
            .Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(new Account { Id = clientId, StatusCode = Account_StatusCode.Active });

        _commissioningMarketRepoMock.Setup(r => r.GetByIdAsync(commissioningMarketId))
            .ReturnsAsync(new KT_CommissioningMarket
            {
                Id = commissioningMarketId,
                StatusCode = KT_CommissioningMarket_StatusCode.Active
            });

        _productRepoMock.Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(new KTR_Product { Id = productId, StatusCode = KTR_Product_StatusCode.Active });

        _productTemplateRepoMock.Setup(r => r.GetByIdAsync(productTemplateId)).ReturnsAsync(
            new KTR_ProductTemplate { Id = productTemplateId, StatusCode = KTR_ProductTemplate_StatusCode.Active });

        _questionBankRepoMock.Setup(q => q.GetByIdsAsync(new List<Guid>() { existingQuestionId }))
            .ReturnsAsync([new KT_QuestionBank
            {
                Id = existingQuestionId,
                KT_QuestionType = KT_QuestionType.SingleChoice,
                KT_StandardOrCustom = KT_QuestionBank_KT_StandardOrCustom.Standard,
                KT_Name = existingQuestionVariable,
                KT_QuestionTitle = "Existing Question Title",
                KT_DefaultQuestionText = "Existing Question Text",
                KTR_ScripterNotes = "Existing Scripter Notes",
                KT_QuestionRationale = "Existing Rationale",
                KT_QuestionVersion = 1,
                KT_IsDummyQuestion = false,
                KTR_AnswerList = "1|Yes;2|No"
            }]);

        var existingQuestion = new AliasedValue(
            KTR_ModuleQuestionBank.EntityLogicalName,
            $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}",
            new EntityReference() { Id = existingQuestionId });

        var noExistingQuestion = new AliasedValue(
            KTR_ModuleQuestionBank.EntityLogicalName,
            $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}",
            new EntityReference() { Id = Guid.NewGuid() });

        var modules = new List<KT_Module>
        {
            new() {
                Id = moduleId,
                StatusCode = KT_Module_StatusCode.Active,
                Attributes = new AttributeCollection
                {
                    { $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}", existingQuestion },
                }
            },
            new()
            {
                Id = moduleId,
                StatusCode = KT_Module_StatusCode.Active,
                Attributes = new AttributeCollection
                {
                    { $"{KTR_ModuleQuestionBank.EntityLogicalName}.{KTR_ModuleQuestionBank.Fields.KTR_QuestionBank}", noExistingQuestion },
                }
            }
        };

        _moduleRepoMock.Setup(r => r.GetWithQuestionAsync(moduleId))
            .ReturnsAsync(modules);

        _projectRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<KT_Project>()))
            .Callback<KT_Project>(p => SetEntityId(p, projectId))
            .Returns(Task.CompletedTask);

        var questionnaireLineIds = new List<Guid>();

        _questionnaireLinesRepoMock
            .Setup(r => r.CreateRecordsInParallel(It.IsAny<List<KT_QuestionnaireLines>>()))
            .Callback<List<KT_QuestionnaireLines>>(lines =>
            {
                questionnaireLineIds.AddRange(lines.Select(l => l.Id));
            })
            .ReturnsAsync([.. questionnaireLineIds]);

        _questionAnswerListRepoMock
            .Setup(r => r.GetByQuestionIdsAsync(It.IsAny<IList<Guid>>()))
            .ReturnsAsync([new() { KTR_KT_QuestionBank = new EntityReference() { Id = existingQuestionId } }]);

        var questionnaireAnswerLineIds = new List<Guid>();

        _questionnaireLinesAnswerListRepoMock
            .Setup(r => r.CreateRecordsInParallel(It.IsAny<List<KTR_QuestionnaireLinesAnswerList>>()))
            .Callback<List<KTR_QuestionnaireLinesAnswerList>>(answers =>
            {
                questionnaireAnswerLineIds.AddRange(answers.Select(a => a.Id));
            })
            .ReturnsAsync([.. questionnaireAnswerLineIds]);

        _envVarServiceMock
            .Setup(s => s.GetProjectUrlAsync(It.IsAny<Guid>()))
            .ReturnsAsync(projectUrlResponse);

        return request;
    }
}
