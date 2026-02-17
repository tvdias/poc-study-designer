using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Product.ApplyTemplateSimulation
{
    public class ApplyTemplateSimulationRequest
    {
        public Guid ProductId { get; set; }

        public Guid ProductTemplateId { get; set; }

        public IList<ConfigurationQuestionRequest> ConfigurationQuestions { get; set; }
    }

    public class ConfigurationQuestionRequest
    {
        public Guid Id { get; set; }

        public IList<ConfigurationAnswerRequest> Answers { get; set; }
    }

    public class ConfigurationAnswerRequest
    { 
        public Guid Id { get; set; }
    }
}
