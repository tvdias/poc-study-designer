namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset
{
    using System;

    public class SubsetCreationResponse
    {
        public Guid SubsetId { get; set; }

        public string SubsetName { get; set; }

        // Number of entities in the created subset (Managed List Entities)
        public int EntityCount { get; set; }

        // Number of times the subset repeatedly used in Questionnaire Lines
        public int UsageCount { get; set; }

        // Indicates if the subset uses the full list of entities
        public bool UsesFullList { get; set; }

        // Indicates if the subset already existed
        public bool IsReused { get; set; }
    }
}
