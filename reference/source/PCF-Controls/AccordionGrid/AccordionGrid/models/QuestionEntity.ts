export interface QuestionEntity {
  id: string;              // guid of the record
  questionVariableName: string; // question variable name field
  statusCode: number;      // statuscode field
  questionText?: string;   // kt_questiontext field
  questionType?: string;   // question type lable
  questionTitle?: string;
}
