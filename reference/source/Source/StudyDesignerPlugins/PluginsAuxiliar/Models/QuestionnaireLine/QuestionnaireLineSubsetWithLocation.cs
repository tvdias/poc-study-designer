namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models
{
    using System;
    using Kantar.StudyDesignerLite.Plugins;

    public class QuestionnaireLineSubsetWithLocation
    {
        public Guid SubsetDefinitionId { get; set; }
        public string SubsetName { get; set; }
        public string Location { get; set; } // "Row" or "Column"
        public Guid QuestionnaireLineId { get; set; }
        public Guid? ManagedListId { get; set; }
        public string ManagedListName { get; set; }
    }
}