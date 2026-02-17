/**
 * @file        048-AutoRefreshQuestionAllocationTab.js
 * @description Will auto refresh the Question Allocation tab in the Managed List form.
 *
 * @date        2025-11-20
 * @version     1.0
 *
 * @usage       This script is invoked on tab state change of Question allocation tab in the Managed List form.
 * @notes       
 */

function onTabStateChange(executionContext) {
    var tab = executionContext.getEventSource();

    if (tab.getDisplayState() === "expanded") {
        var control = Xrm.Page.getControl("Subgrid_new_3");

        if (!control) {
            console.log("Subgrid_new_3 control not found yet.");
            return;
        }

        console.log("Tab expanded. Scheduling PCF refresh...");

        // Delay to allow PCF to initialize fully
        setTimeout(function () {
            if (control && control.refresh) {
                console.log("Refreshing PCF subgrid after delay...");
                control.refresh();
            } else {
                console.log("Control found but refresh() not available.");
            }
        }, 400); 
    }
}