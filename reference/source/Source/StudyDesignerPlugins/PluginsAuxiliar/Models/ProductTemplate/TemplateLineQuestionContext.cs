using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate
{
    internal class TemplateLineQuestionContext
    {
        public TemplateLineQuestionContext(
            KTR_ProductTemplateLine productTemplateLine,
            Guid questionId,
            Guid? moduleId,
            int displayOrder,
            DateTime createdOn)
        {
            ProductTemplateLine = productTemplateLine;
            QuestionId = questionId;
            ModuleId = moduleId;
            DisplayOrder = displayOrder;
            CreatedOn = createdOn;
            IsIncluded = false;
        }

        public KTR_ProductTemplateLine ProductTemplateLine { get; set; }

        public Guid QuestionId { get; set; }

        public Guid? ModuleId { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsIncluded { get; set; }
    }
}
