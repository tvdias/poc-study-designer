using Kantar.StudyDesignerLite.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.ProductTemplate
{
    public class TemplateLineQuestionResult
    {
        public KTR_ProductTemplateLine ProductTemplateLine { get; set; }

        public Guid QuestionId { get; set; }

        public Guid? ModuleId { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
