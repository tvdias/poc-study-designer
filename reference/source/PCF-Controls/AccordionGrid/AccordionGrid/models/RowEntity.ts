import { StandardOrCustom } from "../types/DetailView/QuestionnaireLinesChoices";
import type { QuestionType } from "../types/QuestionType";

export interface RowEntity {
  id: string;
  name: string;
  sortOrder: number;
  statusCode: number;
  firstLabelText: string;
  firstLabelId: QuestionType;
  middleLabelText: string; //module name
  lastLabelText: string; //question variable name
  projectId: string;
  questionTitle: string,
  questionFormatDetail: string,
  answerMin: number,
  answerMax: number,
  isDummy : string,
  answerList: string,
  scripterNotes: string,
  rowSortOrder:string,
  columnSortOrder:string,
  standardOrCustomText: string,
  standardOrCustomId: StandardOrCustom,
  questionVersion?: string,
  questionRationale?: string
}