import * as React from 'react';
import { makeStyles, tokens, Tag } from '@fluentui/react-components';
import { QuestionType, QuestionTypeStyles } from '../../types/QuestionType';
import { RowEntity } from '../../models/RowEntity';
import { StandardOrCustomStyles } from '../../types/DetailView/QuestionnaireLinesChoices';
import { AnswerListTable } from "./AnswerList";
import { DetailsViewProps } from '../../models/props/DetailsViewProps';


const useStyles = makeStyles({
  mainContainer: {

    gap: '20px',
    padding: '20px 20px 0px 20px',
  },
  content: {
    display: 'flex',
    flex: 1,
    gap: '20px',
    padding: '20px 20px 0px 20px',
    wordWrap: 'break-word'

  },
  smallColumn: {
    flex: '0 0 30%',
    maxWidth: '30%',
    boxSizing: 'border-box',
    wordWrap: 'break-word',
  },
  halfColumn: {
    flex: '0 0 60%',
    maxWidth: '50%',
    boxSizing: 'border-box',
  },
  labelTextLeft: {
    borderRadius: tokens.borderRadiusXLarge,
    paddingTop: tokens.spacingHorizontalXS,
    paddingBottom: tokens.spacingHorizontalXS,
    paddingLeft: tokens.spacingHorizontalS,
    paddingRight: tokens.spacingHorizontalS,
    display: 'inline-block',
    fontWeight: tokens.fontWeightRegular,
    marginRight: tokens.spacingHorizontalM,
    fontSize: '11px !important',
  },

  heading: {
    fontWeight: tokens.fontWeightSemibold,
    color: '#000000',
    padding: '5px 5px 5px 0px',
    fontSize: '14px'
  },

  input: {
    color: '#6D6D6D',
    overflowWrap: 'break-word',
    wordBreak: 'break-word',
    whiteSpace: 'normal',
    display: 'block',
    maxWidth: '100%',
    fontSize: '14px'
  },


})

export const DetailsView: React.FC<DetailsViewProps> = ({
      row,
      context
}) => {
  let question = row.firstLabelText;
  let isDummy = row.isDummy === "True" ? "Dummy" : "";
  let isDummyVisible = isDummy ? "block" : "none";
  let restrictionVisible =
    [QuestionType.MultipleChoice, QuestionType.NumericInput, QuestionType.NumericMatrix].includes(row.firstLabelId)
      ? "block"
      : "none";
  let rowSortOrderVisible =
    [QuestionType.MultipleChoice, QuestionType.MultipleChoiceMatrix, QuestionType.NumericMatrix, QuestionType.SingleChoice, QuestionType.SingleChoiceMatrix, QuestionType.TextInputMatrix].includes(row.firstLabelId)
      ? "block"
      : "none";

  let columnSortOrderVisible =
    [QuestionType.MultipleChoiceMatrix, QuestionType.SingleChoiceMatrix].includes(row.firstLabelId)
      ? "block"
      : "none";

  const style = useStyles();

  return (

    <div className={style.mainContainer}>
      <div className={style.content}>
        <div className={style.smallColumn}>
          <p className={style.heading}>Question Type</p>
          <div style={{ display: 'flex', gap: '4px', flexWrap: 'wrap' }}>
            <Tag className={style.labelTextLeft}
              style={StandardOrCustomStyles[row.standardOrCustomId]}
            >
              {row.standardOrCustomText}
            </Tag>
            <Tag className={style.labelTextLeft}
              style={QuestionTypeStyles[row.firstLabelId]}
            >
              {question}
            </Tag>
            <Tag className={style.labelTextLeft} style={{ display: isDummyVisible }}>{isDummy}</Tag>
          </div>
        </div>
        <div className={style.smallColumn}>
          <p className={style.heading}>Variable Name</p>
          <Tag className={style.labelTextLeft}>
            {row.lastLabelText}
          </Tag>
        </div>
      </div>

      <div className={style.content}>
        <div className={style.smallColumn} >
          <p className={style.heading}>Question Title</p>
          <p className={style.input}>{row.questionTitle}</p>
        </div>
        <div className={style.smallColumn}>
          <p className={style.heading}>Question Text</p>
          <p className={style.input}>{row.name}</p>
        </div>
        <div className={style.smallColumn} style={{ display: restrictionVisible }}>
          <p className={style.heading}>Restrictions</p>
          <p className={style.input}>{row.answerMin} | {row.answerMax}</p>
        </div>

      </div>
      <div className={style.content}>
  <div className={style.halfColumn}>
 
    <AnswerListTable
      questionID={row.id}
      context={context}
    />

  </div>
</div>
      <div className={style.content}>
        <div className={style.smallColumn} style={{ display: rowSortOrderVisible }}>
          <p className={style.heading}>Row Sort Order</p>

          {row.rowSortOrder ? (
            <Tag className={style.labelTextLeft}>{row.rowSortOrder}</Tag>
          ) : null}
        </div>
        <div className={style.smallColumn} style={{ display: columnSortOrderVisible }}>
          <p className={style.heading}>Column Sort Order</p>
          {row.columnSortOrder ? (
            <Tag className={style.labelTextLeft}>{row.columnSortOrder}</Tag>
          ) : null}
        </div>
      </div>
      <div className={style.content} style={{ marginBottom: '30px' }}>
        <div className={style.halfColumn}>
          <p className={style.heading}>Scripter Notes</p>
          <p className={style.input}>{row.scripterNotes}</p>
        </div>
        <div className={style.smallColumn}>
          <p className={style.heading}>Question Format Details</p>
          <p className={style.input}>{row.questionFormatDetail}</p>
        </div>
      </div>
    </div>

  );
}
