using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class QuestionnaireLineBuilder
    {
        private readonly KT_QuestionnaireLines _entity;

        public QuestionnaireLineBuilder(KT_Project project)
        {
            _entity = new KT_QuestionnaireLines
            {
                Id = Guid.NewGuid(),
                StateCode = KT_QuestionnaireLines_StateCode.Active,
                StatusCode = KT_QuestionnaireLines_StatusCode.Active,
                KTR_Project = new EntityReference(project.LogicalName, project.Id),
            };
        }
       
        public QuestionnaireLineBuilder()
        {
            _entity = new KT_QuestionnaireLines
            {
                Id = Guid.NewGuid(),
                StateCode = KT_QuestionnaireLines_StateCode.Active,
                StatusCode = KT_QuestionnaireLines_StatusCode.Active
            };
        }

        public QuestionnaireLineBuilder WithScripterNotes(string scripterNotes)
        {
            _entity.KTR_ScripterNotes = scripterNotes;
            return this;
        }

        public QuestionnaireLineBuilder WithModule(KT_Module module)
        {
            _entity.KTR_Module = new EntityReference(module.LogicalName, module.Id);
            return this;
        }

        public QuestionnaireLineBuilder WithVariableName(string name)
        {
            _entity.KT_QuestionVariableName = name;
            return this;
        }

        public QuestionnaireLineBuilder WithStandardOrCustom(KT_QuestionnaireLines_KT_StandardOrCustom custom)
        {
            _entity.KT_StandardOrCustom = custom;
            return this;
        }

        public QuestionnaireLineBuilder WithXmlVariableName(string xmlVariableName)
        {
            _entity.KTR_XmlVariableName = xmlVariableName;
            return this;
        }

        public QuestionnaireLineBuilder WithSortOrder(int sortOrder)
        {
            _entity.KT_QuestionSortOrder = sortOrder;
            return this;
        }
        public QuestionnaireLineBuilder WithIsDummyQuestion(bool isDummy)
        {
            _entity["ktr_isdummyquestion"] = isDummy;
            return this;
        }
        public QuestionnaireLineBuilder WithState(int state)
        {
            if (state == 0) // Active
            {
                _entity.StateCode = KT_QuestionnaireLines_StateCode.Active;
                _entity.StatusCode = KT_QuestionnaireLines_StatusCode.Active;
            }
            else if (state == 1) // Inactive
            {
                _entity.StateCode = KT_QuestionnaireLines_StateCode.Inactive;
                _entity.StatusCode = KT_QuestionnaireLines_StatusCode.Inactive;
            }
            return this;
        }

        public QuestionnaireLineBuilder WithId(Guid id)
        {
            _entity.Id = id;
            return this;
        }

        public QuestionnaireLineBuilder WithCustomNotes(string customNotes)
        {
            _entity.KTR_CustomNotes = customNotes;
            return this;
        }

        public QuestionnaireLineBuilder WithQuestionFormatDetails(string qfd)
        {
            _entity.KTR_QuestionFormatDetails = qfd;
            return this;
        }

        public QuestionnaireLineBuilder WithAnswerMin(int min)
        {
            _entity.KTR_AnswerMin = min;
            return this;
        }

        public QuestionnaireLineBuilder WithAnswerMax(int max)
        {
            _entity.KTR_AnswerMax = max;
            return this;
        }

        public QuestionnaireLineBuilder WithRowSortOrder(KTR_SortOrder rowSortOrder)
        {
            _entity.KTR_RowSortOrder = rowSortOrder;
            return this;
        }

        public QuestionnaireLineBuilder WithColumnSortOrder(KTR_SortOrder colSortOrder)
        {
            _entity.KTR_ColumnSortOrder = colSortOrder;
            return this;
        }

        public QuestionnaireLineBuilder WithScriptletLookup(KTR_Scriptlets scriptlet)
        {
            _entity.KTR_ScriptletsLookup = new EntityReference(scriptlet.LogicalName, scriptlet.Id);
            return this;
        }

        public QuestionnaireLineBuilder WithScriptletInput(string scriptletinput)
        {
            _entity.KTR_Scriptlets = scriptletinput;
            return this;
        }

        public QuestionnaireLineBuilder WithCreatedOn(DateTime dateTime)
        {
            // hard-coded because CreatedOn is read-only 
            _entity["CreatedOn"] = dateTime;
            return this;
        }

        public QuestionnaireLineBuilder WithQuestionBank(KT_QuestionBank question)
        {
            _entity.KTR_QuestionBank = new EntityReference(question.LogicalName, question.Id);
            return this;
        }

        public QuestionnaireLineBuilder WithStatusCode(KT_QuestionnaireLines_StatusCode statusCode)
        {
            _entity.StatusCode = statusCode;
            return this;
        }

        public QuestionnaireLineBuilder WithEditCustomAnswerCode(bool editCustomAnswerCode)
        {
            _entity.KTR_EditCustomAnswerCode = editCustomAnswerCode;
            return this;
        }

        public KT_QuestionnaireLines Build()
        {
            return _entity;
        }
    }
}
