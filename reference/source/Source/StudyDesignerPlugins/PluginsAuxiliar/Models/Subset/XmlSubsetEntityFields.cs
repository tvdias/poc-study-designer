namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class XmlSubsetEntityFields
    {
        public Guid SubsetEntityId { get; set; }
        public Guid SubsetId { get; set; }
        public int DisplayOrder { get; set; }
        public string AnswerCode { get; set; }
        public string AnswerText { get; set; }
    }
}
