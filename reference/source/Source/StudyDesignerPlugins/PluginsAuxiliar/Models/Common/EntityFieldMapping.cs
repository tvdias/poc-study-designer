using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Common
{
    public class EntityFieldMapping
    {
        public string LookupField { get; set; }
        public string LookupEntityNameField { get; set; }
        public string TargetEntityNameField { get; set; }
    }
}
