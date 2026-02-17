/**
 * @file        010-ProjetTemplateApplyCommandAPI.js
 * @description Set the functionality for ApplyTemplateProject.
 *
 * @date        2025-04-01
 * @version     3.0
 *
 * @usage       This script is used on click of ApplyProjectTemplate button in Project Form.
 * @notes       
 */


var applyTemplateButton = (function (config) {
  const ql_entityLogicalName = config.ql_entityLogicalName;

  async function main(primaryControl) {
    const formContext = primaryControl;
    const projectId = formContext.data.entity.getId().replace(/[{}]/g, "");

    var productId = primaryControl.getAttribute("ktr_product")?.getValue();
    var productTemplateId = primaryControl.getAttribute("ktr_producttemplate")?.getValue();

    if(!productId) {
      Xrm.Navigation.openAlertDialog(
        "Could not apply template because this Project doesn't have any Product associated."
      );
      return;
    }

    if(!productTemplateId) {
      Xrm.Navigation.openAlertDialog(
        "Could not apply template because this Project doesn't have any Product Template associated."
      );
      return;
    }

    let studies = await getProjectStudies(projectId);

    if (studies.length > 0) {
      Xrm.Navigation.openAlertDialog(
        "Could not apply template because this Project already has Studies associated with it."
      );
      return;
    }

    let questionnairelines = await getQuestionsByProject(projectId);
    let text = "Are you sure you want to proceed?";

    if (questionnairelines.length > 0) {
      text = `This project already has ${questionnairelines.length} questions associated with it. All existent questions will be inactivated. Do you want to proceed?`;
    }

    showConfirmationDialog(text).then(async function (result) {
      if (result.confirmed) {
        Xrm.Utility.showProgressIndicator("Processing...");
        await invokeCustomApi(projectId);
      }
    });
  }

  async function getProjectStudies(projectId) {

    return Xrm.WebApi.online
      .retrieveMultipleRecords(
        "kt_study",
        `?$filter=_kt_project_value eq ${projectId}&$select=kt_name`
      )
      .then((response) => {
        return response.entities;
      })
      .catch((error) => {
        Xrm.Utility.alertDialog(
          "Error retrieving existing Questions: " + error.message
        );
        return [];
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

  async function getQuestionsByProject(projectId) {
    return Xrm.WebApi.online
      .retrieveMultipleRecords(
        ql_entityLogicalName,
        `?$filter=_ktr_project_value eq ${projectId} and statecode eq 0&$select=kt_name,_ktr_module_value`
      )
      .then((response) => {
        return response.entities;
      })
      .catch((error) => {
        Xrm.Utility.alertDialog(
          "Error retrieving existing Questions: " + error.message
        );
        return [];
      });
  }

  async function invokeCustomApi(projectId) {
    let actionName = "ktr_apply_template_unbound";

    let request = {
      ktr_project_id: projectId,  // Pass the GUID directly as the parameter
      getMetadata: function () {
          return {
              boundParameter: null,  // Since it's unbound, set it to null
              parameterTypes: {
                  "ktr_project_id": { typeName: "Edm.String", structuralProperty: 1 }  // Define as a STRING
              },
              operationType: 0,  // Action (unbound operation)
              operationName: actionName  // Replace with your Custom API's name
          };
      }
    };

      // Execute the custom action
      return Xrm.WebApi.online.execute(request)
      .then(function (response) {
        return response.json(); // Convert the response to JSON
      })
      .then(function (data) {
          let pluginResponse = data.ktr_response || "[]";
          let questionsAdded = JSON.parse(pluginResponse);
          
          Xrm.Utility.alertDialog(
              `Project template applied successfully. ${questionsAdded.length} questions were added.`
          );
          Xrm.Utility.closeProgressIndicator();
          refreshGrid();
      })
      .catch(function (error) {
        Xrm.Utility.closeProgressIndicator();
        Xrm.Navigation.openAlertDialog({ text: error.message });
      });
  }

  function refreshGrid() {
    let subgridControl = Xrm.Page.getControl("Subgrid_new_1");
    if (subgridControl) {
      subgridControl.refresh();
    }
  }
  return {
    main: main
  };
})({
  ql_entityLogicalName: "kt_questionnairelines"
});
