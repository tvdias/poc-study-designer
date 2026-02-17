namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Subset
{
    using System;
    using System.Reflection;
    using Kantar.StudyDesignerLite.Plugins.Subset;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Moq;

    [TestClass]
    public class SubsetDefinitionPreventManualUpdatePreValidationTests
    {
        private readonly Mock<ITracingService> _tracing = new Mock<ITracingService>();
        private readonly Mock<ILocalPluginContext> _context = new Mock<ILocalPluginContext>();
        private readonly Mock<IPluginExecutionContext> _pluginContext = new Mock<IPluginExecutionContext>();
        private readonly SubsetDefinitionPreventManualUpdatePreValidation _plugin = new SubsetDefinitionPreventManualUpdatePreValidation();

        private const string HelperTypeName = "Kantar.StudyDesignerLite.Plugins.Subset.SubsetDefinitionPreventManualUpdatePreValidation";

        private static Type GetHelperType() =>
            typeof(SubsetDefinitionPreventManualUpdatePreValidation).Assembly.GetType(HelperTypeName, throwOnError: true);

        private static object Invoke(string methodName, SubsetDefinitionPreventManualUpdatePreValidation obj, params object[] args)
        {
            var t = GetHelperType();
            var m = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(m, $"Method {methodName} not found.");
            return m.Invoke(obj, args);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _context.Setup(c => c.TracingService).Returns(_tracing.Object);
            _context.Setup(c => c.PluginExecutionContext).Returns(_pluginContext.Object);
        }

        [TestMethod]
        public void Execute_ShouldThrow_WhenContextIsNull()
        {
            // Act & Assert
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                Invoke("ExecuteCdsPlugin", this._plugin, new object[] { null }));

            Assert.IsInstanceOfType(ex.InnerException, typeof(ArgumentNullException));
            StringAssert.Contains(ex.InnerException.Message, "localPluginContext");
        }

        [TestMethod]
        public void Execute_ShouldThrow_WhenDepthIsOne()
        {
            // Arange
            _pluginContext.Setup(x => x.Depth).Returns(1);

            // Act & Assert
            var ex = Assert.ThrowsException<TargetInvocationException>(() =>
                Invoke("ExecuteCdsPlugin", this._plugin, _context.Object));

            Assert.IsInstanceOfType(ex.InnerException, typeof(InvalidPluginExecutionException));
            StringAssert.Contains(ex.InnerException.Message, "Change to this record is not allowed from the UI.");
            _tracing.Verify(x => x.Trace($"{KTR_SubsetDefinition.EntitySchemaName} change attempt blocked."), Times.Once);
        }

        [TestMethod]
        public void Execute_Should_Complete_WhenDepthIsNotOne()
        {
            // Arange
            _pluginContext.Setup(x => x.Depth).Returns(2);

            // Act
            var ex = Invoke("ExecuteCdsPlugin", this._plugin, _context.Object);

            // Assert
            _tracing.Verify(x => x.Trace($"{KTR_SubsetDefinition.EntitySchemaName} change attempt allowed."), Times.Once);
        }
    }
}
