import { AnswerEntity } from "../models/AnswerEntity";
import { AnswerType } from "../types/DetailView/QuestionnaireLinesAnswerList";

export class AnswerDataService {

  private _webAPI: ComponentFramework.WebApi;

  constructor(webAPI: ComponentFramework.WebApi) {
      this._webAPI = webAPI;
  }
  
  async fetchAnswers(questionID: string): Promise<AnswerEntity[]> {
    if (!questionID) return [];

    const query = `?$filter=_ktr_questionnaireline_value eq ${encodeURIComponent(
      `'${questionID}'`
    )} and statecode eq 0&$orderby=ktr_displayorder asc&$select=ktr_answertext,ktr_answercode,ktr_displayorder,ktr_isfixed,ktr_isexclusive,ktr_isopen,ktr_answertype`;

    try {
      const results = await this._webAPI.retrieveMultipleRecords(
        "ktr_questionnairelinesanswerlist",
        query
      );

      return results.entities.map((row: any) => {
        const flags: string[] = [];
        if (row.ktr_isfixed) flags.push("FIXED");
        if (row.ktr_isexclusive) flags.push("EXCLUSIVE");
        if (row.ktr_isopen) flags.push("OPEN");

        return {
          answerText: row.ktr_answertext ?? "",
          answerCode: row.ktr_answercode ?? "",
          flags: flags.join(", "),
          order: row.ktr_displayorder ?? 0,
          type:
            row.ktr_answertype === AnswerType.Column
              ? AnswerType.Column
              : AnswerType.Row,
        } as AnswerEntity;
      });
    } catch (error) {
      console.error("Error fetching answers:", error);
      return [];
    }
  }

  // Fetch ktr_managedlist records associated with a questionnaire line via
  // ktr_questionnairelinesharedlist linking entity. Returns de-duplicated array
  // of simplified objects { id, name }.
  async fetchManagedLists(questionLineId: string): Promise<{ id: string; name: string; location: string }[]> {
    if (!questionLineId) return [];
    // First fetch active linking (shared list) records; then separately verify each managed list is active.
    const query = `?$filter=_ktr_questionnaireline_value eq ${encodeURIComponent(`'${questionLineId}'`)} and statecode eq 0&$select=_ktr_managedlist_value,ktr_location`;
    try {
      const results = await this._webAPI.retrieveMultipleRecords("ktr_questionnairelinesharedlist", query);
      const seen = new Set<string>();
      const managedListEntityName = "ktr_managedlist";
      const output: { id: string; name: string; location: string }[] = [];
      for (const row of results.entities) {
        const id: string = row._ktr_managedlist_value;
        if (!id || seen.has(id)) continue;
        seen.add(id);
        const location: string = row["ktr_location@OData.Community.Display.V1.FormattedValue"] || row.ktr_location || "";
        // Try to retrieve managed list record to confirm active state
        try {
          const ml = await this._webAPI.retrieveRecord(managedListEntityName, id, "?$select=statecode,ktr_name");
          if (ml.statecode !== 0) continue; // skip inactive managed list
          const name: string = ml.ktr_name || row["_ktr_managedlist_value@OData.Community.Display.V1.FormattedValue"] || id;
          output.push({ id, name, location });
        } catch (innerErr) {
          console.warn("Failed to retrieve managed list record", id, innerErr);
        }
      }
      return output;
    } catch (error) {
      console.error("Error fetching managed lists:", error);
      return [];
    }
  }
}
