/**
 * @file        017-setConfigQuestionInDependencyRuleAnswerForm.js
 * @description This fills the 'ConfigQuestion' field in 'DependencyRuleAnswer' quick create form entity, based on the chosen 'DependencyRule'
 *
 * @date        2025-04-02
 * @version     1.0
 *
 * @usage       This script is invoked on load of the Quick create form of DependencyRuleAnswer entity
 * @notes       
 */
var SetConfigQuestion = (function (config) {

    function setDependencyRuleAnswerForm(executionContext) {

        // Dependency Rule Answer related:
        const dependencyRuleField = "ktr_dependencyrule";
    
        // Dependency Rule related:
        const dependencyRuleLogicalTableName = "ktr_dependencyrule";
        const configQuestionLookupField = "ktr_configurationquestion";
    
        var formContext = executionContext.getFormContext();
        
        // Get the selected Dependency Rule
        var dependencyRule = formContext.getAttribute(dependencyRuleField).getValue();
        
        if (dependencyRule && dependencyRule.length > 0) {
            var dependencyRuleId = dependencyRule[0].id.replace("{", "").replace("}", "");
            
            Xrm.WebApi.retrieveMultipleRecords(
                dependencyRuleLogicalTableName, 
                `?$filter=ktr_dependencyruleid eq ${dependencyRuleId}
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
        setDependencyRuleAnswerForm: setDependencyRuleAnswerForm,
    };
})({
    
  });



