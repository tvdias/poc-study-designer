namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Subset
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;

    // Create for testability purposes
    public interface ISubsetRepository
    {
        List<Guid> BulkInsert(IList<KTR_SubsetDefinition> subsetDefinition);
        List<Guid> BulkInsertSubsetStudyAssociation(IList<KTR_StudySubsetDefinition> subsetDefinitionAssociations);
        void Delete(Guid subsetDefinitionId);
        void BulkDelete(IList<Guid> subsetDefinitionIds);

        void DeleteSubsetStudyAssociation(Guid subsetDefinitionAssociationId);
        void BulkDeleteSubsetStudyAssociation(IList<Guid> subsetDefinitionAssociationIds);
        IList<KTR_SubsetDefinition> GetByMasterStudyId(Guid studyId, string[] columns = null);
        List<KTR_StudySubsetDefinition> GetSubsetStudyAssociationByStudyId(Guid studyId, string[] columns = null);
        List<KTR_StudySubsetDefinition> GetSubsetAssociationBySubsetIds(Guid[] associationIds, string[] columns = null);

        List<Guid> BulkInsertSubsetEntities(IList<KTR_SubsetEntities> subsetEntities);
        List<KTR_SubsetEntities> GetSubsetEntitiesByDefinitionIds(Guid[] subsetIds, string[] columns = null);
        List<KTR_SubsetEntities> GetSubsetEntitiesByMLEntityIds(Guid[] mlEntityIds, string[] columns = null);
        void DeleteSubsetEntity(Guid subsetEntityId);
        void BulkDeleteSubsetEntity(IList<Guid> subsetEntityIds);

        List<Guid> BulkInsertQLSubsets(IList<KTR_QuestionnaireLineSubset> qlSubsets);
        List<KTR_QuestionnaireLineSubset> GetQLSubsetsByStudyId(Guid studyId, string[] columns = null);
        void DeleteQLSubset(Guid qlSubsetId);
        void BulkDeleteQLSubset(IList<Guid> qlSubsetIds);

        IEnumerable<KTR_SubsetDefinition> GetStudySubsets(Guid studyId);
        IDictionary<Guid, IList<KTR_SubsetEntities>> GetSubsetEntitiesBySubsetDefinitions(IEnumerable<Guid> subsetDefinitionIds);
        IDictionary<Guid, IList<QuestionnaireLineSubsetWithLocation>> GetQuestionnaireLineSubsetsWithLocation(Guid studyId);
        IDictionary<Guid, IList<SubsetEntityWithManagedListEntity>> GetSubsetEntitiesWithManagedListInfo(IEnumerable<Guid> subsetDefinitionIds);
        List<string> GetSubsetNamesByQuestionnaireLineId(Guid questionnaireLineId, Guid? justCreatedSubsetDefinitionId);
    }
}
