namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    // Create for testability purposes
    public interface IQuestionnaireLineManagedListEntityRepository
    {
        IList<KTR_QuestionnaireLinemanAgedListEntity> GetByStudyId(Guid studyId, string[] columns = null);
    }
}
