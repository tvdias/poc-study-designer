
/**
 * @file        027-formRelaod.js
 * @description To reload the form when a Product is added.
 *
 * @date        2025-06-30
 * @version     1.0
 *
 * @usage       This script is invoked on Save of Project, to reload the Canvas app and the form.
 * @notes       
 */


function addMessageToOnPostSave(executionContext) {
    var formContext = executionContext.getFormContext();
    formContext.data.entity.addOnPostSave(reloadFormOnSave);
}
/**
 * To reload on form save after any changes 
 * @param {} executionContext 
 */

function reloadFormOnSave(executionContext) {
    var formContext = executionContext.getFormContext();

    try {
        // Get current record details
        var entityName = formContext.data.entity.getEntityName();
        var recordId = formContext.data.entity.getId();

        // Remove the GUID brackets if present
        if (recordId.startsWith("{") && recordId.endsWith("}")) {
            recordId = recordId.slice(1, -1);
        }
        // Reload the entire form
        var pageInput = {
            pageType: "entityrecord",
            entityName: entityName,
            entityId: recordId
        };

        Xrm.Navigation.navigateTo(pageInput).then(
            function success() {
                console.log("Form reloaded successfully after save.");
            },
            function error(error) {
                console.error("Error reloading form:", error);
                window.location.reload();
            }
        );

    } catch (error) {
        console.error("Error in reloadFormOnSave:", error);
        window.location.reload();
    }
}
/**
 * Store tab preference in sessionStorage for after reload
 * @param {*} executionContext 
 */
function onProductLookupChanged(executionContext) {
    sessionStorage.setItem("activateProductTab", "true");
}
/**
 * To set the Tab after Reload
 * @param {*} executionContext 
 */

function setProductTabOnLoad(executionContext) {
    // Check if we need to activate the Product tab after reload
    if (sessionStorage.getItem("activateProductTab") === "true") {
        sessionStorage.removeItem("activateProductTab");

        var formContext = executionContext.getFormContext();

        // Wait a bit longer for form to fully load
        setTimeout(function () {
            try {
                var productTab = formContext.ui.tabs.get("tab_Product");
                if (productTab) {
                    productTab.setFocus();
                    console.log("Product tab set as active after reload.");
                } else {
                    console.warn("Product tab not found after reload.");
                }
            } catch (tabError) {
                console.error("Error setting Product tab active after reload:", tabError);
            }
        }, 2000); // Increased delay for canvas app loading
    }
}