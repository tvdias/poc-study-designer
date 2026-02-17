namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLine
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Repository interface for KT_QuestionnaireLines entity operations.
    /// </summary>
    public interface IQuestionnaireLineRepository
    {
        /// <summary>
        /// Gets questionnaire lines by project ID.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <param name="columns">Optional columns to retrieve.</param>
        /// <returns>Collection of questionnaire lines for the project.</returns>
        IList<KT_QuestionnaireLines> GetQuestionnaireLinesByProjectId(Guid projectId, string[] columns = null);

        /// <summary>
        /// Gets questionnaire lines associated with a specific study.
        /// </summary>
        /// <param name="studyId">The study ID.</param>
        /// <returns>Collection of questionnaire lines associated with the study.</returns>
        IEnumerable<KT_QuestionnaireLines> GetStudyQuestionnaireLines(Guid studyId);
    }
}