var abandonOrReworkStudyButton = (function (config) {
    let studyEntityLogicalName = config.studyEntityLogicalName;

    async function main(primaryControl, isAbandon) {
        const formContext = primaryControl;
        const studyId = formContext.data.entity.getId().replace(/[{}]/g, "");
        const newStatusReason = isAbandon ? 847610005 : 847610006; // Abandon = 847610005, Rework = 847610006

        let text = isAbandon
            ? "This study will be abandoned. Are you sure you want to continue?"
            : "This study will be marked for rework. Are you sure you want to continue?";

        if (!newStatusReason) {
            Xrm.Navigation.openAlertDialog("Please select a valid status reason.");
            return;
        }

        showConfirmationDialog(text).then(async function (result) {
            if (result.confirmed) {
                Xrm.Utility.showProgressIndicator("Processing...");
                await updateStudyStatus(studyId, newStatusReason, isAbandon)
                    .then(async (data) => {
                        let confirmationMessage = data.ktr_study_status_confirmation || "Action completed.";

                        if (!isAbandon && data.ktr_new_reworkstudy) {
                            Xrm.Utility.openEntityForm(studyEntityLogicalName, data.ktr_new_reworkstudy);
                            Xrm.Utility.closeProgressIndicator();
                        } else {
                            Xrm.Navigation.openAlertDialog({
                                text: confirmationMessage
                            }).then(() => {
                                formContext.data.refresh();
                                Xrm.Utility.closeProgressIndicator();
                            });
                        }
                    })
                    .catch((error) => {
                        Xrm.Navigation.openAlertDialog({
                            text: "Error: " + error.message
                        });
                    });
            }
        });
    }

    async function updateStudyStatus(studyId, newStatusReason, isAbandon) {
        let actionName = "ktr_abandon_or_rework_study";
        let request = {
            ktr_study_id: studyId,
            ktr_new_status_reason_study: newStatusReason,
            getMetadata: function () {
                return {
                    boundParameter: null,
                    parameterTypes: {
                        ktr_study_id: { typeName: "Edm.String", structuralProperty: 1 },
                        ktr_new_status_reason_study: { typeName: "Edm.Int32", structuralProperty: 1 }
                    },
                    operationType: 0,
                    operationName: actionName
                };
            }
        };

        return Xrm.WebApi.online.execute(request)
            .then(function (response) {
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
