/**
 * @file        008-searchAndAddProjectQuestions.js
 * @description Opens a Search Dialog of entity 'QuestionBank' and allows user to select records.
 * If the record is duplicated then it opens an Error dialog, otherwhise it will open a Success dialog.
 *              
 * @date        2025-02-28
 * @version     1.0
 * 
 * @usage       This script is being called when Adding Questions inside a Project form (in edit-mode)
 * @notes       This script uses Xrm Power Apps library
*/
var SearchQuestionBankProject = (function (config) {

    const lookupTableName = config.lookupTableName;
    const finalTableName = config.finalTableName;
    const gridId = config.gridId;
    const fields = config.fields;
    const finalTableRelationshipField = config.finalTableRelationshipField;

    function openSearchDialog() {
        var projectId = getProjectId();
        if (!projectId) {
            Xrm.Navigation.openAlertDialog({
                text: "Error: Unable to retrieve the Questionnaire ID.",
            });
            return;
        }

        // Step 1: Get all questions already on the questionnaire
        Xrm.WebApi.retrieveMultipleRecords(
            "kt_questionnairelines",
            `?$select=kt_questionvariablename&$filter=_ktr_project_value eq ${projectId}`
        ).then(function (existingLinesResult) {
            var excludedIds = existingLinesResult.entities
                .map(e => e.kt_questionvariablename)
                .filter(id => !!id)
                .map(name => name.trim().toLowerCase());

            //Step 2: Build the filter XML to exclude existing records by ID using 'ne' in 'and' filter
            var conditionsXml = "";
            excludedIds.forEach(function (name) {
                conditionsXml += `<condition attribute="kt_name" operator="ne" value="${name}" />`;
            });

            // Add condition for active records (statecode = 0)
            conditionsXml += `<condition attribute="statecode" operator="eq" value="0" />`;

            var filterXml = `<filter type="and">${conditionsXml}</filter>`;

            var lookupOptions = {
                defaultEntityType: lookupTableName,
                entityTypes: [lookupTableName],
                allowMultiSelect: true,
                disableMru: true,
                filters: [{
                    filterXml: filterXml,
                    entityLogicalName: lookupTableName
                }]
            };

            Xrm.Utility.lookupObjects(lookupOptions).then(function (selectedRecords) {
                if (!selectedRecords || selectedRecords.length === 0) return;

                var promises = selectedRecords.map(rec => createQuestionLineFromRecord(rec.id, projectId));
                Promise.allSettled(promises).then(results => {
                    if (results.some(r => r.status === "fulfilled")) {
                        Xrm.Navigation.openAlertDialog({ text: "Records added successfully!" });
                    } else {
                        Xrm.Navigation.openErrorDialog({ message: "Records could not be added." });
                    }
                    refreshQuestionLinesGrid();
                });
            },
                function (error) {
                    console.error("Error opening lookup:", error);
                });
        });
    }

    function createQuestionLineFromRecord(recordId, questionnaireId) {
        return retrieveQuestionBankRecord(recordId).then((record) =>
            createQuestionLine(record, questionnaireId)
        );
    }

    function getProjectId() {
        if (Xrm.Page && Xrm.Page.data && Xrm.Page.data.entity) {
            return Xrm.Page.data.entity.getId().replace(/[{}]/g, "");
        } else if (Xrm.Utility.getPageContext) {
            return Xrm.Utility.getPageContext().input.entityId.replace(/[{}]/g, "");
        }
        return null;
    }

    function retrieveQuestionBankRecord(recordId) {
        const selectFields = fields.map(field => field.from).join(',');

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
        } else {
            console.error("Subgrid not found on the form.");
        }
    }

    function createQuestionLine(questionBankRecord, projectId) {
        const defaultRecord = {
            ["ktr_Project" + '@odata.bind']: "/" + finalTableRelationshipField.toLowerCase() + "s(" + projectId + ")"
        }

        const newRecord = fields.reduce((acc, curr) => {
            const recordValue = questionBankRecord[curr.from];

            if (recordValue == null) return acc;

            return {
                ...acc,
                [curr.to]: recordValue,
            }
        }, defaultRecord);

        return Xrm.WebApi.createRecord(finalTableName, newRecord).then(
            (result) => {
                //console.log("Question Line created with ID:", result.id);
                return createAnswerLine(questionBankRecord, result.id)
            }
        );
    }

    function createAnswerLine(questionBankRecord, questionnaireLine) {
        return Xrm.WebApi.online.retrieveMultipleRecords(
            "ktr_questionanswerlist",
            `?$filter=_ktr_kt_questionbank_value eq ${questionBankRecord.kt_questionbankid}&$select=ktr_name,ktr_answerid,ktr_customproperty,ktr_displayorder,ktr_effectivedate,ktr_enddate,ktr_isactive,ktr_isexclusive,ktr_answertext,ktr_answertype,ktr_isopen,ktr_isfixed,ktr_istranslatable,statecode,statuscode,ktr_version`
        ).then(response => {
            // create all answers in parallel
            const createAnswersPromises = response.entities.map((answerRecord) => {
                const answerLineRecord = {
                    ktr_name: answerRecord.ktr_name,
                    ktr_answercode: answerRecord.ktr_name,
                    ktr_answerid: answerRecord.ktr_answerid,
                    ktr_answertype: answerRecord.ktr_answertype,
                    ktr_customproperty: answerRecord.ktr_customproperty,
                    ktr_displayorder: answerRecord.ktr_displayorder,
                    ktr_effectivedate: answerRecord.ktr_effectivedate,
                    ktr_enddate: answerRecord.ktr_enddate,
                    ktr_isactive: answerRecord.ktr_isactive,
                    ktr_isexclusive: answerRecord.ktr_isexclusive,
                    ktr_answertext: answerRecord.ktr_answertext,
                    ktr_isopen: answerRecord.ktr_isopen,
                    ktr_isfixed: answerRecord.ktr_isfixed,
                    ktr_istranslatable: answerRecord.ktr_istranslatable,
                    statecode: answerRecord.statecode,
                    statuscode: answerRecord.statuscode,
                    ktr_version: answerRecord.ktr_version,
                    ["ktr_QuestionnaireLine@odata.bind"]: "/kt_questionnairelineses(" + questionnaireLine + ")",
                    ["ktr_QuestionBank@odata.bind"]: "/kt_questionbanks(" + questionBankRecord.kt_questionbankid + ")",
                    ["ktr_QuestionAnswer@odata.bind"]: "/ktr_questionanswerlists(" + answerRecord.ktr_questionanswerlistid + ")"
                };

                return Xrm.WebApi.createRecord("ktr_questionnairelinesanswerlist", answerLineRecord)
                    .then(result => {
                        //console.log("AnswerLine created ===>", result);
                    })
                    .catch(error => {
                        console.error("Error creating answer line:", error.message);
                    });
            });
            // wait for all answeres to be created
            return Promise.allSettled(createAnswersPromises);

        }).catch(error => {
            console.error("Error fetching answer list:", error.message);
        });
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
        { from: 'kt_questionversion', to: 'ktr_questionversion' },
        { from: 'ktr_rowsortorder', to: 'ktr_rowsortorder' },
        { from: 'ktr_columnsortorder', to: 'ktr_columnsortorder' },
        { from: 'ktr_answermin', to: 'ktr_answermin' },
        { from: 'ktr_answermax', to: 'ktr_answermax' },
        { from: 'ktr_questionformatdetails', to: 'ktr_questionformatdetails' },
        { from: 'ktr_customnotes', to: 'ktr_customnotes' },
        { from: 'kt_isdummyquestion', to: 'ktr_isdummyquestion' }

    ],

    finalTableRelationshipField: 'kt_Project',
});
