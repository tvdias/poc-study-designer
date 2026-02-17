/**
 * @file        029-HideScriptletsFromRoles.js
 *
 * @date        2025-07-17
 * @version     1.0
 *
 * @usage       This script is used to hide/show Scriptlet tab on form or Scriptlet field on quick view form from CS user and Librarian
 * @notes       This script works onLoad of Questionnaire line main form, Study snapshots main form, Questionnaire line quick view
 */
//847610000 - CS User
//847610001 - Librarian
const DisallowedRoles = [847610000, 847610001];

//Visibility of tab on main form (keeping tab name variable to use same function in multiple main forms)
function hideScripletTabForDisallowedRoles(executionContext, tabName) {
    var formContext = executionContext.getFormContext();
    var userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace("{", "").replace("}", "");
    Xrm.WebApi.retrieveRecord("systemuser", userId, "?$select=ktr_businessrole").then(function (result) {
        var userRoleValue = result.ktr_businessrole;
        if (DisallowedRoles.includes(userRoleValue)) {
            var tab = formContext.ui.tabs.get(tabName);
            if (tab) {
                tab.setVisible(false);
            } else {
                console.warn("Tab '" + tabName + "' not found on this form.");
            }
        }
    }).catch(function (error) {
        console.error("Error retrieving user business role:", error.message);
    });
}
//Visibility of Scriptlets field on quick view
function hideScripletFieldForDisallowedRoles(executionContext) {

    var formContext = executionContext.getFormContext();
    var quickViewControl = formContext.ui.quickForms.get("QuickviewControl1744888826162");
    var columncontrol = quickViewControl.getControl("ktr_scriptlets");
    var userId = Xrm.Utility.getGlobalContext().userSettings.userId.replace("{", "").replace("}", "");
    Xrm.WebApi.retrieveRecord("systemuser", userId, "?$select=ktr_businessrole").then(function (result) {
        var userRoleValue = result.ktr_businessrole;
        if (DisallowedRoles.includes(userRoleValue)) {
            if (columncontrol) {

                columncontrol.setVisible(false);
            }
            else {
                console.log("Field not found on this form.");
            }
        }
    }).catch(function (error) {
        console.error("Error retrieving user business role:", error.message);
    });
}