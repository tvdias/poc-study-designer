/**
 * @file        015-studyQuestionarreLines.js
 * @description Set to Filter Questionnaire Line lookup  and show questions from the Master Questionnaire which are not in study.
 *
 * @date        2025-04-02
 * @version     1.1
 *
 * @usage       This script is invoked on load of the Quick create of Study Questionnaire Line form.
 * @notes       This script uses Xrm Power Apps library
 */

var StudyQuestionnaireLineFilter = (function () {
    "use strict";
    const entity = {
        study: "kt_study",
        studyquestionnaireline: "ktr_studyquestionnaireline",
        questionnairelines: "kt_questionnairelines"
    };
    const attribute = {
        study: "ktr_study"
    };
    const control = {
        questionnaireline: "ktr_questionnaireline"
    };
    class StudyQuestionnaireLineFilter {
        static async onLoad(executionContext) {
            try {
                var formContext = executionContext.getFormContext();
                var questionnaireControl = formContext.getControl(control.questionnaireline);
                if (questionnaireControl) {
                    questionnaireControl.addPreSearch(async () => {
                        await StudyQuestionnaireLineFilter.filterQuestionnaireLine(executionContext);
                    });
                }
            } catch (error) {
                console.error("Error in onLoad:", error);
            }
        }

        static async filterQuestionnaireLine(executionContext) {
            try {
                var formContext = executionContext.getFormContext();
                var study = formContext.getAttribute(attribute.study)?.getValue();
                var studyId = study[0].id.replace(/[{}]/g, "");
                // Fetch Project ID & Existing Study Questionnaire Lines in a single batch request
                let [studyRecord, studyLines] = await Promise.all([
                    Xrm.WebApi.retrieveRecord(entity.study, studyId, "?$select=_kt_project_value"),
                    Xrm.WebApi.retrieveMultipleRecords(entity.studyquestionnaireline, `?$filter=_ktr_study_value eq ${studyId}`)
                ]);
                var projectId = studyRecord?._kt_project_value;
                if (!projectId) return(console.log("No project found."));

                var excludedQuestionnaireLines = studyLines.entities
                    .map(line => line._ktr_questionnaireline_value)
                    .filter(id => id); // Remove invalid values

                // FetchXML Query for Lookup Filtering
                var fetchXml = `<fetch version='1.0' mapping='logical' distinct='true'>
                    <entity name='kt_questionnairelines'>
                        <attribute name='kt_questionnairelinesid' />
                        <attribute name='ktr_project' />
                        <attribute name='kt_questionvariablename' />
                        <filter type='and'>
                            <condition attribute='ktr_project' operator='eq' value='${projectId}' />
                            <condition attribute='statecode' operator='eq' value='0' />
                            ${excludedQuestionnaireLines.length > 0 ? `
                            <condition attribute='kt_questionnairelinesid' operator='not-in'>
                                ${excludedQuestionnaireLines.map(id => `<value>${id}</value>`).join("")}
                            </condition>` : ""}
                        </filter>
                    </entity>
                </fetch>`;

                // Define View Layout XML
                var viewLayout = `<grid name='resultset' jump='kt_name' select='1' icon='1' preview='1' object='2'>
                    <row name='result' id='kt_questionnairelinesid'>
                       <cell name='kt_questionvariablename' width='300' />
                    </row>
                </grid>`;

                // Apply Custom View
                var questionnaireControl = formContext.getControl(control.questionnaireline);
                if (questionnaireControl) {
                    const viewId = "{00000000-0000-0000-0000-000000000002}"; // Unique GUID
                    questionnaireControl.addCustomView(viewId, entity.questionnairelines, "Filtered Questionnaire Lines", fetchXml, viewLayout, true);
                    
                }

            } catch (error) {
                console.error("Error filtering Questionnaire Line:", error);
            }
        }
    }

    return StudyQuestionnaireLineFilter;
})();
