using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineScriptletsPostOperationTests
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
        public void WhenQLineCreated_ShouldCreateScriptletAndLinkToQline()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project).Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineScriptletsPostOperation>(pluginContext);

            // Assert
            var scriptlet = _context.CreateQuery<KTR_Scriptlets>().FirstOrDefault();
            Assert.IsNotNull(scriptlet);
            Assert.AreEqual($"Scriptlet - {qLine.Id}", scriptlet.GetAttributeValue<string>(KTR_Scriptlets.Fields.KTR_Name));

            var updatedQLine = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault(x => x.Id == qLine.Id);
            Assert.IsNotNull(updatedQLine);
            Assert.IsTrue(updatedQLine.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_ScriptletsLookup));
            Assert.AreEqual(scriptlet.Id, updatedQLine.GetAttributeValue<EntityReference>(KT_QuestionnaireLines.Fields.KTR_ScriptletsLookup).Id);
        }
    }
}
