using Kantar.StudyDesignerLite.Plugins;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product.ApplyTemplateSimulation
{
    public class ApplyTemplateSimulationResponse
    {
        public IList<QuestionResponse> Questions { get; set; }
    }

    public class QuestionResponse
    {
        public Guid Id { get; set; }

        public string QuestionTitle { get; set; }

        public int DisplayOrder { get; set; }

        public ModuleResponse Module { get; set; }

        public IList<AnswerResponse> Answers { get; set; }

        public KT_QuestionType? QuestionType { get; set; }

        public KT_SingleOrMultiCode? SingleOrMultiCoded { get; set; }

        public string QuestionText { get; set; }

        public int? AnswerMin { get; internal set; }
        
        public int? AnswerMax { get; internal set; }
        
        public string QuestionFormatDetails { get; internal set; }
        
        public string CustomNotes { get; internal set; }
        
        public string QuestionRationale { get; internal set; }
    }

    public class ModuleResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    public class AnswerResponse
    {
        public Guid Id { get; set; }

        public string Text { get; set; }
    }
}
