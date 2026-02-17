import type { QuestionEntity } from "../models/QuestionEntity";
import type { RowEntity } from "../models/RowEntity";
import { sanitizeGuid } from "../utils/StringHelper";

export class QuestionDataService {

    private _webAPI: ComponentFramework.WebApi;

    private STATUS_ACTIVE = 1;

    constructor(webAPI: ComponentFramework.WebApi) {
        this._webAPI = webAPI;
    }

    /**
     * Retrieve all active kt_questionbank records, excluding any already in RowEntity[]
     */
    async getActiveQuestions(existingRows: RowEntity[], isScripter: boolean, standardOrCustom?: 0 | 1, projectId?: string): Promise<{ questions: QuestionEntity[]; count: number }> {
        try {
            // Build a set of question variable names that are already used in the subgrid
            const existingVarNames = new Set(
                existingRows
                    .map(r => r.lastLabelText?.toLowerCase())
                    .filter(Boolean)
            );

            // Use passed projectId if available
            const rawProjectId = projectId || existingRows[0]?.projectId;
            const sanitizedProjectId = sanitizeGuid(rawProjectId);

            if (sanitizedProjectId) {
                try {
                    const projectFilter = `_ktr_project_value eq '${sanitizedProjectId}'`;
                    const query = `?$select=kt_questionvariablename&$filter=${projectFilter}`;
                    const allLinesResult = await this._webAPI.retrieveMultipleRecords("kt_questionnairelines", query);

                    for (const line of allLinesResult.entities || []) {
                        const nameRaw = line.kt_questionvariablename;
                        const name = nameRaw?.toLowerCase();
                        if (name) {
                            existingVarNames.add(name);
                        }
                    }
                } catch (innerError) {
                    console.warn("getActiveQuestions - Could not retrieve all questionnaire lines:", innerError);
                }
            }
            else {
                console.warn("getActiveQuestions - no projectId found; skipping project-level check.");
            }

            let filter = `statuscode eq ${this.STATUS_ACTIVE}`;
            if (standardOrCustom !== undefined) {
                filter += ` and kt_standardorcustom eq ${standardOrCustom}`;
            }
 
            if (isScripter && standardOrCustom == 0) {
                filter += ` and kt_isdummyquestion eq true`;

            }
       

            const result = await this._webAPI.retrieveMultipleRecords(
                "kt_questionbank",
                `?$select=kt_questionbankid,kt_name,kt_defaultquestiontext,statuscode,kt_questiontype,kt_questiontitle
                 &$filter=${filter}`
            );
            const questions = (result.entities || [])
                .map(question => ({
                    id: question.kt_questionbankid,
                    questionVariableName: question.kt_name,
                    questionText: question.kt_defaultquestiontext,
                    statusCode: question.statuscode,
                    questionType: question["kt_questiontype@OData.Community.Display.V1.FormattedValue"] || "",
                    questionTitle: question.kt_questiontitle
                }))
                .filter(q => {
                    const name = q.questionVariableName?.toLowerCase();
                    const excluded = !!name && existingVarNames.has(name);
                    if (excluded) console.log("getActiveQuestions - excluding question (duplicate):", q.questionVariableName);
                    return !excluded;
                });

            return { questions, count: questions.length };
        } catch (error) {
            console.error("getActiveQuestions - Error retrieving questions:", error);
            return { questions: [], count: 0 };
        }
    }

   async deleteQuestionnaireLines(questionID: string): Promise<{ success: boolean; error?: any }> {
        if (!questionID) {
            return { success: false, error: "No question ID provided" };
        }

          try {
            await Xrm.WebApi.deleteRecord("kt_questionnairelines", questionID);
            return { success: true };
          } catch (error) {
            console.error("Error deleting questionnaire line:", error);
            return { success: false, error };
          }
        }

    /**
     * [Helper Method] Retrieve all questionnaire line records for Module data service.
     */
    async GetQuestionsModulesByProject(projectId?: string): Promise<any[]> {
        if (!projectId) return [];

        const sanitizedId = sanitizeGuid(projectId);

        try {
            const query = `?$select=_ktr_module_value&$filter=_ktr_project_value eq '${sanitizedId}'`;
            const result = await this._webAPI.retrieveMultipleRecords("kt_questionnairelines", query);
            return result.entities || [];
        } catch (error) {
            console.error("getQuestionsByProject - failed to fetch questionnaire lines:", error);
            return [];
        }
    }

}