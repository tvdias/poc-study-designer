namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.SubsetEntitiesSnapshot
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;

    public interface ISubsetEntitiesSnapshotRepository
    {
        IList<KTR_StudySubsetEntitiesSnapshot> GetSubsetEntitiesSnapshots(IList<Guid> subsetDefinitionSnapshotIds);
        IDictionary<Guid, IList<KTR_StudySubsetEntitiesSnapshot>> GetSubsetEntitiesBySubsetDefinitions(IList<Guid> subsetDefinitionSnapshotIds);
        IList<KTR_StudySubsetEntitiesSnapshot> GetByStudyId(Guid studyId, string[] columns = null);
    }
}
