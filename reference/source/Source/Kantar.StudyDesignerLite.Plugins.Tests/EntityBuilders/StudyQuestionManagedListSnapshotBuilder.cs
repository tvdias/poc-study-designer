namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    using System;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;

    /// <summary>
    /// Builder for KTR_StudyQuestionManagedListSnapshot entities for unit tests.
    /// </summary>
    public class StudyQuestionManagedListSnapshotBuilder
    {
        private readonly KTR_StudyQuestionManagedListSnapshot _entity;

        public StudyQuestionManagedListSnapshotBuilder(KTR_StudyQuestionnaireLineSnapshot studyQlSnapshot)
        {
            _entity = new KTR_StudyQuestionManagedListSnapshot
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_StudyQuestionManagedListSnapshot_StateCode.Active,
                StatusCode = KTR_StudyQuestionManagedListSnapshot_StatusCode.Active,
                KTR_QuestionnaireLinesNaPsHot = studyQlSnapshot?.ToEntityReference(),
                KTR_QuestionnaireLineManagedList = new EntityReference(KTR_QuestionnaireLinesHaRedList.EntityLogicalName, Guid.NewGuid())
            };
        }

        public StudyQuestionManagedListSnapshotBuilder WithManagedList(KTR_ManagedList managedList)
        {
            _entity.KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, managedList.Id);
            return this;
        }

        public StudyQuestionManagedListSnapshotBuilder WithQuestionnaireLineManagedList(KTR_QuestionnaireLinesHaRedList questionnaireManagedList)
        {
            _entity.KTR_QuestionnaireLineManagedList = questionnaireManagedList == null ? null : new EntityReference(KTR_QuestionnaireLinesHaRedList.EntityLogicalName, questionnaireManagedList.Id);
            return this;
        }

        public StudyQuestionManagedListSnapshotBuilder WithStatusCode(KTR_StudyQuestionManagedListSnapshot_StatusCode statusCode)
        {
            _entity.StatusCode = (KTR_StudyQuestionManagedListSnapshot_StatusCode?)statusCode;
            return this;
        }

        public StudyQuestionManagedListSnapshotBuilder WithStateCode(KTR_StudyQuestionManagedListSnapshot_StateCode stateCode)
        {
            _entity.StateCode = (KTR_StudyQuestionManagedListSnapshot_StateCode?)stateCode;
            return this;
        }

        public StudyQuestionManagedListSnapshotBuilder WithQuestionnaireLineSnapshot(KTR_StudyQuestionnaireLineSnapshot snapshot)
        {
            _entity.KTR_QuestionnaireLinesNaPsHot = snapshot?.ToEntityReference();
            return this;
        }

        public StudyQuestionManagedListSnapshotBuilder WithLocation(KTR_Location location)
        {
            _entity.KTR_Location = location;
            return this;
        }

        public KTR_StudyQuestionManagedListSnapshot Build()
        {
            return _entity;
        }
    }
}
