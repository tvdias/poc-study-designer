function markAsComplete(formContext) {
    var entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
    var entityName = formContext.data.entity.getEntityName();

    var data = {
        statecode: 1,     // Inactive
        statuscode: 2     //  "Complete"
    };

    Xrm.WebApi.updateRecord(entityName, entityId, data).then(
        function success(result) {
            Xrm.Navigation.openAlertDialog({ text: "Record marked as complete." }).then(
                function () {
                    formContext.data.refresh(true); // Refreshes the form
                }
            );
        },
        function (error) {
            Xrm.Navigation.openAlertDialog({ text: "Error: " + error.message });
        }
    );
}
