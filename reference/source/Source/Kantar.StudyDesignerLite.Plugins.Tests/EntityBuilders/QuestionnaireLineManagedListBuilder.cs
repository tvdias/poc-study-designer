using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLineManagedListBuilder
    {
        private readonly KTR_QuestionnaireLinesHaRedList _entity;

        public QuestionnaireLineManagedListBuilder(KT_Project project, KTR_ManagedList managedList, KT_QuestionnaireLines qline)
        {
            _entity = new KTR_QuestionnaireLinesHaRedList
            {
                Id = Guid.NewGuid(),
                StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Active,
                KTR_ProjectId = new EntityReference(KT_Project.EntityLogicalName, project.Id),
                KTR_ManagedList = new EntityReference(KTR_ManagedList.EntityLogicalName, managedList.Id),
                KTR_QuestionnaireLine = new EntityReference(KT_QuestionnaireLines.EntityLogicalName, qline.Id)
            };
        }

        public QuestionnaireLineManagedListBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public QuestionnaireLineManagedListBuilder WithLocation(KTR_Location location)
        {
            _entity.KTR_Location = location;
            return this;
        }

        public KTR_QuestionnaireLinesHaRedList Build()
        {
            return _entity;
        }
    }
}
