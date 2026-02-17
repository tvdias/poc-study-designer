UPDATE kt_questionbank
SET ktr_answerlist = ''
WHERE ktr_answerlist = 'NA';

UPDATE kt_questionnairelines
SET ktr_answerlist = ''
WHERE ktr_answerlist = 'NA';