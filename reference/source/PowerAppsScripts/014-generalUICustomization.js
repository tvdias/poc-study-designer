/**
 * @file        014-generalUICustomization.js
 * @description General scripting for UI.
 *
 * @date        2025-04-01
 * @version     1.0
 *
 * @usage       General scripting for UI related functions like hiding and showing tabs, fields etc.
 * @notes       This script is for general UI customization and should be used in conjunction with other scripts.
 *              It is not bound to any specific form or entity and can be reused across different forms.
 */



var Kantar = Kantar || {};
Kantar.UIGeneral = Kantar.UIGeneral || {};

//=========================================================
//USAGE EXAMPLE:
//Kantar.UIGeneral.setTabVisibility(executionContext, 'tabName', true); // Replace 'tabName' with the actual tab name and true/false for visibility
//=========================================================
Kantar.UIGeneral.setTabVisibility = function (executionContext, tabName, isVisible) {
    try {
        console.log("Entering setTabVisibility");

        // Get the form context from the execution context
        var formContext = executionContext.getFormContext();

        // Check if the tab exists
        var tab = formContext.ui.tabs.get(tabName);
        if (tab) {
            // Set the visibility of the tab
            tab.setVisible(isVisible);
        } else {
            throw new Error("Tab with name " + tabName + " not found.");
        }
    } catch (error) {
        console.error("Error in setTabVisibility: ", error.message);
    }
};