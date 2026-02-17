/**
 * @file        033-checkAndManageStudyForm.js
 * @description Manages Study form field editability based on status reason
 *
 * @date        2025-07-17
 * @version     1.0
 *
 * @usage       This script is invoked on load and status change of the Study Main form
 * @notes       Implements form management logic based on status reason workflow
 */

// Status reason constants
const StatusReasons = {
    DRAFT: 1,
    READY_FOR_SCRIPTING: 847610001,
    APPROVED_FOR_LAUNCH: 847610002,
    REWORK: 847610006,
    ABANDON: 847610005,
    COMPLETED: 2,
    SNAPSHOT: 847610004
};

// Status constants
const Status = {
    ACTIVE: 0,
    INACTIVE: 1
};

function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();
    checkAndManageStudyForm(formContext);
}

function onStatusChange(executionContext) {
    var formContext = executionContext.getFormContext();
    checkAndManageStudyForm(formContext);
}

function checkAndManageStudyForm(formContext) {
    var statusAttr = formContext.getAttribute("statuscode");
    var stateAttr = formContext.getAttribute("statecode");

    if (!statusAttr || !stateAttr) {
        return;
    }

    var statusReason = statusAttr.getValue();
    var status = stateAttr.getValue();

    // Apply rules based on status reason
    switch (statusReason) {
        case StatusReasons.DRAFT:
            applyDraftRules(formContext);
            break;

        case StatusReasons.READY_FOR_SCRIPTING:
            applyReadyForScriptingRules(formContext);
            break;

        case StatusReasons.APPROVED_FOR_LAUNCH:
            applyApprovedForLaunchRules(formContext);
            break;

        case StatusReasons.REWORK:
            applyReworkRules(formContext);
            break;

        case StatusReasons.ABANDON:
            applyAbandonRules(formContext);
            break;

        case StatusReasons.COMPLETED:
        case StatusReasons.SNAPSHOT:
            // Both Completed and Snapshot have same rules when Status = Inactive
            if (status === Status.INACTIVE) {
                applyInactiveRules(formContext);
            }
            break;

        default:
            // Default behavior - make everything editable
            applyDraftRules(formContext);
            break;
    }
}

/**
 * Draft status: All fields, tabs, and grids are editable
 */
function applyDraftRules(formContext) {
    setAllFieldsEditability(formContext, false); // false = enabled/editable
    setFieldEditability(formContext, "ktr_parentstudy", true);
}

/**
 * Ready for Scripting: Only Scripter Notes and Status Reason editable, everything else read-only
 */
function applyReadyForScriptingRules(formContext) {
    setAllFieldsEditability(formContext, true); // true = disabled/read-only

    // Enable only Scripter Notes and Status Reason
    setFieldEditability(formContext, "ktr_scripternotes", false);
    setFieldEditability(formContext, "header_statuscode", false);
}

/**
 * Approved for Launch: Only Status Reason editable, everything else read-only
 */
function applyApprovedForLaunchRules(formContext) {
    setAllFieldsEditability(formContext, true);
    
    // Enable only Status Reason
    setFieldEditability(formContext, "header_statuscode", false);
}

/**
 * Rework and Abandon: Nothing is editable
 */
function applyReworkRules(formContext) {
    setAllFieldsEditability(formContext, true);
}

function applyAbandonRules(formContext) {
    setAllFieldsEditability(formContext, true);
}

/**
 * Completed/Snapshot with Inactive status: Nothing is editable
 */
function applyInactiveRules(formContext) {
    setAllFieldsEditability(formContext, true);
}

/**
 * Helper function to set editability for all fields
 */
function setAllFieldsEditability(formContext, disabled) {
    formContext.ui.controls.forEach(function (control) {
        if (control.getControlType() === "standard" || control.getControlType() === "lookup" || control.getControlType() === "optionset" || control.getControlType() === "customcontrol:MscrmControls.RichTextEditor.RichTextEditorControl") {
            control.setDisabled(disabled);
        }
    });
}

/**
 * Helper function to set editability for a specific field
 */
function setFieldEditability(formContext, fieldName, disabled) {
    // Wait for control to load before setting disabled state
    setTimeout(function () {
        var control = formContext.getControl(fieldName);
        if (control) {
            control.setDisabled(disabled);
        }
    }, 500); // Wait 500ms for control to be ready
}
