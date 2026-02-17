export class SnapshotsDataService {
  async getAssociatedSnapshots(questionID: string): Promise<any[]> {
    if (!questionID) return [];

    const query = `?$filter=_ktr_questionnaireline_value eq ${encodeURIComponent(
      `'${questionID}'`
    )}&$select=ktr_studyquestionnairelinesnapshotid`;

    try {
      const results = await Xrm.WebApi.retrieveMultipleRecords(
        "ktr_studyquestionnairelinesnapshot",
        query
      );

      return results.entities.length > 0 ? results.entities : [];
    } catch {
      return [];
    }
  }

 

}
