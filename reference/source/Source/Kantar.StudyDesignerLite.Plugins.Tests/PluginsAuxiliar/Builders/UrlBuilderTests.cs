using Kantar.StudyDesignerLite.PluginsAuxiliar.Builders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.General;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Builders
{
    [TestClass]
    public class UrlBuilderTests
    {
        private const string ExpectedOrgUrl = "https://contoso.crm.dynamics.com";
        private const string ExpectedAppId = "c38c8672-c0c2-4c74-83f0-c626e97b763f";
        private const string EntityLogicalName = "kt_project";
        private static readonly Guid EntityId = Guid.NewGuid();

        private Mock<IEnvironmentVariablesService> _mockEnvironmentService;

        [TestInitialize]
        public void Setup()
        {
            _mockEnvironmentService = new Mock<IEnvironmentVariablesService>();
        }

        [TestMethod]
        public void BuildEntityUrl_AllInputsProvided_ReturnsCorrectUrl()
        {
            // Arrange
            _mockEnvironmentService.Setup(s => s.GetEnvironmentVariableValue("ktr_OrgUrl"))
                .Returns(ExpectedOrgUrl);
            _mockEnvironmentService.Setup(s => s.GetEnvironmentVariableValue("ktr_AppId"))
                .Returns(ExpectedAppId);

            var urlBuilder = new UrlBuilder(_mockEnvironmentService.Object)
                .WithOrgUrl()
                .WithAppId()
                .WithEntity(EntityLogicalName)
                .WithId(EntityId);

            var expectedUrl = $"{ExpectedOrgUrl}/main.aspx?appid={ExpectedAppId}&pagetype=entityrecord&etn={EntityLogicalName}&id={EntityId}";

            // Act
            var actualUrl = urlBuilder.BuildEntityUrl();

            // Assert
            Assert.AreEqual(expectedUrl, actualUrl);
        }
    }
}
