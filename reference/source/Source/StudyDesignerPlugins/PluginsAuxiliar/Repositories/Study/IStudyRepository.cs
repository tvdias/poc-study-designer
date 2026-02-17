namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Study
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    // Create for testability purposes
    public interface IStudyRepository
    {
        KT_Study Get(Guid studyId, string[] columns = null);
        KT_Study Get(Guid studyId);
        List<KTR_StudyManagedListEntity> GetByStudyId(Guid studyId, string[] columns = null);
        void UpdateStudyXml(Guid studyId, string xmlContent);
    }
}
