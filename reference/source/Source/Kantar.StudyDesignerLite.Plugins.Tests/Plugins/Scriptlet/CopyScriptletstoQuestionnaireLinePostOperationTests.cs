using System;
using System.Collections.Generic;
using System.Linq;
using FakeXrmEasy;
using FakeXrmEasy.Extensions;
using Kantar.StudyDesignerLite.Plugins.Scriptlets;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Scriptlets
{
    [TestClass]
    public class CopyScriptletstoQuestionnaireLinePostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void WhenScriptletUpdated_ShouldUpdateMatchingQuestionnaireLine()
        {
            // Arrange
            var scriptlet = new ScriptletBuilder()
                .WithScriptletInput("Updated Value")
                .Build();

            var qline = new QuestionnaireLineBuilder()
                .WithScriptletLookup(scriptlet)
                .WithScriptletInput("Old Value")
                .Build();

            _context.Initialize(new List<Entity> { scriptlet, qline });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = scriptlet;

            // Act
            _context.ExecutePluginWith<CopyScriptletstoQuestionnaireLinePostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KT_QuestionnaireLines.EntityLogicalName).FirstOrDefault(x => x.Id == qline.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Value", updated[KT_QuestionnaireLines.Fields.KTR_Scriptlets]);
        }

        [TestMethod]
        public void WhenScriptletsInputEmpty_ShouldUpdateQuestionnaireLineToNull()
        {
            // Arrange
            var scriptlet = new ScriptletBuilder()
                .WithScriptletInput(null) // No value in scriptlet
                .Build();

            var qline = new QuestionnaireLineBuilder()
                .WithScriptletLookup(scriptlet)
                .WithScriptletInput("Old Value") // Existing value in QLine
                .Build();

            _context.Initialize(new List<Entity> { scriptlet, qline });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = scriptlet;

            // Act
            _context.ExecutePluginWith<CopyScriptletstoQuestionnaireLinePostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KT_QuestionnaireLines.EntityLogicalName).FirstOrDefault(x => x.Id == qline.Id);
            Assert.IsFalse(updated.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_Scriptlets)); 
            Assert.IsNull(updated.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_Scriptlets));
        }

        [TestMethod]
        public void WhenNoMatchingQuestionnaireLine_ShouldExitWithoutError()
        {
            // Arrange
            var scriptlet = new ScriptletBuilder()
                .WithScriptletInput("Value")
                .Build();

            _context.Initialize(new List<Entity> { scriptlet });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = scriptlet;

            // Act
            _context.ExecutePluginWith<CopyScriptletstoQuestionnaireLinePostOperation>(pluginContext);

            // Assert: No questionnaire line should exist with the scriptlet lookup
            var qlines = _context.CreateQuery(KT_QuestionnaireLines.EntityLogicalName).ToList();
            Assert.IsTrue(qlines.All(q => !q.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_Scriptlets)));
        }

        [TestMethod]
        public void WhenDepthGreaterThanOne_ShouldExitEarly()
        {
            // Arrange
            var scriptlet = new ScriptletBuilder()
                .WithScriptletInput("Value")
                .Build();

            var qline = new QuestionnaireLineBuilder()
                .WithScriptletLookup(scriptlet)
                .Build();

            _context.Initialize(new List<Entity> { scriptlet, qline });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.Depth = 2;
            pluginContext.InputParameters["Target"] = scriptlet;

            // Act
            _context.ExecutePluginWith<CopyScriptletstoQuestionnaireLinePostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery(KT_QuestionnaireLines.EntityLogicalName).FirstOrDefault(x => x.Id == qline.Id);
            Assert.IsFalse(updated.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_Scriptlets));
        }
    }
}
