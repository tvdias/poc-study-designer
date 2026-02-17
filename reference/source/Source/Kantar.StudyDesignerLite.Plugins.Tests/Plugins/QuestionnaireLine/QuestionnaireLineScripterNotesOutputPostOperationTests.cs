using FakeXrmEasy;
using FakeXrmEasy.Extensions;
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
    public class QuestionnaireLineScripterNotesOutputPostOperationTests
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
        public void WhenQuestionnaireLineCreatedWithValidFields_ShouldGenerateHtml()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project)
                .WithRowSortOrder(KTR_SortOrder.Alphabetical)
                .WithColumnSortOrder(KTR_SortOrder.Random)
                .WithAnswerMin(5)
                .WithAnswerMax(10)
                .WithQuestionFormatDetails("Some format details")
                .WithScripterNotes("Some scripter notes")
                .WithCustomNotes("Custom note")
                .Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineScripterNotesOutputPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault(x => x.Id == qLine.Id);
            Assert.IsTrue(updated.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput));
            Assert.IsTrue(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Row Sort Order"));
            Assert.IsTrue(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Custom Notes"));
        }

        [TestMethod]
        public void WhenQuestionnaireLineUpdatedWithFewFields_ShouldGeneratePartialHtml()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var qLine = new QuestionnaireLineBuilder(project)
                .WithAnswerMin(2)
                .WithScripterNotes("Only notes")
                .Build();

            _context.Initialize(new List<Entity> { project, qLine });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Update);
            pluginContext.InputParameters["Target"] = qLine;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineScripterNotesOutputPostOperation>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault(x => x.Id == qLine.Id);
            Assert.IsTrue(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Answer Min"));
            Assert.IsFalse(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Answer Max"));
        }

        [TestMethod]
        public void WhenWrongEntity_ShouldNotDoAnything()
        {
            // Arrange
            var contact = new Entity("contact") { Id = Guid.NewGuid() };
            _context.Initialize(new[] { contact });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Create);
            pluginContext.InputParameters["Target"] = contact;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineScripterNotesOutputPostOperation>(pluginContext);

            // Assert: Ensure contact entity was not modified (no scripter notes output field added)
            var updated = _context.CreateQuery("contact").FirstOrDefault(x => x.Id == contact.Id);
            Assert.IsFalse(updated.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput));
        }
    }
}
