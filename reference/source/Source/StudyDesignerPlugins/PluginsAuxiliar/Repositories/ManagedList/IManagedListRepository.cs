namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Repository interface for KTR_ManagedList entity operations.
    /// </summary>
    public interface IManagedListRepository
    {
        /// <summary>
        /// Gets managed lists associated with a specific study.
        /// </summary>
        /// <param name="studyId">The study ID.</param>
        /// <returns>Collection of managed lists associated with the study.</returns>
        IEnumerable<KTR_ManagedList> GetStudyManagedLists(Guid studyId);

        /// <summary>
        /// Gets managed lists with their entities for a specific study.
        /// Only returns entities that are actually associated with the study.
        /// </summary>
        /// <param name="studyId">The study ID.</param>
        /// <returns>Dictionary of managed lists with their study-filtered entities grouped by managed list ID</returns>
        IDictionary<Guid, IList<KTR_ManagedListEntity>> GetStudyManagedListsWithEntities(Guid studyId);

        /// <summary>
        /// Gets managed lists by their IDs.
        /// </summary>
        /// <param name="managedListIds">The managed list IDs.</param>
        /// <returns>Collection of managed lists with the specified IDs</returns>
        IEnumerable<KTR_ManagedList> GetManagedListsByIds(IEnumerable<Guid> managedListIds);
    }
}
