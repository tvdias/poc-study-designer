/**
* @file        034-AutosaveScriptletQuickForm.js
* @description Will autosave Scriptlet main form that is present on Questionnaire line main form
*
* @date        2025-07-22
* @version     1.1
*
* @usage       This script is invoked on change of Scriptlet field on Scriptlet form
* @notes       
*/
function autoSaveScriptletifEdited(executionContext) {
    var formContext = executionContext.getFormContext();

    // Only save if form is dirty, valid, and not already saving
    if (formContext.data.getIsDirty() && formContext.data.isValid()) {
        formContext.data.save().then(
            function success() {
                console.log("Autosave successful.");
            },
            function error(err) {
                console.error("Autosave failed: " + err.message);
            }
        );
    }
}