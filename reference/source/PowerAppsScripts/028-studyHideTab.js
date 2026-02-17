/**
 * @file        028-studyHideTab.js
 * @description onLoad of the Study, hide the “Scripter View” tab from any user that has the “Kantar – Client Services User”
 *              security role (GUID "c567cc6b-210a-f011-bae2-000d3a2274a5"
 *
 * @date        2025-07-09
 * @version     1.0
 *
 * @usage       This script is invoked on load of the Main form of Study entity
 * @notes       
 */

const ClientServicesRoleId = "c567cc6b-210a-f011-bae2-000d3a2274a5"; // GUID of the "Kantar – Client Services User" role

function hideTabForClientServiceUser(executionContext) {
    var formContext = executionContext.getFormContext();
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles.getAll()

    // Check if the user has the "Kantar – Client Services User" role
    var hasClientServicesRole = userRoles.some(function (role) {
        return role.id.toLowerCase() === ClientServicesRoleId;
    });

    // If the user has the role, hide the "Scripter View" tab
    if (hasClientServicesRole) {
        var scripterViewTab = formContext.ui.tabs.get("tab_7");
        if (scripterViewTab) {
            scripterViewTab.setVisible(false);
        }
    }
}
