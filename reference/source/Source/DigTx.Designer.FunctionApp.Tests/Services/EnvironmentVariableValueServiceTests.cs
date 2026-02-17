namespace DigTx.Designer.FunctionApp.Tests.Services;

using System;
using System.Threading.Tasks;
using DigTx.Designer.FunctionApp.Infrastructure.Interfaces;
using DigTx.Designer.FunctionApp.Services;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Extensions.Logging;
using Moq;

public class EnvironmentVariableValueServiceTests
{
    private readonly Mock<ILogger<EnvironmentVariableValueService>> _loggerMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IEnvironmentVariableValueRepository> _envRepoMock = new();

    private EnvironmentVariableValueService CreateService()
    {
        _uowMock.SetupGet(u => u.EnvironmentVariableValueRepository).Returns(_envRepoMock.Object);
        return new EnvironmentVariableValueService(_loggerMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task GetProjectUrlAsync_ReturnsExpectedUrl()
    {
        // Arrange
        var orgUrl = "https://test.crm.dynamics.com";
        var appId = "00000000-0000-0000-0000-000000000123";
        var projectId = Guid.NewGuid();

        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameOrgUrlAsync()).ReturnsAsync(orgUrl);
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameAppIdAsync()).ReturnsAsync(appId);

        var service = CreateService();

        // Act
        var result = await service.GetProjectUrlAsync(projectId);

        // Assert
        var expected = $"{orgUrl}/main.aspx?appid={appId}&pagetype=entityrecord&etn={KT_Project.EntityLogicalName}&id={projectId}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetProjectUrlAsync_Throws_WhenOrgUrlMissing()
    {
        // Arrange
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameOrgUrlAsync()).ReturnsAsync((string)null);
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameAppIdAsync()).ReturnsAsync("appId");
        var service = CreateService();
        var projectId = Guid.NewGuid();

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProjectUrlAsync(projectId));
        Assert.Equal("Organization URL must be provided.", ex.Message);
    }

    [Fact]
    public async Task GetProjectUrlAsync_Throws_WhenAppIdMissing()
    {
        // Arrange
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameOrgUrlAsync()).ReturnsAsync("https://org");
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameAppIdAsync()).ReturnsAsync(string.Empty);
        var service = CreateService();
        var projectId = Guid.NewGuid();

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProjectUrlAsync(projectId));
        Assert.Equal("App ID must be provided.", ex.Message);
    }

    [Fact]
    public async Task GetProjectUrlAsync_Throws_WhenProjectIdEmpty()
    {
        // Arrange
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameOrgUrlAsync()).ReturnsAsync("https://org");
        _envRepoMock.Setup(r => r.GetEnvironmentVariableNameAppIdAsync()).ReturnsAsync("appid");
        var service = CreateService();

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProjectUrlAsync(Guid.Empty));
        Assert.Equal("Entity Id must be provided.", ex.Message);
    }
}
