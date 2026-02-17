using System;
using Microsoft.Xrm.Sdk;
using Kantar.StudyDesignerLite.Plugins;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLineSharedListBuilder
    {
        private readonly KTR_QuestionnaireLinesHaRedList _entity;

        public QuestionnaireLineSharedListBuilder()
        {
            _entity = new KTR_QuestionnaireLinesHaRedList
            {
                Id = Guid.NewGuid(),
                StateCode = KTR_QuestionnaireLinesHaRedList_StateCode.Active,
                StatusCode = KTR_QuestionnaireLinesHaRedList_StatusCode.Active
            };
        }

        public QuestionnaireLineSharedListBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithQuestionnaireLine(Guid questionnaireLineId)
        {
            _entity.KTR_QuestionnaireLine = new EntityReference("kt_questionnairelines", questionnaireLineId);
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithName(string name)
        {
            _entity.KTR_Name = name;
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithStatusCode(KTR_QuestionnaireLinesHaRedList_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithFieldValue(string fieldName, object value)
        {
            _entity.Attributes[fieldName] = value;
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithProject(KT_Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            _entity.KTR_ProjectId = new EntityReference(project.LogicalName, project.Id);
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithManagedList(KTR_ManagedList managedList)
        {
            if (managedList == null)
            {
                throw new ArgumentNullException(nameof(managedList));
            }

            _entity.KTR_ManagedList = new EntityReference(managedList.LogicalName, managedList.Id);
            return this;
        }

        public QuestionnaireLineSharedListBuilder WithLocation(int optionSetValue, string formattedValue = null)
        {
            _entity.Attributes[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location] = new OptionSetValue(optionSetValue);
            if (!string.IsNullOrWhiteSpace(formattedValue))
            {
                _entity.FormattedValues[KTR_QuestionnaireLinesHaRedList.Fields.KTR_Location] = formattedValue;
            }
            return this;
        }
        public KTR_QuestionnaireLinesHaRedList Build()
        {
            return _entity;
        }
    }
}
