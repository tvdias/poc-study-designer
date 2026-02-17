Update ktr_questionnairelinesanswerlist
SET ktr_answercode = ktr_name
Where ktr_name IS NOT NULL and ktr_answercode <> ktr_name;