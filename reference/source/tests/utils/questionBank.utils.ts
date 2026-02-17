import { QuestionCreationForm } from "../models/questionCreationForm";
import { Utils } from "./utils";

export class QuestionBankUtils {
  static CreateQuestionCreationForm(): QuestionCreationForm {
    const questionGuid = Utils.generateGUID();
    return {
      questionName: `Question Name ${questionGuid}`,
      questionType: "Categorical multi",
      questionText: `Question Text Test ${questionGuid}`,
      questionAnswer: `Question Answer Test ${questionGuid}`,
      questionTitle: `Question Title Test ${questionGuid}`,
      questionRationale: `Question Rationale Test ${questionGuid}`,
      questionScriptorNotes: `Question Scriptor Notes Test ${questionGuid}`,
      questionStandardOrCustom: "Standard",
      questionSingleOrMulticode: "Singlecode",
    };
  }
}
