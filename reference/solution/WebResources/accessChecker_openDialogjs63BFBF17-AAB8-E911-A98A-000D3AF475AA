/**
 * @file        searchQuestionBankDialog.js
 * @description Opens a Search Dialog of entity 'QuestionBank' and allows user to select records.
 * If the record is duplicated then it opens an Error dialog, otherwhise it will open a Success dialog.
 *              
 * @date        2025-02-05
 * @version     1.0
 * 
 * @usage       This script is being called when Adding Questions inside a Questionnaire form (in edit-mode)
 * @notes       This script uses Xrm Power Apps library
*/
var SearchQuestionBankModule = (function (config) {
  
  const lookupTableName = config.lookupTableName;
  const finalTableName = config.finalTableName;
  const gridId = config.gridId;
  const fields = config.fields;
  const finalTableRelationshipField = config.finalTableRelationshipField;

  function openSearchDialog() {
    var lookupOptions = {
      defaultEntityType: lookupTableName,
      entityTypes: [lookupTableName],
      allowMultiSelect: true,
    };

    Xrm.Utility.lookupObjects(lookupOptions).then(
      function (success) {
        //console.log("Selected recordssss:", success);

        if (success.length == 0) {
          return;
        }

        var questionnaireId = getQuestionnaireId();
        if (!questionnaireId) {
          Xrm.Navigation.openAlertDialog({
            text: "Error: Unable to retrieve the Questionnaire ID.",
          });
          return;
        }

        const retrievePromises = success.map((selectedRecord) =>
          createQuestionLineFromRecord(selectedRecord.id, questionnaireId)
        );

        Promise.allSettled(retrievePromises).then((results) => {
          const hasAnySuccess = results.some(
            (result) => result.status === "fulfilled"
          );

          //console.log("results => ", results);

          if (hasAnySuccess) {
            Xrm.Navigation.openAlertDialog({
              text: "Records added successfully!",
            });
          } else {
            Xrm.Navigation.openErrorDialog({
              message: "Records could not be added.",
            });
          }

          refreshQuestionLinesGrid();
        });
      },
      function (error) {
        console.error("Error opening lookup:", error);
      }
    );
  }

  function createQuestionLineFromRecord(recordId, questionnaireId) {
    return retrieveQuestionBankRecord(recordId).then((record) =>
      createQuestionLine(record, questionnaireId)
    );
  }

  function getQuestionnaireId() {
    if (Xrm.Page && Xrm.Page.data && Xrm.Page.data.entity) {
      return Xrm.Page.data.entity.getId().replace(/[{}]/g, "");
    } else if (Xrm.Utility.getPageContext) {
      return Xrm.Utility.getPageContext().input.entityId.replace(/[{}]/g, "");
    }
    return null;
  }

  function retrieveQuestionBankRecord(recordId) {
    const selectFields = fields.map(field => field.from).join(',');
    //console.log("selectFields => ", selectFields);

    return Xrm.WebApi.retrieveRecord(
      lookupTableName,
      recordId.replace(/[{}]/g, ""),
      "?$select=" + selectFields
    );
  }

  function refreshQuestionLinesGrid() {
    var subgridName = gridId;
    var subgridControl = Xrm.Page.getControl(subgridName);
    if (subgridControl) {
      subgridControl.refresh();
      //console.log("Questionnaire Lines subgrid refreshed.");
    } else {
      console.error("Subgrid not found on the form.");
    }
  }
  
  function createQuestionLine(questionBankRecord, questionnaireId) {
    const defaultRecord = {
       [finalTableRelationshipField + '@odata.bind']: "/" + finalTableRelationshipField.toLowerCase() + "s(" + questionnaireId + ")"
    }

    const newRecord = fields.reduce((acc, curr) => {
      const recordValue = questionBankRecord[curr.from];

      if (recordValue == null) return acc;

      return {
        ...acc,
        [curr.to]: recordValue,
      }
    }, defaultRecord);

    //console.log("QUESTION LINE 1: ", newRecord);

    return Xrm.WebApi.createRecord(finalTableName, newRecord).then(
      (result) => {
        //console.log("Question Line created with ID:", result.id);
      }
    );
  }

  return {
    openSearchDialog: openSearchDialog,
  };
})({
  lookupTableName: 'kt_questionbank',
  finalTableName: 'kt_questionnairelines',
  gridId: 'Subgrid_new_1',
  fields: [
    { from: 'kt_name', to: 'kt_questionvariablename' },
    { from: 'kt_questiontype', to: 'kt_questiontype' },
    { from: 'kt_defaultquestiontext', to: 'kt_questiontext2' },
    { from: 'ktr_answerlist', to: 'ktr_answerlist' },
    { from: 'kt_questiontitle', to: 'kt_questiontitle' },
    { from: 'kt_standardorcustom', to: 'kt_standardorcustom' },
    { from: 'kt_questionrationale', to: 'ktr_questionrationale' },
    { from: 'ktr_scripternotes', to: 'ktr_scripternotes' },
    { from: 'kt_questionversion', to: 'ktr_questionversion' }
  ],
  finalTableRelationshipField: 'kt_Questionnaire',
});
