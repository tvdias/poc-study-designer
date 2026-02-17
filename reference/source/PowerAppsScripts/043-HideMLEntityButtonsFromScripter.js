/**
 * @file        043-HideMLEntityButtonsFromScripter.js
 * @description This hides the Activate / Deactivate buttons and Delete button for Managed list entity record to Scripter
 *
 * @date        2025-10-07
 * @version     1.0
 *
 * @usage       Used on Activate / Deactivate and Delete buttons on grid and form of Managed list entity, through ribbon workbench
 * @notes       
 */
function hideOrShowStatusButtons(primaryItemIds) {
    const disallowedRoleIds = ["3a27fcc5-cc0a-f011-bae2-000d3a2274a5"]; // Kantar - scripter role
    const userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

    for (let i = 0; i < userRoles.getLength(); i++) {
        const role = userRoles.get(i);
        if (disallowedRoleIds.includes(role.id)) {
            return false; // hide the button
        }
    }
    return true; // show the button
}

