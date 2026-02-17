/**
 * @file        045-ManagedListAnswercodeEditableScript.js
 * @description Makes sure : Inactive Managed Lists are completely read-only and non-saveable, The “Answer Code Editable” flag auto-saves itself when toggled.
 *
 * @date        2025-10-08
 * @version     1.0
 *
 * @usage       Used on form of Managed list.
 * @notes       
 */

var Ktr = Ktr || {};
Ktr.ManagedList = Ktr.ManagedList || {};

/**
 * Disable all editable fields if record is inactive.
 * Should be called on Form OnLoad.
 * @param {Object} executionContext - The form execution context.
 */
Ktr.ManagedList.DisableFieldsForInactive = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var stateAttr = formContext.getAttribute("statecode");

    if (!stateAttr) return;

    var state = stateAttr.getValue(); // 0 = Active, 1 = Inactive (OOB default)

    if (state === 1) {
        console.log("[Lockdown] Record is inactive - disabling all fields.");

        // Disable all controls
        formContext.ui.controls.forEach(function (control) {
            try {
                if (control && control.getDisabled && control.setDisabled) {
                    control.setDisabled(true);
                }
            } catch (e) {
                console.warn("[Lockdown] Could not disable control:", control.getName(), e);
            }
        });

        // Prevent save attempts
        formContext.data.entity.addOnSave(function (eContext) {
            eContext.getEventArgs().preventDefault();
            Xrm.Navigation.openAlertDialog({
                title: "Save Blocked",
                text: "This record is inactive and cannot be saved."
            });
        });
    }
};


/**
 * Prevent save explicitly when record is inactive.
 * Should be registered on Form OnSave (with execution context).
 */
Ktr.ManagedList.PreventSaveWhenInactive = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var stateAttr = formContext.getAttribute("statecode");

    if (stateAttr && stateAttr.getValue() === 1) {
        var eventArgs = executionContext.getEventArgs();
        eventArgs.preventDefault();

        Xrm.Navigation.openAlertDialog({
            title: "Save Blocked",
            text: "You cannot save changes to an inactive record."
        });
    }
};


/**
 * Auto-save form when Answer Code Editable field changes
 * Triggered when user toggles the ktr_answercodeeditable checkbox
 * @param {Object} executionContext - Execution context from field change event
 */
Ktr.ManagedList.OnAnswerCodeEditableChange = async function(executionContext) {
    try {
        var formContext = executionContext.getFormContext();
        var answerCodeEditableAttr = formContext.getAttribute("ktr_answercodeeditable");
        
        if (!answerCodeEditableAttr) {
            console.warn("[Auto-Save] ktr_answercodeeditable field not found");
            return;
        }
        
        var newValue = answerCodeEditableAttr.getValue();
        console.log(`[Auto-Save] Answer Code Editable changed to: ${newValue}`);
        
        // Check if form has unsaved changes
        var isDirty = formContext.data.entity.getIsDirty();
        
        if (!isDirty) {
            console.log("[Auto-Save] No unsaved changes detected - skipping save");
            return;
        }
        
        // Check if this is a new record (not yet saved)
        var recordId = formContext.data.entity.getId();
        if (!recordId) {
            console.log("[Auto-Save] New unsaved record - user must save manually first");
            
            // Show notification to guide user
            Xrm.Navigation.openAlertDialog({
                text: "Please save this Managed List record before toggling the Answer Code Editable setting.",
                title: "Save Required"
            });
            
            return;
        }
        
        console.log("[Auto-Save] Initiating auto-save...");
        
        // Save the form asynchronously
        await formContext.data.save();
        
        console.log("[Auto-Save] Form saved successfully");
        
        // Optional: Show brief success notification
        formContext.ui.setFormNotification(
            "Answer Code Editable setting saved",
            "INFO",
            "AUTOSAVE_SUCCESS"
        );
        
        // Clear notification after 3 seconds
        setTimeout(function() {
            formContext.ui.clearFormNotification("AUTOSAVE_SUCCESS");
        }, 3000);
        
    } catch (error) {
        console.error("[Auto-Save] Error during auto-save:", error);
        
        // Show error notification to user
        var formContext = executionContext.getFormContext();
        if (formContext && formContext.ui) {
            formContext.ui.setFormNotification(
                "Failed to auto-save. Please save manually.",
                "ERROR",
                "AUTOSAVE_ERROR"
            );
        }
        
        // Optional: Show error dialog for better visibility
        Xrm.Navigation.openAlertDialog({
            text: "Auto-save failed: " + error.message + "\n\nPlease save the record manually.",
            title: "Auto-Save Error"
        });
    }
};