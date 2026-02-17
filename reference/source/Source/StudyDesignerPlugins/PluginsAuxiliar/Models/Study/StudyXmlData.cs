namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Data model containing all information needed for Study XML generation.
    /// </summary>
    public class StudyXmlData
    {
        public KT_Study Study { get; set; }
        public KT_Project Project { get; set; }
        public IEnumerable<KTR_Language> Languages { get; set; }
        public IEnumerable<KTR_ManagedList> ManagedLists { get; set; }
        public IEnumerable<KTR_ManagedListEntity> ManagedListEntities { get; set; }
        public IEnumerable<KTR_StudyQuestionnaireLineSnapshot> QuestionnaireLinesSnapshot { get; set; }
        public IDictionary<Guid, IList<KTR_StudyQuestionAnswerListSnapshot>> QuestionnaireLineAnswersSnapshot { get; set; }
        public IDictionary<Guid, IList<KTR_ManagedListEntity>> ManagedListEntitiesGrouped { get; set; }
        public IList<KTR_StudySubsetDefinitionSnapshot> Subsets { get; set; }
        public IList<KTR_StudySubsetEntitiesSnapshot> SubsetEntities { get; set; }
    }
}
