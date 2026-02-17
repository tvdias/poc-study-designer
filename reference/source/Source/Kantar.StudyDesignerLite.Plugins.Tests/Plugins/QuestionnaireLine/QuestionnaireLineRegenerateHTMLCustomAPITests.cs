namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeXrmEasy;
    using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;

    [TestClass]
    public class QuestionnaireLineRegenerateHTMLCustomAPITests
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
            var question = new QuestionnaireLineBuilder(project)
                .WithRowSortOrder(KTR_SortOrder.Alphabetical)
                .WithColumnSortOrder(KTR_SortOrder.Random)
                .WithAnswerMin(5)
                .WithAnswerMax(10)
                .WithQuestionFormatDetails("Some format details")
                .WithScripterNotes("Some scripter notes")
                .WithCustomNotes("Custom note")
                .Build();

            var existingAnswers = new List<Entity>
            {
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Row 1")
                    .WithAnswerCode("Row1A")
                    .WithDisplayOrder(1)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Row 2")
                    .WithAnswerCode("Row2B")
                    .WithDisplayOrder(2)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Column 1")
                    .WithAnswerCode("Column1A")
                    .WithDisplayOrder(3)
                    .WithAnswerType(KTR_AnswerType.Column)
                    .Build(),
            };

            var newColumn = new QuestionnaireLinesAnswerListBuilder(question)
                .WithAnswerText("Column 2")
                .WithAnswerCode("Column2B")
                .WithDisplayOrder(4)
                .WithAnswerType(KTR_AnswerType.Column)
                .Build();

            _context.Initialize(new List<Entity> { project, question }.Concat(existingAnswers).Append(newColumn));

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.InputParameters["projectId"] = project.Id;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineRegenerateHTMLCustomAPI>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault(x => x.Id == question.Id);
           
            // Assert HTML
            var updatedQuestion = _context.Data[KT_QuestionnaireLines.EntityLogicalName][question.Id];
            var html = updatedQuestion.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_AnswerList);
            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Row 1"));
            Assert.IsTrue(html.Contains("Row1A"));
            Assert.IsTrue(html.Contains("Row 2"));
            Assert.IsTrue(html.Contains("Row2B"));
            Assert.IsTrue(html.Contains("Column 1"));
            Assert.IsTrue(html.Contains("Column1A"));
            Assert.IsTrue(html.Contains("Column2B"));
            Assert.IsTrue(html.Contains("Column 2"));
            Assert.AreEqual(2, CountOccurrences(html, "<table", StringComparison.OrdinalIgnoreCase));
        }

        private int CountOccurrences(string source, string substring, StringComparison comparison)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(substring, index, comparison)) != -1)
            {
                count++;
                index += substring.Length;
            }
            return count;
        }

    }
}
