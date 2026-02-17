namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models
{
    using System;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Model representing a subset entity with its related managed list entity information.
    /// </summary>
    public class SubsetEntityWithManagedListEntity
    {
        public Guid SubsetEntityId { get; set; }
        public Guid SubsetDefinitionId { get; set; }
        public Guid? ManagedListEntityId { get; set; }
        public string EntityCode { get; set; }
        public string EntityName { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
