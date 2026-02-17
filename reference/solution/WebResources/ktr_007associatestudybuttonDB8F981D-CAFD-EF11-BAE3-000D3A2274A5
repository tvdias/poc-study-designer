/**
 * @file        007-associate-study-button.js
 * @description Associate studies to a questionnaire line
 *
 * @date        2025-03-07
 * @version     1.0
 *
 * @usage       This script is invoked when click in a custom button in a questionnaire line form (in edit-mode) to associate studies to the questionnaire line.
 * @notes       This script uses Xrm Power Apps library
 */

let Sdk = window.Sdk || {};

/**
 * Request to execute a create operation
 */
Sdk.CreateRequest = class {
  constructor(entityName, payload) {
    this.etn = entityName;
    this.payload = payload;
  }
  getMetadata() {
    return {
      boundParameter: null,
      parameterTypes: {},
      operationType: 2, // This is a CRUD operation. Use '0' for actions and '1' for functions
      operationName: "Create"
    };
  }
};

var associateStudyButton = (function (config) {
  const lookupTableName = config.lookupTableName;
  const mainTableName = config.mainTableName;

  function main(primaryControl) {
    const formContext = primaryControl;
    const questionnairelineId = formContext.data.entity
      .getId()
      .replace(/[{}]/g, "");

    openSearchDialog(questionnairelineId)
      .then(async function (selectedStudyRecords) {
        if (selectedStudyRecords.length > 0) {
          await associateStudy(questionnairelineId, selectedStudyRecords);
        }
      })
      .then(function () {
        refreshGrid();
      })
      .catch(function (error) {
        Xrm.Navigation.openAlertDialog({
          text: "Error: " + error.message
        });
      });
  }

  async function associateStudy(questionnairelineId, studies) {
    let requests = studies.map((record) => {
      return new Sdk.CreateRequest(mainTableName, {
        ["ktr_Study@odata.bind"]:
          "/kt_studies(" + record.id.replace(/[{}]/g, "") + ")",
        ["ktr_QuestionnaireLine@odata.bind"]:
          "/kt_questionnairelineses(" + questionnairelineId + ")"
      });
    });

    return Xrm.WebApi.online
      .executeMultiple(requests)
      .then(function (response) {
        console.log("response", response);
      })
      .catch(function (error) {
        Xrm.Navigation.openAlertDialog({
          text: "Error creating: " + error.message
        });
      });
  }

  async function openSearchDialog(questionnairelineId) {
    let filterXml = await buildLookupFilter(questionnairelineId);

    let lookupOptions = {
      defaultEntityType: lookupTableName,
      entityTypes: [lookupTableName],
      allowMultiSelect: true,
      disableMru: true,
      filters: [{ filterXml: filterXml }]
    };

    return Xrm.Utility.lookupObjects(lookupOptions);
  }

  async function buildLookupFilter(questionnairelineId) {
    let questionnaireline = await getQuestionnaireRecord(questionnairelineId);

    let studies = await getStudiesRecordByQuestionnaire(questionnairelineId);

    let basefilter = `        
        <filter type="and">
            <condition attribute="kt_project" operator="eq" value="${questionnaireline._ktr_project_value}" />`;

    let excludeConditions = studies.entities
      .map((entities) => `<value>${entities.ktr_Study.kt_studyid}</value>`)
      .join("");

    if (excludeConditions) {
      basefilter += `<condition attribute="kt_studyid" operator="not-in">
                ${excludeConditions}
            </condition>`;
    }

    basefilter += `</filter>`;

    return basefilter;
  }

  async function getQuestionnaireRecord(questionnairelineId) {
    return Xrm.WebApi.retrieveRecord(
      "kt_questionnairelines",
      questionnairelineId,
      `?$select=
        kt_name,
        _ktr_project_value`
    ).then(function (record) {
      return record;
    });
  }

  async function getStudiesRecordByQuestionnaire(questionnairelineId) {
    let filter = `?$filter=_ktr_questionnaireline_value eq ${questionnairelineId}
    &$select=ktr_name
    &$expand=ktr_Study($select=kt_name)`;

    return Xrm.WebApi.retrieveMultipleRecords(mainTableName, filter).then(
      function (record) {
        return record;
      }
    );
  }

  function refreshGrid() {
    let subgridControl = Xrm.Page.getControl("assigned_studies_subgrid");
    if (subgridControl) {
      subgridControl.refresh();
    } else {
      console.error("Subgrid not found on the form.");
    }
  }

  return {
    main: main
  };
})({
  lookupTableName: "kt_study",
  mainTableName: "ktr_studyquestionnaireline"
});
