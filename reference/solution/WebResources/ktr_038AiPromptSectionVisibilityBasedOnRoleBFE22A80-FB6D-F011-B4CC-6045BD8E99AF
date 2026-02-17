/**
* @file        038-AiPromptSectionVisibilityBasedOnRole.js
* @description Will hide Ai Prompt section for CS User and Scripter business roles.
*
* @date        2025-07-31
* @version     1.1
*
* @usage       This script is invoked on Configuration question form load
* @notes       
*/

//847610000 - CS User
//847610002 - Scripter
const disallowedRoles = [847610000, 847610002];

function hideAiPromptSectionBasedOnRole(executionContext) {
    var formContext = executionContext.getFormContext();
    // Get User Id from global context
    var userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace("{", "").replace("}", "");
    Xrm.WebApi.retrieveRecord("systemuser", userId, "?$select=ktr_businessrole").then(function (result) {
        var userRoleValue = result.ktr_businessrole;
        if (disallowedRoles.includes(userRoleValue)) {
            // Hide the section
            var tabName = "tab_general"; // Logical name of the tab
            var sectionName = "sec_aiprompt"; // Logical name of the section
            var section = formContext.ui.tabs.get(tabName).sections.get(sectionName);
            if (section) {
                   section.setVisible(false);
                } else {
                   console.warn("Section '" + section + "' not found on this form.");
                }      
        }       
    }).catch(function (error) {
        console.error("Error retrieving user business role:", error.message);
    });
}