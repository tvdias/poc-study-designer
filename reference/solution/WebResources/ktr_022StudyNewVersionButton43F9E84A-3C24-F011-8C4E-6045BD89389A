var newVersionButton = (function (config) {
    let studyEntityLogicalName = config.studyEntityLogicalName;

    async function main(primaryControl) {
        const formContext = primaryControl;
        const studyId = formContext.data.entity.getId().replace(/[{}]/g, "");

        const confirmationText = "Are you sure you want to create a new version of this study?";

        showConfirmationDialog(confirmationText).then(async function (result) {
            if (result.confirmed) {
                try {
                    Xrm.Utility.showProgressIndicator("Processing...");
                    const data = await cloneStudyNewVersion(studyId);
                    let newStudyId = data.ktr_new_version_study;

                    if (newStudyId) {
                        await Xrm.Navigation.openAlertDialog({
                            text: "New version of the study created successfully."
                        });

                        await Xrm.Navigation.openForm({
                            entityName: studyEntityLogicalName,
                            entityId: newStudyId
                        });

                        Xrm.Utility.closeProgressIndicator();
                    } else {
                        await Xrm.Navigation.openAlertDialog({
                            text: "New study version could not be created. Please try again."
                        });

                        Xrm.Utility.closeProgressIndicator();
                    }
                } catch (error) {
                    await Xrm.Navigation.openAlertDialog({
                        text: "Error: " + error.message
                    });

                    Xrm.Utility.closeProgressIndicator();
                }
            }
        });
    }

    async function cloneStudyNewVersion(studyId) {
        let actionName = "ktr_study_new_version"; // Your Custom API Name

        let request = {
            ktr_oldstudy_id: studyId,
            getMetadata: function () {
                return {
                    boundParameter: null,
                    parameterTypes: {
                        ktr_oldstudy_id: { typeName: "Edm.String", structuralProperty: 1 }
                    },
                    operationType: 0,
                    operationName: actionName
                };
            }
        };

        return Xrm.WebApi.online.execute(request)
            .then(async function (response) {
                if (!response.ok) {
                    const errorBody = await response.json();
                    throw new Error(errorBody.error.message);
                }
                return response.json();
            });
    }

    function showConfirmationDialog(text) {
        return Xrm.Navigation.openConfirmDialog({
            title: "Confirmation",
            text: text,
            confirmButtonLabel: "Yes",
            cancelButtonLabel: "No"
        });
    }

    return {
        main: main
    };
})({
    studyEntityLogicalName: "kt_study"
});
