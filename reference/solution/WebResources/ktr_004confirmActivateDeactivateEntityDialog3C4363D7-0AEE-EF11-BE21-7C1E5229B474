/**
 * @file        confirmActivateDeactivateEntityDialog.js
 * @description Opens a confirmation Dialog to ask if user is sure about activating/deactivating:
 *                  - Module (if you pass parameter 'kt_module')
 *                  - Question (if you pass parameter 'kt_questionbank')
 *              
 * @date        2025-02-18
 * @version     2.0
 * 
 * @usage       This script is being called in Module/QuestionBank form in OnChange event of 'Status Reason' dropdown
 * @notes       This script uses Xrm Power Apps library
*/
var ActivateDeactivateEntityDialog = (function(config) {
    const statusActive = config.status.active;
    const statusInactive = config.status.inactive;
    const statusDraft = config.status.draft;

    const stateActive = config.state.active;
    const stateInactive = config.state.inactive;

    const messages = config.messages;

    function openConfirmActivationDialog(executionContext, logicalEntityName) {
        var formContext = executionContext.getFormContext();
        var statusField = formContext.getAttribute("statuscode");

        if (statusField) {
            var newValue = statusField.getValue();
            var oldValue = statusField.getInitialValue();

            if (newValue !== oldValue && newValue == statusActive) {
                showConfirmDialog(
                    messages[logicalEntityName].activateTitle,
                    messages[logicalEntityName].activateMessage,
                    function () {
                        statusField.setValue(oldValue);
                    });
            }
        }
    }

    function openConfirmInactivationDialog(executionContext, logicalEntityName){
        var formContext = executionContext.getFormContext();
        var stateField = formContext.getAttribute("statecode");
        var statusField = formContext.getAttribute("statuscode");

        if (stateField) {
            var newValue = stateField.getValue();
            var oldValue = stateField.getInitialValue();

            var statusCurrentValue = statusField.getValue();

            if (statusCurrentValue != statusDraft && newValue !== oldValue && newValue == stateInactive) {
                showConfirmDialog(
                    messages[logicalEntityName].inactivateTitle,
                    messages[logicalEntityName].inactivateMessage,
                    function () {
                        stateField.setValue(oldValue);
                    });
            }
        }
    }

    function showConfirmDialog(confirmTitle, confirmMessage, onCancelCallbackFn) {
        var confirmStrings = {
            text: confirmMessage,
            title: confirmTitle,
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        };
        var confirmOptions = { height: 200, width: 400 };

        Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions)
            .then(function (result) {
                if (!result.confirmed) {
                    // Revert to old value if user cancels
                    onCancelCallbackFn();
                }
        });
    }

    return {
        openConfirmActivationDialog: openConfirmActivationDialog,
        openConfirmInactivationDialog: openConfirmInactivationDialog,
    };
})({
    state: {
        active: 0,
        inactive: 1
    },
    status: {
        active: 1,
        inactive: 2,
        draft: 847610001,
    },
    messages: {
        kt_module: {
            activateTitle: 'Activate Module',
            activateMessage: 'Are you sure you want to activate this module? This action will make the module available for selection after saving.',
            inactivateTitle: 'Deactivate Module',
            inactivateMessage: 'Are you sure you want to deactivate this module? This action will make the module not able to be selected after saving.'
        },
        kt_questionbank: {
            activateTitle: 'Activate Question',
            activateMessage: 'Are you sure you want to activate this question? This action will make the question available for selection after saving.',
            inactivateTitle: 'Deactivate Question',
            inactivateMessage: 'Are you sure you want to deactivate this question? This action will make the question not able to be selected after saving.'
        },
    }
});