using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Study
{
    public class FieldChangedResult
    {
        public Entity Entity { get; set; }
        public KTR_ChangelogFieldChanged FieldChanged { get; set; }
        public string FieldLogicalName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
