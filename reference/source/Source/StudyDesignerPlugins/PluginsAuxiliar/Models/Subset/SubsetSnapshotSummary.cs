namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset
{
    using System.Collections.Generic;

    public class SubsetSnapshotSummary
    {
        public string SubsetName { get; set; }

        public int QuestionCount { get; set; }

        public IList<SubsetSnapshotEntity> Entities { get; set; }
    }

    public class SubsetSnapshotEntity
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
}