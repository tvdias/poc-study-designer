namespace Kantar.StudyDesignerLite.Plugins.Tests.PluginsAuxiliar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class XmlGenerationHelperTests
    {
        [TestMethod]
        public void XmlGenerationHelper_Success()
        {
            // Arrange
            var project = new ProjectBuilder().Build();
            var study = new StudyBuilder(project).Build();
            var questionnaireLine = new QuestionnaireLineBuilder().Build();
            var questionnaireLinesSnapshot = new StudyQuestionnaireLineSnapshotBuilder(study, questionnaireLine).Build();
            var questionnaireLineAnswer = new QuestionnaireLinesAnswerListBuilder(questionnaireLine).Build();
            var questionnaireLineAnswersSnapshot = new StudyQuestionnaireLineAnswerSnapshotBuilder(questionnaireLinesSnapshot, questionnaireLineAnswer).Build();

            var data = new StudyXmlData
            {
                Study = study,
                Project = project,
                Languages = null,
                Subsets = null,
                SubsetEntities = null,
                QuestionnaireLinesSnapshot = new List<KTR_StudyQuestionnaireLineSnapshot> { questionnaireLinesSnapshot },
                QuestionnaireLineAnswersSnapshot = new Dictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>>
                {
                    { Guid.NewGuid(), new List<KTR_StudyQuestionAnswerListSnapshot> { questionnaireLineAnswersSnapshot } }
                }
            };

            // Act
            var result = XmlGenerationHelper.GenerateStudyXml(data);

            // Assert
            Assert.IsNotNull(result);

            // Parse the XML to check structure
            var xml = XDocument.Parse(result);
            Assert.AreEqual("Study", xml.Root.Name.LocalName);
        }
        [TestMethod]
        public void AddLineBreaksAndFormat_CoversAllScenarios()
        {
            // Arrange
            string input = "“Hello” ‘World’!\r\nThis is a test.  \r\nNew line after CRLF.\nAnother line.\rCarriage return.\u0085Next line.\u2028Line separator.\u2029Paragraph separator."
                + @"\r\nVerbatim CRLF.\nVerbatim LF.\rVerbatim CR.\u0085Verbatim NextLine.\u2028Verbatim LineSep.\u2029Verbatim ParaSep."
                + "<b>Bold</b> <i>Italic</i> <u>Underline</u> <strong>Strong</strong> <em>Emphasis</em> <script>alert('x')</script>"
                + "Sentence one. Sentence two! Sentence three?And after.";

            string dissallowedTag = "&lt;script&gt;alert(&#39;x&#39;)&lt;/script&gt;";

            // Act
            string result = HtmlGenerationHelper.AddLineBreaksAndFormat(input);

            // Assert
            // 1. Smart quotes replaced
            Assert.IsFalse(result.Contains("“") || result.Contains("”") || result.Contains("‘") || result.Contains("’"));

            // 2. Verbatim newlines replaced
            Assert.IsFalse(result.Contains(@"\r\n") || result.Contains(@"\n") || result.Contains(@"\r"));

            // 3. Allowed style tags preserved
            Assert.IsTrue(result.Contains("<b>Bold</b>"));
            Assert.IsTrue(result.Contains("<i>Italic</i>"));
            Assert.IsTrue(result.Contains("<u>Underline</u>"));
            Assert.IsTrue(result.Contains("<strong>Strong</strong>"));
            Assert.IsTrue(result.Contains("<em>Emphasis</em>"));

            // 4. Disallowed tags encoded
            Assert.IsTrue(!result.Contains(dissallowedTag));

            // 5. Line endings normalized to <br/>
            Assert.IsTrue(result.Contains("<br/>New line after CRLF."));
            Assert.IsTrue(result.Contains("<br/>Another line."));
            Assert.IsTrue(result.Contains("<br/>Carriage return."));
            Assert.IsTrue(result.Contains("<br/>Next line."));
            Assert.IsTrue(result.Contains("<br/>Line separator."));

            // 7. Sentence spacing formatting
            Assert.IsTrue(result.Contains(".<br/><br/>Sentence two!<br/><br/>Sentence three?<br/><br/>And after."));

            // 8. Output wrapped in <p> tags
            Assert.IsTrue(result.StartsWith("<p>") && result.EndsWith("</p>"));
        }
    }
}
