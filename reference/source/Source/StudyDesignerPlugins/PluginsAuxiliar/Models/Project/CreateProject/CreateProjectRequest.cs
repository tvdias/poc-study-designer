using System;
using System.Collections.Generic;
using System.Linq;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Project.CreateProject
{
    public class CreateProjectRequest
    {
        public Guid ClientId { get; set; }

        public Guid CommissioningMarketId { get; set; }

        public string Description { get; set; }

        public Guid ProductId { get; set; }

        public Guid ProductTemplateId { get; set; }

        public string ProjectName { get; set; }

        public IList<QuestionRequest> Questions { get; set; }

        public List<ExistingQuestionRequest> GetExistingQuestions() =>
            Questions
            .Where(x => x.Origin == QuestionRequestOrigin.QuestionBank)
            .OfType<ExistingQuestionRequest>()
            .ToList();

        public List<NewQuestionRequest> GetNewQuestions() =>
            Questions
            .Where(x => x.Origin == QuestionRequestOrigin.New)
            .OfType<NewQuestionRequest>()
            .ToList();
    }

    public abstract class QuestionRequest
    {
        public QuestionRequestOrigin Origin { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    public class NewQuestionRequest : QuestionRequest
    {
        public string StandardOrCustom { get; set; }

        public string VariableName { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public string ScripterNotes { get; set; }

        public string QuestionRationale { get; set; }

        public string QuestionType { get; set; }

        public bool IsDummyQuestion { get; set; }

        public IList<AnswerRequest> Answers { get; set; } = new List<AnswerRequest>();
    }

    public class ExistingQuestionRequest : QuestionRequest
    {
        public Guid Id { get; set; }

        public ModuleRequest Module { get; set; }
    }

    public class ModuleRequest
    {
        public Guid Id { get; set; }
    }

    public class AnswerRequest
    {
        public string Name { get; set; }

        public string Text { get; set; }

        public string Location { get; set; }
    }

    public enum QuestionRequestOrigin
    {
        New,
        QuestionBank
    }
}
