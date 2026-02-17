import { AnswerType } from "../types/DetailView/QuestionnaireLinesAnswerList";

export interface AnswerEntity {
  answerText: string;
  answerCode: string;
  flags: string;
  order: number;
  type: AnswerType;
}