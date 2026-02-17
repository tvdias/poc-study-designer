namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Microsoft.Xrm.Sdk;

    public class SubsetCreationContext
    {
        public SubsetCreationContext()
        { }

        public SubsetCreationContext(List<KTR_QuestionnaireLinemanAgedListEntity> entityByQL, KT_Study study)
        {
            var entity = GetEntity(entityByQL);
            this.MasterStudyId = study.KTR_MasterStudy is null ? study.Id : study.KTR_MasterStudy.Id;
            GenerateHash(entityByQL);
            this.SubsetDefinitionId = Guid.NewGuid();
            this.IsNewSubset = true;
            this.QuestionnaireLinemanAgedListEntities = entityByQL;
            this.KTR_ManagedListEntities = entityByQL.Select(e => e.KTR_ManagedListEntity).ToList();
            this.KTR_ManagedList = entity.KTR_ManagedList;
            this.EntityCount = entityByQL.Count();
            this.Study = study;

        }

        public Guid SubsetDefinitionId { get; set; }

        public Guid MasterStudyId { get; set; }

        public string SubsetName { get; set; }

        public string Hash { get; set; }
        public KT_Study Study { get; set; }

        public int EntityCount { get; set; }

        // Not calculated yet doing overhead requests
        public int UsageCount { get; set; } = -1;

        public bool IsFullList { get; set; }

        public bool IsReused { get; set; }

        /// <summary>
        /// QuestionnaireLinemanAgedListEntities used to create this Subset.
        /// </summary>
        public List<KTR_QuestionnaireLinemanAgedListEntity> QuestionnaireLinemanAgedListEntities { get; set; }

        /// <summary>
        /// Managed List associated with the QuestionnaireLinemanAgedListEntities
        /// </summary>
        public EntityReference KTR_ManagedList { get; set; }

        /// <summary>
        /// Managed List Entities associated with the QuestionnaireLinemanAgedListEntities
        /// </summary>
        public List<EntityReference> KTR_ManagedListEntities { get; set; }

        /// <summary>
        /// Questionnaire Line associated with this Subset from the QuestionnaireLinemanAgedListEntities
        /// </summary>
        public List<EntityReference> KTR_QuestionnaireLines { get; set; } = new List<EntityReference>();

        /// <summary>
        /// New Subset Entities that need to be created in the system.
        /// </summary>
        public List<KTR_SubsetEntities> NewSubsetEntities { get; set; } = new List<KTR_SubsetEntities>();

        /// <summary>
        /// Subset Entities that already exist in the system and should be reused.
        /// </summary>
        public List<KTR_SubsetEntities> ExistingSubsetEntities { get; set; } = new List<KTR_SubsetEntities>();

        /// <summary>
        /// New Questionnaire Line Subsets that need to be created in the system.
        /// </summary>
        public List<KTR_QuestionnaireLineSubset> NewQuestionnaireLineSubsets { get; set; } = new List<KTR_QuestionnaireLineSubset>();

        /// <summary>
        /// Questionnaire Line Subsets that already exist in the system and should be reused.
        /// </summary>
        public List<KTR_QuestionnaireLineSubset> ExistingQuestionnaireLineSubsets { get; set; } = new List<KTR_QuestionnaireLineSubset>();

        /// <summary>
        /// StudySubsetDefinition association for this Subset and Study.IsNewStudySubsetDefinitionAssociation indicates if it is new.
        /// </summary>
        public KTR_StudySubsetDefinition StudySubsetDefinitionAssociation { get; set; } = null;

        /// <summary>
        /// Indicates if the StudySubsetDefinition association is new and needs to be created.
        /// </summary>
        public bool IsNewStudySubsetDefinitionAssociation { get; set; } = false;

        /// <summary>
        /// Indicates if the SubsetDefinition is new and needs to be created.
        /// </summary>
        public bool IsNewSubset { get; set; } = false;

        public KTR_SubsetDefinition ToEntity()
        {
            return new KTR_SubsetDefinition
            {
                Id = SubsetDefinitionId,
                KTR_Name = SubsetName,
                KTR_FilterSignature = Hash,
                KTR_ManagedList = KTR_ManagedList,
                KTR_MasterStudyId = new EntityReference(KT_Study.EntityLogicalName, MasterStudyId),
                KTR_EntityCount = EntityCount,
                KTR_IsFullList = IsFullList,
                KTR_Project = Study.KT_Project,
                KTR_UsageCount = UsageCount,
            };
        }

        public SubsetCreationResponse ToResponse()
        {
            return new SubsetCreationResponse
            {
                SubsetId = SubsetDefinitionId,
                SubsetName = SubsetName,
                EntityCount = EntityCount,
                UsageCount = UsageCount,
                UsesFullList = IsFullList,
                IsReused = IsReused
            };
        }

        private KTR_QuestionnaireLinemanAgedListEntity GetEntity(List<KTR_QuestionnaireLinemanAgedListEntity> entities)
        {
            if (entities == null || entities.Count == 0)
            {
                return null;
            }
            if (entities.All(e => entities.First().KTR_QuestionnaireLine.Id == e.KTR_QuestionnaireLine.Id))
            {
                return entities.First();
            }
            else
            {
                throw new InvalidOperationException("KTR_QuestionnaireLinemanAgedListEntity contains multiple Questionnaire Lines");
            }
        }

        private void GenerateHash(IList<KTR_QuestionnaireLinemanAgedListEntity> entities)
        {
            var orderedIds = entities
                .Select(e => e.KTR_ManagedListEntity.Id)
                .OrderBy(id => id)
                .ToList();

            if (this.MasterStudyId == Guid.Empty)
            {
                throw new InvalidOperationException("Cannot generate hash for empty MasterStudyId");
            }

            var rawData = $"{this.MasterStudyId}|{entities.First().KTR_ManagedList.Id}|{string.Join("|", orderedIds)}";

            this.Hash = EncodeHelper.ComputeSha256Hash(rawData);
        }
    }
}
