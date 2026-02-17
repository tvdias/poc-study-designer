/**
 * @file        saveModuleDialog.js
 * @description Opens a Dialog allowing the user to choose whether to edit the current model version or create a new one.
 * 
 * @date        2025-02-17
 * @version     1.0
 * 
 * @usage       This script is invoked when saving a module inside a Module form (in edit-mode)
 * @notes       This script uses Xrm Power Apps library
 */

var Sdk = window.Sdk || {};

/**
 * Request to execute a create operation
 */
Sdk.CreateRequest = function(entityName, payload) {
    this.etn = entityName;
    this.payload = payload;
};

Sdk.CreateRequest.prototype.getMetadata = function () {
    return {
        boundParameter: null,
        parameterTypes: {},
        operationType: 2, // This is a CRUD operation. Use '0' for actions and '1' for functions
        operationName: "Create",
    };
};

var SaveModuleDialog = (function (config) {

  const lookupTableName = config.lookupTableName;
  const finalTableName = config.finalTableName;

  function openConfirmDialog(primaryControl) {
    var formContext = primaryControl;
    var moduleId = formContext.data.entity.getId();

    // If module is new (no ID), just save without showing the dialog
    if (!moduleId) {
        formContext.data.entity.save();
        return;
    }

    // Existing module: Show confirmation dialog
    Xrm.Navigation.openConfirmDialog({
        title: "Confirm Save",
        text: "Do you want to edit the current version or create a new version?",
        confirmButtonLabel: "Create New Version",
        cancelButtonLabel: "Save Current Version"
    }).then(function (response) {
        if (response.confirmed === true) {
            bumpModuleVersion(formContext);
        } else if (response.confirmed === false) {
            formContext.data.entity.save();
        }
    });
  }

  function bumpModuleVersion(formContext) {
    var moduleId = formContext.data.entity.getId().replace(/[{}]/g, "");

    // Retrieve parent module lookup correctly
    Xrm.WebApi.retrieveRecord(lookupTableName, moduleId, "?$select=kt_name,kt_moduleversionnumber,_ktr_parentmodule_value")
    .then(function (moduleRecord) {
        // Get the first version (parent module)
        var parentModuleId = moduleRecord._ktr_parentmodule_value || moduleId; // If null, set itself as V1

        // Extract module name and ensure correct versioning format
        var moduleName = moduleRecord.kt_name;
        var versionNumber = parseInt(moduleRecord.kt_moduleversionnumber, 10) + 1;
        var newModuleName = moduleName.replace(/ - V\d+$/, "") + " - V" + versionNumber;

        var newModuleData = {
            "kt_moduledescription": moduleRecord.kt_moduledescription,
            "kt_moduleinstructions": moduleRecord.kt_moduleinstructions,
            "kt_name": newModuleName,
            "kt_modulelabel": moduleRecord.kt_modulelabel,
            "kt_moduleversionnumber": versionNumber.toString(),
            ["ktr_ParentModule@odata.bind"]: "/kt_modules(" + parentModuleId + ")" // Bind the parent module correctly
        };

        return Xrm.WebApi.createRecord(lookupTableName, newModuleData);
    })
    .then(function (newModuleResult) {
        return cloneModuleQuestionBankRecords(moduleId, newModuleResult.id);
    })
    .then(function (newModuleId) {
        Xrm.Navigation.openForm({
            entityName: lookupTableName,
            entityId: newModuleId
        });
    })
    .catch(function (error) {
        Xrm.Navigation.openAlertDialog({ text: "Error: " + error.message });
    });
  }
  
  function cloneModuleQuestionBankRecords(oldModuleId, newModuleId) {
    return Xrm.WebApi.retrieveMultipleRecords(finalTableName, "?$filter=ktr_Module/kt_moduleid eq '{" + oldModuleId + "}'")
      .then(function (response) {
        var createRequests = response.entities.map((record) => {
          return new Sdk.CreateRequest(
            finalTableName,
            {
              ["ktr_Module@odata.bind"]: "/kt_modules(" + newModuleId + ")",
              "ktr_name": record.ktr_name,
              "ktr_sortorder": record.ktr_sortorder,
              ["ktr_QuestionBank@odata.bind"]: "/kt_questionbanks(" + record._ktr_questionbank_value + ")"
            }
          );
        });

        return Xrm.WebApi.online.executeMultiple(createRequests);
      })
      .then(() => newModuleId);
  }

  return {
    openConfirmDialog: openConfirmDialog,
  };
})({
  lookupTableName: 'kt_module',
  finalTableName: 'ktr_modulequestionbank'
});
