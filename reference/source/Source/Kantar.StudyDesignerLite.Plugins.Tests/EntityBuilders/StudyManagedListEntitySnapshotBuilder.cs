namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xrm.Sdk;
    using Kantar.StudyDesignerLite.Plugins;

    /// <summary>
    /// Builder for KTR_StudyQuestionManagedListSnapshot entities for unit tests.
    /// </summary>
    public class StudyManagedListEntitySnapshotBuilder
    {
        private readonly KTR_StudyManagedListEntitiesSnapshot _entity;

        public StudyManagedListEntitySnapshotBuilder(KTR_StudyQuestionnaireLineSnapshot studyQlSnapshot)
        {
            _entity = new KTR_StudyManagedListEntitiesSnapshot
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_StudyManagedListEntitiesSnapshot_StateCode.Active,
                StatusCode = KTR_StudyManagedListEntitiesSnapshot_StatusCode.Active,
                KTR_QuestionnaireLinesNaPsHot = studyQlSnapshot?.ToEntityReference()
            };
        }

        public StudyManagedListEntitySnapshotBuilder WithQuestionnaireLineManagedListEntity(KTR_QuestionnaireLinemanAgedListEntity qLmLEntity)
        {
            _entity.KTR_QuestionnaireLinemanAgedListEntity = new EntityReference(KTR_QuestionnaireLinemanAgedListEntity.EntityLogicalName, qLmLEntity.Id);
            return this;
        }

        public StudyManagedListEntitySnapshotBuilder WithStudyQuestionManagedListSnapshot(KTR_StudyQuestionManagedListSnapshot qmlSnapshot)
        {
            if (qmlSnapshot != null)
            {
                _entity.KTR_StudyQuestionManagedListSnapshot =
                    new EntityReference(KTR_StudyQuestionManagedListSnapshot.EntityLogicalName, qmlSnapshot.Id);
            }
            return this;
        }

        public StudyManagedListEntitySnapshotBuilder WithManagedListEntity(KTR_ManagedListEntity managedListEntity)
        {
            if (managedListEntity != null)
            {
                _entity.KTR_ManagedListEntity =
                    new EntityReference(KTR_ManagedListEntity.EntityLogicalName, managedListEntity.Id);
            }
            return this;
        }
        public StudyManagedListEntitySnapshotBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public StudyManagedListEntitySnapshotBuilder WithDisplayOrder(int displayOrder)
        {
            _entity.KTR_DisplayOrder = displayOrder;
            return this;
        }

        public KTR_StudyManagedListEntitiesSnapshot Build()
        {
            return _entity;
        }
    }
}
