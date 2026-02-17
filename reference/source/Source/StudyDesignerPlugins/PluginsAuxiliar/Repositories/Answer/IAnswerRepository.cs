namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Answer
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    public interface IAnswerRepository
    {
        /// <summary>
        /// Gets all answers for the specified questionnaire lines in a study.
        /// </summary>
        /// <param name="questionnaireLineIds">The questionnaire line IDs to get answers for</param>
        /// <returns>Collection of answers grouped by questionnaire line</returns>
        IDictionary<Guid, IList<KTR_QuestionnaireLinesAnswerList>> GetAnswersByQuestionnaireLines(IEnumerable<Guid> questionnaireLineIds);
    }
}