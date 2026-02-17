namespace DigTx.Designer.FunctionApp.Tests.Services;

using System;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using DigTx.Designer.FunctionApp.Services;
using DigTx.Designer.FunctionApp.Services.Interfaces;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Moq;

public partial class ProjectServiceTests
{
    private readonly Mock<ILogger<ProjectService>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEnvironmentVariableValueService> _envVarServiceMock = new();
    private readonly ProjectService _projectService;

    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<IQuestionnaireLinesRepository> _questionnaireLinesRepoMock = new();
    private readonly Mock<IQuestionAnswerListRepository> _questionAnswerListRepoMock = new();
    private readonly Mock<IQuestionnaireLinesAnswerListRepository> _questionnaireLinesAnswerListRepoMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<ICommissioningMarketRepository> _commissioningMarketRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IProductTemplateRepository> _productTemplateRepoMock = new();
    private readonly Mock<IQuestionBankRepository> _questionBankRepoMock = new();
    private readonly Mock<IModuleRepository> _moduleRepoMock = new();

    public ProjectServiceTests()
    {
        _uowMock.SetupGet(x => x.ClientRepository).Returns(_clientRepoMock.Object);
        _uowMock.SetupGet(x => x.CommissioningMarketRepository).Returns(_commissioningMarketRepoMock.Object);
        _uowMock.SetupGet(x => x.ProductRepository).Returns(_productRepoMock.Object);
        _uowMock.SetupGet(x => x.ProductTemplateRepository).Returns(_productTemplateRepoMock.Object);
        _uowMock.SetupGet(x => x.QuestionBankRepository).Returns(_questionBankRepoMock.Object);
        _uowMock.SetupGet(x => x.ModuleRepository).Returns(_moduleRepoMock.Object);
        _uowMock.SetupGet(x => x.QuestionnaireLinesRepository).Returns(_questionnaireLinesRepoMock.Object);
        _uowMock.SetupGet(x => x.QuestionAnswerListRepository).Returns(_questionAnswerListRepoMock.Object);
        _uowMock.SetupGet(x => x.QuestionnaireLinesAnswerListRepository).Returns(_questionnaireLinesAnswerListRepoMock.Object);
        _uowMock.SetupGet(u => u.ProjectRepository).Returns(_projectRepoMock.Object);
        _projectService = new ProjectService(
            _loggerMock.Object,
            _uowMock.Object,
            _envVarServiceMock.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Succeeds()
    {
        // Arrange
        var logger = new Mock<ILogger<ProjectService>>();
        var uow = new Mock<IUnitOfWork>();
        var env = new Mock<IEnvironmentVariableValueService>();

        // Act
        var service = new ProjectService(logger.Object, uow.Object, env.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var uow = new Mock<IUnitOfWork>();
        var env = new Mock<IEnvironmentVariableValueService>();

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new ProjectService(null!, uow.Object, env.Object));

        // Assert
        Assert.Equal("ILogger", ex.ParamName);
    }

    [Fact]
    public void Constructor_NullUnitOfWork_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<ProjectService>>();
        var env = new Mock<IEnvironmentVariableValueService>();

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new ProjectService(logger.Object, null!, env.Object));

        // Assert
        // Note: Constructor uses nameof(IProjectRepository) instead of IUnitOfWork.
        Assert.Equal("IProjectRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_NullEnvironmentVariableValueService_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<ProjectService>>();
        var uow = new Mock<IUnitOfWork>();

        // Act
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new ProjectService(logger.Object, uow.Object, null!));

        // Assert
        Assert.Equal("IEnvironmentVariableValueService", ex.ParamName);
    }

    [Fact]
    public async Task GetByIdAsync_ProjectExists_ReturnsProject()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new KT_Project { Id = projectId };
        _projectRepoMock
            .Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _projectService.GetByIdAsync(projectId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(projectId, result!.Id);

        _projectRepoMock.Verify(r => r.GetByIdAsync(projectId), Times.Once);

        // Ensure no warning logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ProjectMissing_LogsWarningAndReturnsNull()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepoMock
            .Setup(r => r.GetByIdAsync(projectId))
            .ReturnsAsync((KT_Project?)null);

        // Act
        var result = await _projectService.GetByIdAsync(projectId);

        // Assert
        Assert.Null(result);
        _projectRepoMock.Verify(r => r.GetByIdAsync(projectId), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Project with ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
