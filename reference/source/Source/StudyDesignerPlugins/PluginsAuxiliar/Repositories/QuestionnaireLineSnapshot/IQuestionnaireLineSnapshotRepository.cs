namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineSnapshot
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;

    public interface IQuestionnaireLineSnapshotRepository
    {
        IEnumerable<KTR_StudyQuestionnaireLineSnapshot> GetStudyQuestionnaireLineSnapshots(Guid studyId);
    }
}
