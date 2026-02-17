/**
 * @file        053-ManagedListEntityTriggerAnswerCodeFlow.js
 * @description  Validate and Trigger the Generate Answer Code for Managed List Entities flow.
 * @date        2026-01-12
 * @version     1.1
 *
 * @usage       usage on the OnSave event of the Managed List Entity form.
 * @notes       If the Answer Text field is modified and conditions are met, the flow is triggered by setting the ktr_shouldgenerateanswercode field to true.
 * The flow will not be triggered if the Answer Code field is modified.
 * Only triggers on Edit forms.
 * Scripter and System Administrator roles bypass certain checks. However, Scripter restrictions will be enforced in 044-ManagedListEntityAnswercodeScript.js
 * @author      Sean Donato
 */

var TriggerFlow = (function () {
  "use strict";
  const Role_Scripter = "3a27fcc5-cc0a-f011-bae2-000d3a2274a5";
  const Role_CSUser = "c567cc6b-210a-f011-bae2-000d3a2274a5";
  const Role_SystemAdministrator = "7da62815-9506-f011-bae4-7c1e52277c6c";

  async function triggerGenerateAnswerCodeFlow(executionContext) {
    const formContext = executionContext.getFormContext();
    const formType = formContext.ui.getFormType();

    if (formType !== 2) {
      // Edit Form
      return;
    }
    const recordId = Xrm.Page.data.entity.getId();

    const mlEntity = await getManagedListEntityByIdAsync(recordId);
    console.log("Retrieved Managed List Entity:", mlEntity);

    const ml = await getManagedListEnByIdAsync(mlEntity._ktr_managedlist_value);
    console.log("Retrieved Managed List:", ml);

    var hasAnswerCodeChanged = await answerCodeHasChange(
      executionContext,
      mlEntity
    );
    console.log("Has Answer Code Changed:", hasAnswerCodeChanged);
    var hasAnswerTextChanged = await answerTextHasChange(
      executionContext,
      mlEntity
    );
    console.log("Has Answer Text Changed:", hasAnswerTextChanged);
    // Check conditions to trigger the flow
    if (hasAnswerCodeChanged) {
      console.log("Answer Code has changed. Exiting without triggering flow.");
      return;
    }
    if (hasAnswerTextChanged) {
      if (
        ml.ktr_answercodeeditable === true &&
        userHasRoleById([Role_CSUser]) &&
        mlEntity.ktr_everinsnapshot === false
      ) {
        console.log(
          "Triggering Generate Answer Code Flow for CS User on Editable Answer Code Managed List Entity not in Snapshot."
        );
        await updateFlag(recordId);
        return;
      }
      if (userHasRoleById([Role_Scripter])) {
        console.log(
          "Triggering Generate Answer Code Flow for Scripter Role regardless of other conditions."
        );
        await updateFlag(recordId);
        return;
      }
      if (userHasRoleById([Role_SystemAdministrator])) {
        console.log(
          "Triggering Generate Answer Code Flow for System Administrator Role regardless of other conditions."
        );
        await updateFlag(recordId);
      }
    }
  }

  function userHasRoleById(targetRoleIds) {
    const roles = Xrm.Utility.getGlobalContext().userSettings.roles;
    for (let i = 0; i < roles.getLength(); i++) {
      const role = roles.get(i);
      if (targetRoleIds.includes(role.id)) {
        return true;
      }
    }
    return false;
  }

  async function answerCodeHasChange(executionContext, mlEntity) {
    var formContext = executionContext.getFormContext();
    var answerCodeField = formContext.getControl("ktr_answercode");
    var hasChanges =
      answerCodeField.getAttribute().getValue() !== mlEntity.ktr_answercode;
    return hasChanges;
  }

  async function answerTextHasChange(executionContext, mlEntity) {
    var formContext = executionContext.getFormContext();
    var answerTextField = formContext.getControl("ktr_answertextvalue");
    var hasChanges =
      answerTextField.getAttribute().getValue() !== mlEntity.ktr_answertext;
    return hasChanges;
  }

  async function getManagedListEntityByIdAsync(recordId) {
    return getRecordByIdAsync("ktr_managedlistentity", recordId, [
      "ktr_managedlistentityid",
      "ktr_shouldgenerateanswercode",
      "_ktr_managedlist_value",
      "ktr_everinsnapshot",
      "ktr_answercode",
      "ktr_answertext",
      "statecode",
      "statuscode"
    ]);
  }

  async function getManagedListEnByIdAsync(recordId) {
    return getRecordByIdAsync("ktr_managedlist", recordId, [
      "ktr_managedlistid",
      "ktr_answercodeeditable"
    ]);
  }

  async function updateFlag(recordId) {
    console.log(
      "Updating ktr_shouldgenerateanswercode flag to true for record:",
      recordId
    );
    const data = {
      ktr_shouldgenerateanswercode: true
    };

    return Xrm.WebApi.updateRecord(
      "ktr_managedlistentity",
      recordId.replace(/[{}]/g, ""),
      data
    );
  }

  async function getRecordByIdAsync(entityLogicalName, recordId, selectFields) {
    return Xrm.WebApi.retrieveRecord(
      entityLogicalName,
      recordId.replace(/[{}]/g, ""),
      "?$select=" + selectFields.join(",")
    );
  }

  return {
    triggerGenerateAnswerCodeFlow: triggerGenerateAnswerCodeFlow
  };
})();
