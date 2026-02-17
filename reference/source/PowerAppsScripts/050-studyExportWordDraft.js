/**
 * @file        050-studyExportWordDraft.js
 * @description Set the functionality for Study export word button, in draft state.
 *
 * @date        2025-11-26
 * @version     1.0
 *
 * @usage       This script is used to set onclick functionality of the create document button.
 * @notes       This script works only on Study create document button.
 */

var Kantar = Kantar || {};
var DRAFT_STATUS = 1;

//OCA ID - 82c25cb1-081a-f011-998a-7c1e5275a51f

Kantar.StudyExportWordDraft = (function () {

    function loadDCPWebResource(resourceName) {
        return new Promise((resolve, reject) => {
            const scriptId = resourceName.replace(/\W/g, "_");

            if (document.getElementById(scriptId)) {
                resolve();
                return;
            }

            const clientUrl = Xrm.Utility.getGlobalContext().getClientUrl();
            const script = document.createElement("script");
            script.id = scriptId;
            script.type = "text/javascript";
            script.src = `${clientUrl}/WebResources/${resourceName}`;

            script.onload = () => {
                console.log(`${resourceName} loaded`);
                resolve();
            };

            script.onerror = () => {
                console.error(`Failed to load ${resourceName}`);
                reject(new Error(`Failed to load ${resourceName}`));
            };

            document.head.appendChild(script);
        });
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

    async function openExportWordDraftDocument(primaryControl, itemId, ocaId) {

        var formContext = primaryControl;
        var rawStudyId = formContext.data.entity.getId();
        if (!rawStudyId) {
            alert("No record selected.");
            return;
        }

        var statusReason = formContext.getAttribute("statuscode")?.getValue();
        console.log("Status Reason:", statusReason);

        var runApi = (statusReason === DRAFT_STATUS);
        var projectId = null;

        // Only proceed with API if Study is draft
        if (runApi) {
            var projectLookup = formContext.getAttribute("kt_project")?.getValue();

            if (!projectLookup || projectLookup.length === 0) {
                alert("No project is linked to this Study.");
                return;
            }

            projectId = projectLookup[0].id.replace(/[{}]/g, "");
            console.log("Project ID:", projectId);

            Xrm.Utility.showProgressIndicator("Generating Study Document...");
        }

        try {
            // DCP loads in all cases
            var dcpPromise = loadDCPWebResource("ptm_globalambutton.min.js");

            // Only run API for Draft
            if (runApi) {
                var response = await callRegenerateHTMLCustomAPI(projectId);

                if (!response.ok) {
                    console.error("Custom API failed:", response.status);
                    alert("Could not generate HTML. Please try again.");
                    return;
                }
            }

            await dcpPromise;

            if (runApi) Xrm.Utility.closeProgressIndicator();

            if (typeof ptm_openLookupDlg === "function") {
                ptm_openLookupDlg(
                    itemId, itemId, itemId, itemId, itemId,
                    itemId, itemId, itemId, itemId, itemId,
                    ocaId
                );
            } else {
                alert("Document CorePack dialog is not available. Please try again.");
            }

        } catch (e) {
            console.error("Error:", e);
            alert("Error: " + (e.message || e));
        } finally {
            try { Xrm.Utility.closeProgressIndicator(); } catch (e) {}
        }
    }

    return {
        openExportWordDraftDocument: openExportWordDraftDocument
    };

})();

window.openExportWordDraftDocument = Kantar.StudyExportWordDraft.openExportWordDraftDocument;