/**
 * @file        006-QuestionNewVersionButton.js
 * @description create a new version of a question record and increment the version number by 1
 *
 * @date        2025-03-06
 * @version     1.1
 *
 * @usage       This script is invoked when click in a custom button in a Question form (in edit-mode) to create a new version.
 * @notes       This script uses Xrm Power Apps library
 */

let Sdk = window.Sdk || {};

/**
 * Request to execute a create operation
 */
Sdk.CreateRequest = function (entityName, payload) {
  this.etn = entityName;
  this.payload = payload;
};

Sdk.CreateRequest.prototype.getMetadata = function () {
  return {
    boundParameter: null,
    parameterTypes: {},
    operationType: 2, // This is a CRUD operation. Use '0' for actions and '1' for functions
    operationName: "Create"
  };
};

/*
 * Request to execute an Associate operation.
 */
Sdk.AssociateRequest = class {
  constructor(target, relatedEntities, relationship) {
    this.target = target;
    this.relatedEntities = relatedEntities;
    this.relationship = relationship;
  }
  getMetadata() {
    return {
      boundParameter: null,
      parameterTypes: {},
      operationType: 2, // Associate and Disassociate fall under the CRUD umbrella
      operationName: "Associate"
    };
  }
};

var questionNewVersionButton = (function (config) {
  const logicalTableName = config.logicalTableName;
  const questionProperties = config.tableProperties;
  const statuscodeDraftValue = config.statuscodeDraftValue;
  const statecodeActiveValue = config.statecodeActiveValue;

  function versionQuestion(primaryControl) {
    const formContext = primaryControl;
    clone(formContext);
  }

  function clone(formContext) {
    const questionId = formContext.data.entity.getId().replace(/[{}]/g, "");

    Xrm.WebApi.retrieveRecord(
      logicalTableName,
      questionId,
      `?$select=${questionProperties.name},
      ${questionProperties.statecode},
      ${questionProperties.statuscode},
      ${questionProperties.questiontype},
      ${questionProperties.defaultquestiontext},
      ${questionProperties.answerlist},
      ${questionProperties.questionrationale},
      ${questionProperties.standardorcustom},
      ${questionProperties.questiontitle},
      ${questionProperties.scripternotes},
      ${questionProperties.singleormulticode},
      ${questionProperties.methodology},
      ${questionProperties.rowsortorder},
      ${questionProperties.columnsortorder},
      ${questionProperties.answermin},
      ${questionProperties.answermax},
      ${questionProperties.questionformatdetails},
      ${questionProperties.customnotes},
      _ktr_parentquestion_value,
      ${questionProperties.questionversion}
      &$expand=ktr_Tag_kt_QuestionBank_kt_QuestionBank($select=ktr_tagid)`
    )
      .then(async function (record) {
        const parentQuestionId = record._ktr_parentquestion_value || questionId;
        const versionNumber = parseInt(record.kt_questionversion, 10) + 1;
        await validVersion(
          parentQuestionId,
          parseInt(record.kt_questionversion, 10)
        );
        const tags = record.ktr_Tag_kt_QuestionBank_kt_QuestionBank.map(
          (tag) => tag.ktr_tagid
        );

        const newQuestion = {
          [questionProperties.name]:
            record.kt_name.replace(/ - V\d+$/, "") + " - V" + versionNumber,
          [questionProperties.questiontype]: record.kt_questiontype,
          [questionProperties.defaultquestiontext]:
            record.kt_defaultquestiontext,
          [questionProperties.answerlist]: record.ktr_answerlist,
          [questionProperties.questionrationale]: record.kt_questionrationale,
          [questionProperties.standardorcustom]: record.kt_standardorcustom,
          [questionProperties.questiontitle]: record.kt_questiontitle,
          [questionProperties.scripternotes]: record.ktr_scripternotes,
          [questionProperties.singleormulticode]: record.kt_singleormulticode,
          [questionProperties.methodology]: record.kt_methodology,
          [questionProperties.statecode]: statecodeActiveValue,
          [questionProperties.statuscode]: statuscodeDraftValue,
          [questionProperties.rowsortorder]: record.ktr_rowsortorder,
          [questionProperties.columnsortorder]: record.ktr_columnsortorder,
          [questionProperties.answermin]: record.ktr_answermin,
          [questionProperties.answermax]: record.ktr_answermax,
          [questionProperties.questionformatdetails]: record.ktr_questionformatdetails,
          [questionProperties.customnotes]: record.ktr_customnotes,
          [questionProperties.questionversion]: versionNumber.toString(),
          ["ktr_ParentQuestion@odata.bind"]:
                "/kt_questionbanks(" + parentQuestionId + ")",
        };
        return Xrm.WebApi.createRecord(logicalTableName, newQuestion)
          .then(function (newRecord) {
            return { newRecord, tags };
          })
          .catch(function (error) {
            Xrm.Navigation.openAlertDialog({
              text: "Error creating Question: " + error.message
            });
          });
      })
      .then(function (newRecord) {
        if (newRecord.tags.length > 0) {
          enrichWithTags(newRecord.newRecord.id, newRecord.tags);
        }
        return newRecord.newRecord;
      })
      .then(function (newRecord) {
        Xrm.Navigation.openForm({
          entityName: logicalTableName,
          entityId: newRecord.id
        });
      })
      .catch(function (error) {
        Xrm.Navigation.openAlertDialog({
          text: "Error: " + error.message
        });
      });
  }

  function enrichWithTags(recordId, tags) {
    let target = {
      entityType: logicalTableName,
      id: recordId
    };

    let relatedEntities = tags.map((tagId) => {
      return {
        entityType: "ktr_tag",
        id: tagId
      };
    });

    let manyToManyAssociateRequest = new Sdk.AssociateRequest(
      target,
      relatedEntities,
      "ktr_Tag_kt_QuestionBank_kt_QuestionBank"
    );

    Xrm.WebApi.online
      .execute(manyToManyAssociateRequest)
      .then(function (response) {
        if (response.ok) {
          console.log("Status: %s %s", response.status, response.statusText);
        }
      })
      .catch(function (error) {
        Xrm.Navigation.openAlertDialog({
          text: "Error Associating Tags: " + error.message
        });
      });
  }

  async function validVersion(parentQuestionId, currentVersion) {
    let fetchXmlQuery = `?$apply=filter((_ktr_parentquestion_value eq ${parentQuestionId}))/aggregate(kt_questionversion with max as MaxVersion)`;

    return Xrm.WebApi.online
      .retrieveMultipleRecords("kt_questionbank", fetchXmlQuery)
      .then(
        function success(result) {
          let maxVersion = result.entities[0].MaxVersion;
          if (maxVersion > currentVersion) {
            throw new Error(
              "Can not create a new version from an older version. Current version is: " +
                maxVersion
            );
          }
        },
        function error(error) {
          console.log("Error retrieving records:", error.message);
        }
      );
  }

  return {
    versionQuestion: versionQuestion
  };
})({
  logicalTableName: "kt_questionbank",
  statuscodeDraftValue: 847610001,
  statecodeActiveValue: 1,
  tableProperties: {
    statecode: "statecode",
    statuscode: "statuscode",
    questionversion: "kt_questionversion",
    questiontype: "kt_questiontype",
    defaultquestiontext: "kt_defaultquestiontext",
    answerlist: "ktr_answerlist",
    questionrationale: "kt_questionrationale",
    standardorcustom: "kt_standardorcustom",
    questiontitle: "kt_questiontitle",
    scripternotes: "ktr_scripternotes",
    singleormulticode: "kt_singleormulticode",
    methodology: "kt_methodology",
    name: "kt_name",
    rowsortorder: "ktr_rowsortorder",
    columnsortorder: "ktr_columnsortorder",
    answermin: "ktr_answermin", 
    answermax: "ktr_answermax",
    questionformatdetails: "ktr_questionformatdetails", 
    customnotes: "ktr_customnotes"
  }
});
