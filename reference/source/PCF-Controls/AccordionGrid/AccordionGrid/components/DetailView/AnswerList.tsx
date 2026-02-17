import * as React from "react";
import { AnswerType } from "../../types/DetailView/QuestionnaireLinesAnswerList";
import { AnswerListTableProps } from "../../models/props/AnswerListProps";
import { AnswerEntity } from "../../models/AnswerEntity";
import { AnswerDataService } from "../../services/AnswerDataService";
import { EntityHelper } from "../../utils/EntityHelper";

export const AnswerListTable: React.FC<AnswerListTableProps> = ({
  questionID,
  context
}) => {
  const [answers, setAnswers] = React.useState<AnswerEntity[] | null>(null);
  const [managedLists, setManagedLists] = React.useState<{ id: string; name: string; location: string; url?: string }[]>([]);

  React.useEffect(() => {
    const loadAnswers = async () => {
      let answerService = new AnswerDataService(context.webAPI);
      const data = await answerService.fetchAnswers(questionID);
      setAnswers(data);
      const ml = await answerService.fetchManagedLists(questionID);
      // Generate full edit URL for each managed list using EntityHelper
      const mlWithUrls = await Promise.all(
        ml.map(async m => ({
          ...m,
          url: await EntityHelper.generateEditUrl(context, "ktr_managedlist", m.id)
        }))
      );
      setManagedLists(mlWithUrls);
    };
    if (questionID) {
      loadAnswers();
    }
  }, [questionID]);

  if (!answers) {
    return null; // still fetching
  }

  const renderTable = (list: AnswerEntity[], title: string, managedListsForLocation: { id: string; name: string; location: string; url?: string }[]) => {
    if (list.length === 0 && managedListsForLocation.length === 0) return null;

    return (
      <div style={{ fontFamily: "Arial", fontSize: "9pt", marginBottom: "15px" }}>
        <div style={{ textAlign: "left", fontWeight: "bold", margin: "10px 0 4px 0" }}>
          {title}
        </div>
        {/* Single combined table: managed lists first (if any), then answers */}
        <table
          cellPadding={0}
          cellSpacing={0}
          style={{
            fontFamily: "Arial",
            borderCollapse: "collapse",
            border: "1px solid #ccc",
            tableLayout: "fixed",
            width: "600px",
            maxWidth:"600px"
          }}
        >
          <tbody>
            {managedListsForLocation.map(ml => (
              <tr key={"ml-" + ml.id}>
                <td
                  style={{
                    padding: "4px",
                    width: "330px",
                    maxWidth: "330px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                  }}
                >
                  Managed List: {ml.name || ml.id}
                </td>
                <td
                  style={{
                    padding: "4px",
                    width: "200px",
                    maxWidth: "200px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                  }}
                >
                  <a
                    href={ml.url || "#"}
                    target="_blank"
                    rel="noopener noreferrer"
                    style={{ textDecoration: 'underline', color: '#0F6CBD' }}
                  >{ml.name || ml.id}</a>
                </td>
                <td
                  style={{
                    padding: "4px",
                    width: "200px",
                    maxWidth: "200px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                  }}
                >
                </td>
              </tr>
            ))}
            {list.map((item, index) => (
              <tr key={index}>
                <td
                  style={{
                    padding: "4px",
                    width: "330px",
                    maxWidth: "330px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                  }}
                >
                  {item.answerText}
                </td>
                <td
                  style={{
                    padding: "4px",
                    width: "200px",
                    maxWidth: "200px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                   
                  }}
                >
                  {item.answerCode}
                </td>
                <td
                  style={{
                    padding: "4px",
                    width: "200px",
                    maxWidth: "200px",
                    wordWrap: "break-word",
                    whiteSpace: "normal",
                    border: "1px solid #ccc",
                    fontFamily: "Arial",
                   
                  }}
                >
                  {item.flags}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  };

  const rowAnswers = answers.filter((a) => a.type === AnswerType.Row);
  const colAnswers = answers.filter((a) => a.type === AnswerType.Column);
  const managedListsRows = managedLists.filter(m => (m.location || '').toLowerCase().includes('row'));
  const managedListsColumns = managedLists.filter(m => (m.location || '').toLowerCase().includes('column'));

  return (
    <div style={{ fontFamily: "Arial", fontSize: "9pt" }}>
      {renderTable(rowAnswers, "Rows", managedListsRows)}
      {renderTable(colAnswers, "Columns", managedListsColumns)}
    </div>
  );
};
function loadAnswersAnswers() {
  throw new Error("Function not implemented.");
}

