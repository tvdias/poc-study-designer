using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Helpers
{
    [TestClass]
    public class PluginContextHelperTests
    {

        [TestMethod]
        public void PluginContextHelper_GetInputParameter_Success()
        {
            // Arrange
            var key = "Target";
            var expected = new Entity("account") { Id = Guid.NewGuid() };

            var contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(c => c.InputParameters).Returns(new ParameterCollection
            {
                { key, expected }
            });

            // Act
            var result = contextMock.Object.GetInputParameter<Entity>(key);

            // Assert
            Assert.AreSame(expected, result);
        }
    }
}
