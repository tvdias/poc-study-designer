/**
 * @file        016-setConfigQuestionInProjectConfigAnswerForm.js
 * @description This fills the 'ConfigQuestion' field in 'ProjectConfigQuestionAnswer' quick create form entity, based on the chosen 'ProjectConfigQuestion'
 *
 * @date        2025-04-02
 * @version     1.0
 *
 * @usage       This script is invoked on load of the Quick create form of ProjectConfigQuestionAnswer entity
 * @notes       
 */
var SetConfigQuestion = (function (config) {

    function setProjectConfigAnswerForm(executionContext) {

        // Project Config Question Answer related:
        const projectConfigQuestionField = "ktr_projectproductconfigquestion";
    
        // Project Config Question related:
        const projectConfigQuestionLogicalTableName = "ktr_projectproductconfig";
        const configQuestionLookupField = "ktr_configurationquestion";
    
        var formContext = executionContext.getFormContext();
        
        // Get the selected Project Config Question
        var projectConfigQuestion = formContext.getAttribute(projectConfigQuestionField).getValue();
        
        if (projectConfigQuestion && projectConfigQuestion.length > 0) {
            var projectConfigQuestionId = projectConfigQuestion[0].id.replace("{", "").replace("}", "");
            
            Xrm.WebApi.retrieveMultipleRecords(
                projectConfigQuestionLogicalTableName, 
                `?$filter=ktr_projectproductconfigid eq ${projectConfigQuestionId}
                &$select=_ktr_configurationquestion_value
                &$expand=ktr_ConfigurationQuestion($select=ktr_configurationquestionid,ktr_name)`
            ).then(
                function (result) {
                    if (result.entities.length > 0) {
                        var configurationQuestion = result.entities[0]['ktr_ConfigurationQuestion'];
    
                        if (configurationQuestion) {
                            formContext.getAttribute(configQuestionLookupField).setValue([
                                {
                                    id: configurationQuestion.ktr_configurationquestionid, 
                                    name: configurationQuestion.ktr_name, 
                                    entityType: configQuestionLookupField 
                                }
                            ]);
                        }
                    }
                },
                function (error) {
                    console.log("Error retrieving Configuration Question: " + error.message);
                }
            );
        }
    }

    return {
        setProjectConfigAnswerForm: setProjectConfigAnswerForm,
    };
})({
    
  });



