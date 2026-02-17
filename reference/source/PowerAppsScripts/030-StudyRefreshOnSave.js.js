/**
 * @file        030-StudyRefreshOnSave.js
 * @description Refreshes the Study form post save
 *
 * @date        2025-07-11
 * @version     1.2
 *
 * @usage       This script is invoked on save of the Main form of Study entity
 *              This is supposed to refresh the study form post the record is saved so that user can see the 
 *                  FWLanguages cleared out when the Fieldwork Market lookup in Study is updated.
 *             The script also calls a Custom API to regenerate HTML questionnaire lines before save
 *             and calls another Custom API to create study snapshots after save if the Study is in Ready for Scripting status.
 * @notes       
 */

var StudyRefreshOnSave = (function (config) {

    const LanguagesSubgrid = config.LanguagesSubgrid;
    const DraftStatusReason = config.DraftStatusReason;
    const ReadyForScriptingReason = config.ReadyForScriptingReason;
    
    // Flag to prevent recursive save
    let isSavingFromScript = false;

    async function onSave(executionContext) {
        var formContext = executionContext.getFormContext();
        const eventArgs = executionContext.getEventArgs();

        // Prevent recursion
        if (isSavingFromScript) {
            console.log("Save triggered by script — skipping API call.");
            return;
        }

        // Check if the form is in Create (NEW) mode
        const formType = formContext.ui.getFormType(); 
        if (formType === 1) {
            console.log("Form is in NEW mode — skipping API call.");
            return;
        }

        // Get current and original status values
        const statusReasonAttr = formContext.getAttribute("statuscode");
        const statusReason = statusReasonAttr?.getValue();
        const originalStatusReason = statusReasonAttr?.getInitialValue();
        
        // Check if status is transitioning from Draft to Ready for Scripting
        const isTransitioningToReadyForScripting = 
            originalStatusReason === DraftStatusReason && 
            statusReason === ReadyForScriptingReason;
        
        if (statusReason !== DraftStatusReason && statusReason !== ReadyForScriptingReason) {
            console.log("Study is not in DRAFT or READY FOR SCRIPTING — skipping API call.");
            return;
        }

        const projectId = formContext.getAttribute("kt_project")?.getValue();
        if (!projectId || projectId.length === 0) {
            console.warn("ProjectId empty — skipping API call.");
            return;
        }

        // Stop the save → we want API FIRST
        eventArgs.preventDefault();

        try {
            const projectGuid = projectId[0].id.replace(/[{}]/g, "");
            const response = await callRegenerateHTMLCustomAPI(projectGuid);

            if (!response.ok) {
                console.error("Custom API returned an error:", response);
            } else {
                console.log("Custom API completed successfully.");
            }

            // Set flag to prevent recursion
            isSavingFromScript = true;
            await formContext.data.save();
            isSavingFromScript = false;
        } catch (error) {
            console.error("Custom API execution failed:", error);

            // Even if API fails, allow save without recursion
            isSavingFromScript = true;
            await formContext.data.save();
            isSavingFromScript = false;
        }

        // AFTER save, call snapshot API ONLY if transitioning from Draft to Ready for Scripting
        if (isTransitioningToReadyForScripting) {
            const studyId = formContext.data.entity.getId().replace(/[{}]/g, "");
            callCreateStudySnapshotsAPI(studyId);
            console.log("Snapshot API triggered (processing in background).");
        }

        // Post-save subgrid refresh
        setTimeout(function () {
            formContext.getControl(LanguagesSubgrid).refresh();
        }, 3000);
    }

    function callRegenerateHTMLCustomAPI(projectId) {
        var request = {
            projectId: projectId,
            getMetadata: function () {
                return {
                    boundParameter: null,
                    parameterTypes: {
                        projectId: {
                            typeName: "Edm.String",
                            structuralProperty: 1
                        }
                    },
                    operationType: 0,
                    operationName: "ktr_project_questionnairelines_regenerate_html_unbound"
                };
            }
        };

        return Xrm.WebApi.online.execute(request);
    }

    function callCreateStudySnapshotsAPI(studyId) {
        var request = {
            studyId: studyId,
            getMetadata: function () {
                return {
                    boundParameter: null,
                    parameterTypes: {
                        studyId: {
                            typeName: "Edm.String",
                            structuralProperty: 1
                        }
                    },
                    operationType: 0,
                    operationName: "ktr_create_study_snapshots"
                };
            }
        };

        return Xrm.WebApi.online.execute(request);
    }

    return {
        onSave: onSave,
    };
})({
   LanguagesSubgrid: 'Subgrid_new_6',
   DraftStatusReason: 1,
   ReadyForScriptingReason: 847610001
});
