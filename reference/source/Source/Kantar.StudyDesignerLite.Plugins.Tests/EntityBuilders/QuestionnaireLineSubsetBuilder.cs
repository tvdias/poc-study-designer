using System;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLineSubsetBuilder
    {
        private readonly KTR_QuestionnaireLineSubset _entity;

        public QuestionnaireLineSubsetBuilder()
        {
            _entity = new KTR_QuestionnaireLineSubset
            {
                Id = Guid.NewGuid()
            };

        }

        public QuestionnaireLineSubsetBuilder WithName(string name)
        {
            _entity[KTR_QuestionnaireLineSubset.Fields.KTR_Name] = name;
            return this;
        }
        public QuestionnaireLineSubsetBuilder WithSubsetDefinition(Entity subsetDefinition)
        {
            if (subsetDefinition == null) { throw new ArgumentNullException(nameof(subsetDefinition)); }
            _entity[KTR_QuestionnaireLineSubset.Fields.KTR_SubsetDefinitionId]
                = new EntityReference(subsetDefinition.LogicalName, subsetDefinition.Id);
            return this;
        }
        public QuestionnaireLineSubsetBuilder WithUsesFullList(bool usesFullList)
        {
            _entity[KTR_QuestionnaireLineSubset.Fields.KTR_UsesFullList] = usesFullList;
            return this;
        }
        public QuestionnaireLineSubsetBuilder WithStudy(KT_Study study)
        {
            _entity[KTR_QuestionnaireLineSubset.Fields.KTR_Study] = new EntityReference(study.LogicalName, study.Id);
            return this;
        }
        public QuestionnaireLineSubsetBuilder WithQuestionnaireLine(KT_QuestionnaireLines qLines)
        {
            _entity[KTR_QuestionnaireLineSubset.Fields.KTR_QuestionnaireLineId] = new EntityReference(qLines.LogicalName, qLines.Id);
            return this;
        }
        public KTR_QuestionnaireLineSubset Build()
        {
            return _entity;
        }
    }
}
