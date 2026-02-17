namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetDefinitionSnapshot
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    public interface ISubsetDefinitionSnapshotRepository
    {
        IList<KTR_StudySubsetDefinitionSnapshot> GetStudySubsetSnapshots(Guid studyId);
        IList<KTR_StudySubsetDefinitionSnapshot> GetByStudyId(Guid studyId, string[] columns = null);
    }
}
