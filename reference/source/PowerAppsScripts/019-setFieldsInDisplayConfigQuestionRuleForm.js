/**
 * @file        019-setFieldsInDisplayConfigQuestionRuleForm.js
 * @description This fills the 'ConfigQuestion' field in 'ProductConfigQuestionDisplayRule' quick create form entity, based on the chosen 'ProductConfigQuestion'
 *
 * @date        2025-04-11
 * @version     1.0
 *
 * @usage       This script is invoked on load of the Quick create form of ProductConfigQuestionDisplayRule entity
 * @notes       
 */
var SetFields = (function (config) {

    // Product Config Question related:
    const productConfigQuestionField = "ktr_productconfigquestion";
    const productConfigQuestionLogicalTableName = "ktr_productconfigquestion";
    const configQuestionLookupField = "ktr_ruleconfigquestion";
 
    // Config Question related:
    const configQuestionLogicalTableName = "ktr_configurationquestion";

    function setProductConfigQuestionDisplayRuleForm(executionContext) {
        var formContext = executionContext.getFormContext();
        
        // Get the selected Product Config Question
        var productConfigQuestion = formContext.getAttribute(productConfigQuestionField).getValue();
        
        if (productConfigQuestion && productConfigQuestion.length > 0) {
            var productConfigQuestionId = productConfigQuestion[0].id.replace("{", "").replace("}", "");
            
            Xrm.WebApi.retrieveMultipleRecords(
                productConfigQuestionLogicalTableName, 
                `?$filter=ktr_productconfigquestionid eq ${productConfigQuestionId}
                &$select=_ktr_configurationquestion_value
                &$expand=ktr_ConfigurationQuestion($select=ktr_configurationquestionid,ktr_name)`
            ).then(
                function (result) {
                    if (result.entities.length > 0) {
                        setConfigQuestionAttribute(formContext, result);
                    }
                },
                function (error) {
                    console.log("Error retrieving Configuration Question: " + error.message);
                }
            );
        }
    }

    function setConfigQuestionAttribute(formContext, result) {
        var configurationQuestion = result.entities[0]['ktr_ConfigurationQuestion'];

        if (configurationQuestion) {
            formContext.getAttribute(configQuestionLookupField).setValue([
                {
                    id: configurationQuestion.ktr_configurationquestionid, 
                    name: configurationQuestion.ktr_name, 
                    entityType: configQuestionLogicalTableName 
                }
            ]);
        }
    }

    return {
        setProductConfigQuestionDisplayRuleForm: setProductConfigQuestionDisplayRuleForm,
    };
})({
    
  });



