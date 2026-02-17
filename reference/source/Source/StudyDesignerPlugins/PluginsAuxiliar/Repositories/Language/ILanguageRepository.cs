namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Language
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Repository interface for KTR_Language entity operations.
    /// </summary>
    public interface ILanguageRepository
    {
        /// <summary>
        /// Gets languages associated with a specific study.
        /// </summary>
        /// <param name="studyId">The study ID.</param>
        /// <returns>Collection of languages associated with the study.</returns>
        IEnumerable<KTR_Language> GetStudyLanguages(Guid studyId);
    }
}