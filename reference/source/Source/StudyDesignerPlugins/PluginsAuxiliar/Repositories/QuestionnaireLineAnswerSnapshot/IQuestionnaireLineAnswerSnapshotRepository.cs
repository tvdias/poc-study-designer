using System;
using System.Collections.Generic;
using Kantar.StudyDesignerLite.Plugins;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.QuestionnaireLineAnswerSnapshot
{
    public interface IQuestionnaireLineAnswerSnapshotRepository
    {
        /// <summary>
        /// Get all answer snapshots for the given questionnaire line snapshot IDs
        /// </summary>
        IDictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>> GetAnswersBySnapshotIds(IEnumerable<Guid> snapshotIds);
    }
}
