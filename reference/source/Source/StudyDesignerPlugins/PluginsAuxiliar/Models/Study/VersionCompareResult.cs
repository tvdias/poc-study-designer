using Kantar.StudyDesignerLite.Plugins;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Study
{
    public class VersionCompareResult
    {
        public KTR_ChangelogRelatedObject RelatedObject { get; set; }
        public IEnumerable<Guid> AddedEntityIds { get; set; }
        public IEnumerable<Guid> RemovedEntityIds { get; set; }
        public IEnumerable<Guid> CommonEntityIds { get; set; }
        public IList<FieldChangedResult> FieldsChangedResults { get; set; }
    }
}
