export enum QuestionType {
    SingleChoice = 0,
    NumericInput = 1,
    SingleChoiceMatrix = 2,
    SmallTextInput = 3,
    MultipleChoice = 4,
    DisplayScreen = 5,
    LargeTextInput = 6,
    NumericMatrix = 7,
    Logic = 8,
    MultipleChoiceMatrix = 12,
    TextInputMatrix = 13
}

export const QuestionTypeStyles: Record<QuestionType, React.CSSProperties> = {
     [QuestionType.SingleChoice]: {
        backgroundColor: '#B5E2FF',
        color: '#0674FF',
     },
     [QuestionType.NumericInput]: {
        backgroundColor: '#FFF0F0',
        color: '#FF9595',
     },
     [QuestionType.SingleChoiceMatrix]: {
        backgroundColor: '#ECD8FC',
        color: '#9E3EDF',
     },
     [QuestionType.SmallTextInput]: {
        backgroundColor: '#CDFEEC',
        color: '#00A486',
     },
     [QuestionType.MultipleChoice]: {
        backgroundColor: '#D0F6FD',
        color: '#0BA4CF',
     },
     [QuestionType.DisplayScreen]: {
        backgroundColor: '#f6f6f6',
        color: '#000000',
     },
     [QuestionType.LargeTextInput]: {
        backgroundColor: '#63F2CE',
        color: '#00836F',
     },
     [QuestionType.NumericMatrix]: {
        backgroundColor: '#FFF0F0',
        color: '#FF9595',
     },
     [QuestionType.Logic]: {
        backgroundColor: '#F7E8FF',
        color: '#F826FF',
     },
     [QuestionType.MultipleChoiceMatrix]: {
        backgroundColor: '#ECD8FC',
        color: '#9E3EDF',
     },
     [QuestionType.TextInputMatrix]: {
        backgroundColor: '#63F2CE',
        color: '#00836F',
     },
}