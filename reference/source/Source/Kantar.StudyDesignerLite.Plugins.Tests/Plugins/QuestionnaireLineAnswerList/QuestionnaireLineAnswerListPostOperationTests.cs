using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.QuestionnaireLine;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.QuestionnaireLine
{
    [TestClass]
    public class QuestionnaireLineAnswerListPostOperationTests
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;
        private Mock<ITracingService> _tracingService;

        [TestInitialize]
        public void Initialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
            _tracingService = new Mock<ITracingService>();
        }

        [TestMethod]
        public void ExecutePlugin_HtmlAndXMLListGenerated()
        {
            var question = new QuestionnaireLineBuilder().Build();
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

            _context.Initialize(new List<Entity> { question }.Concat(existingAnswers).Append(newColumn));

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newColumn;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPostOperation>(pluginContext);

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
        [TestMethod]
        public void ExecutePlugin_IsFixedIsExecutedIsOpen_HtmlAndXMLListGenerated()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var question = new KT_QuestionnaireLines
            {
                Id = questionId,
                KT_QuestionType = KT_QuestionType.SingleChoiceMatrix,
            };

            var existingAnswers = new List<Entity>
            {
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Row 1")
                    .WithDisplayOrder(1)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .WithIsExclusive(true)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Row 2")
                    .WithDisplayOrder(2)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .WithIsFixed(true)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Column 1")
                    .WithDisplayOrder(3)
                    .WithAnswerType(KTR_AnswerType.Column)
                    .WithIsFixed(true)
                    .WithIsExclusive(true)
                    .Build(),
            };

            var newColumn = new QuestionnaireLinesAnswerListBuilder(question)
                .WithAnswerText("Column 2")
                .WithDisplayOrder(4)
                .WithAnswerType(KTR_AnswerType.Column)
                .WithIsExclusive (true)
                .WithIsFixed (true)
                .WithIsOpen(true)
                .Build();

            _context.Initialize(new List<Entity> { question }.Concat(existingAnswers).Append(newColumn));

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newColumn;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPostOperation>(pluginContext);

            // Assert HTML
            var updatedQuestion = _context.Data[KT_QuestionnaireLines.EntityLogicalName][question.Id];
            var html = updatedQuestion.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_AnswerList);
            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Row 1"));
            Assert.IsTrue(html.Contains("Row 2"));
            Assert.IsTrue(html.Contains("Column 1"));
            Assert.IsTrue(html.Contains("Column 2"));
            Assert.AreEqual(2, CountOccurrences(html, "<table", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(html.Contains("FIXED"));
            Assert.IsTrue(html.Contains("EXCLUSIVE"));
            Assert.IsTrue(html.Contains("OPEN"));
           
            int tdCount = CountOccurrences(html, "<td", StringComparison.OrdinalIgnoreCase);
            int trCount = CountOccurrences(html, "<tr", StringComparison.OrdinalIgnoreCase);

            // Assert each row has 3 columns
            Assert.AreEqual(trCount * 3, tdCount);

        }

        [TestMethod]
        public void ExecutePlugin_QuestionTypeStandard_HtmlAndXMLListGenerated()
        {
            // Arrange
            var questionId = Guid.NewGuid();
            var question = new KT_QuestionnaireLines
            {
                Id = questionId,
                KT_QuestionType = KT_QuestionType.NumericInput
            };

            var answers = new List<Entity>
            {
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Answer A")
                    .WithDisplayOrder(1)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Answer B")
                    .WithDisplayOrder(2)
                    .WithAnswerType(KTR_AnswerType.Row)
                    .Build(),
            };

            var newAnswer = new QuestionnaireLinesAnswerListBuilder(question)
                .WithAnswerText("Answer C")
                .WithDisplayOrder(3)
                .WithAnswerType(KTR_AnswerType.Row)
                .Build();

            _context.Initialize(new List<Entity> { question }.Concat(answers).Append(newAnswer));

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = newAnswer;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPostOperation>(pluginContext);

            // Assert HTML
            var updatedQuestion = _context.Data[KT_QuestionnaireLines.EntityLogicalName][question.Id];
            var html = updatedQuestion.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_AnswerList);
            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Answer A"));
            Assert.IsTrue(html.Contains("Answer B"));
            Assert.IsTrue(html.Contains("Answer C"));
        }

        [TestMethod]
        public void Execute_OnlyColumns_GeneratesOnlyColumnTable()
        {
            // Arrange
            var question = new QuestionnaireLineBuilder()
                .Build();

            var existingAnswers = new List<Entity>
            {
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Column A")
                    .WithDisplayOrder(1)
                    .WithAnswerType(KTR_AnswerType.Column)
                    .Build(),
                new QuestionnaireLinesAnswerListBuilder(question)
                    .WithAnswerText("Column B")
                    .WithDisplayOrder(2)
                    .WithAnswerType(KTR_AnswerType.Column)
                    .Build()
            };

            _context.Initialize(new List<Entity> { question }.Concat(existingAnswers));

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = "Create";
            pluginContext.InputParameters["Target"] = existingAnswers.Last(); // Simulate new column creation

            // Act
            _context.ExecutePluginWith<QuestionnaireLineAnswerListPostOperation>(pluginContext);

            // Assert: HTML
            var updatedQuestion = _context.Data[KT_QuestionnaireLines.EntityLogicalName][question.Id];
            var html = updatedQuestion.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_AnswerList);

            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Column A"));
            Assert.IsTrue(html.Contains("Column B"));
            Assert.IsTrue(CountOccurrences(html, "<table", StringComparison.OrdinalIgnoreCase) == 1, "Only one table (for columns) should be generated");
            Assert.IsFalse(html.Contains("Row"), "No row table or row label should be present");

        }

        [TestMethod]
        public void ExecutePlugin_QuestionWithMLs_HtmlGenerated()
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

            var ml1 = new ManagedListBuilder(project)
                    .WithName("ML 1")
                    .Build();
            var ml2 = new ManagedListBuilder(project)
                    .WithName("ML 2")
                    .Build();
            var ml3 = new ManagedListBuilder(project)
                    .WithName("ML 3")
                    .Build();

            var ml1AsRow = new QuestionnaireLineManagedListBuilder(project, ml1, question)
                .WithLocation(KTR_Location.Row)
                .Build();
            var ml2AsRow = new QuestionnaireLineManagedListBuilder(project, ml2, question)
                .WithLocation(KTR_Location.Row)
                .Build();

            var ml3AsColumn = new QuestionnaireLineManagedListBuilder(project, ml3, question)
                .WithLocation(KTR_Location.Column)
                .Build();

            _context.Initialize(new List<Entity> { project, question, ml1, ml2, ml3, ml1AsRow, ml2AsRow, ml3AsColumn });

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.InputParameters["questionnaireLineId"] = question.Id;

            // Act
            _context.ExecutePluginWith<QuestionnaireLineRegenerateXMLandHTMLCustomAPI>(pluginContext);

            // Assert
            var updated = _context.CreateQuery<KT_QuestionnaireLines>().FirstOrDefault(x => x.Id == question.Id);
            Assert.IsTrue(updated.Attributes.ContainsKey(KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput));
            Assert.IsTrue(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Row Sort Order"));
            Assert.IsTrue(updated[KT_QuestionnaireLines.Fields.KTR_ScripterNotesOutput].ToString().Contains("Custom Notes"));

            // Assert HTML
            var updatedQuestion = _context.Data[KT_QuestionnaireLines.EntityLogicalName][question.Id];
            var html = updatedQuestion.GetAttributeValue<string>(KT_QuestionnaireLines.Fields.KTR_AnswerList);
            Assert.IsNotNull(html);
            Assert.IsTrue(html.Contains("Rows"));
            Assert.IsTrue(html.Contains("Managed List: ML 1"));
            Assert.IsTrue(html.Contains("Managed List: ML 2"));
            Assert.IsTrue(html.Contains("Columns"));
            Assert.IsTrue(html.Contains("Managed List: ML 3"));
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
