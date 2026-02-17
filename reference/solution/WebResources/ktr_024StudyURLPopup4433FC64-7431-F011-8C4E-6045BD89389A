/**
 * @file        024-showURLPopup.js
 * @description Will show pop up on study main form, when status reason changes from Draft to Ready for Scripting
 *
 * @date        2025-05-21
 * @version     1.1
 *
 * @usage       This script is invoked on load and save of the Main form of Study entity
 * @notes       
 */

var previousStatusReason = null;
const ReadyForScriptingStatus = 847610001; // Your status reason value

function captureStatusOnLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    var statusAttr = formContext.getAttribute("statuscode");

    if (statusAttr) {
        previousStatusReason = statusAttr.getValue();
    }
}

function showPopupOnStatusReasonChange(executionContext) {
    var formContext = executionContext.getFormContext();
    var statusAttr = formContext.getAttribute("statuscode");
    var currentStatus = statusAttr ? statusAttr.getValue() : null;

    if (currentStatus === ReadyForScriptingStatus && previousStatusReason !== ReadyForScriptingStatus) {
        Xrm.Navigation.openConfirmDialog({
            title: "Message",
            text: "Please ensure that you have added valid URL records for Quotas, Variable Content, Media and Other Documents to this Study record before you continue.",
            confirmButtonLabel: "OK",
            cancelButtonLabel: "Cancel"
        }).then(function (result) {
            if (result.confirmed) {
                // User confirmed; update previous value to avoid multiple popups
                previousStatusReason = currentStatus;
            } else {
                // User cancelled; revert the status back
                statusAttr.setValue(previousStatusReason);
            }
        });
    }
}