/**
 * @file        049-studyExportXml.js
 * @description Set the functionality for Stud export xml button.
 *
 * @date        2025-11-26
 * @version     1.0
 *
 * @usage       This script is used to set onclick functionality of the export xml button.
 * @notes       This script works only on Study entity xml button.
 */

var Kantar = Kantar || {};
Kantar.StudyExportXml = (function () {

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

    function callGenerateXmlCustomApi(studyId) {
        var request = {
            ktr_studyId: studyId,
            getMetadata: function () {
                return {
                    boundParameter: null,
                    parameterTypes: {
                        ktr_studyId: {
                            typeName: "Edm.String",
                            structuralProperty: 1
                        }
                    },
                    operationType: 0,
                    operationName: "ktr_generate_study_xml"
                };
            }
        };

        return Xrm.WebApi.online.execute(request);
    }

    async function openExportXMLDocument(
        primaryControl,
        itemId, // FirstSelectedItemId
        ocaId, // One Click Action Id
    ) {
         var formContext = primaryControl;
            
        var rawStudyId = formContext.data.entity.getId();
        if (!rawStudyId) {
            alert("No record selected.");
            return;
        }

        var studyId = rawStudyId.replace(/[{}]/g, "");

        Xrm.Utility.showProgressIndicator("Generating study XML...");

        try {
            var apiPromise = callGenerateXmlCustomApi(studyId);
            var dcpPromise = loadDCPWebResource("ptm_globalambutton.min.js");

            var response = await apiPromise;

            if (!response.ok) {
                console.error("Custom API call failed with status:", response.status);
                alert("Could not generate study XML. Please try again.");
                return;
            }

            await dcpPromise;

            Xrm.Utility.closeProgressIndicator();

            if (typeof ptm_openLookupDlg === "function") {
                ptm_openLookupDlg(
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    itemId,
                    ocaId
                );
            } else {
                alert("Document CorePack dialog is not available. Please try again.");
            }

        } catch (e) {
            console.error("Error in openExportXMLDocument:", e);
            alert("Error: " + (e.message || e));
        } finally {
            try {
                Xrm.Utility.closeProgressIndicator();
            } catch (e) {
            }
        }
    }

    return {
        openExportXMLDocument: openExportXMLDocument
    };

})();

window.openExportXMLDocument = Kantar.StudyExportXml.openExportXMLDocument;