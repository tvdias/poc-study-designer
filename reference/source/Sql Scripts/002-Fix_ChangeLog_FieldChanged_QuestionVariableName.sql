
 --CHECK AFFECTED ROWS:
SELECT distinct 
    ch.ktr_formerstudy,
    ch.ktr_currentstudy,
    snp.ktr_questionnaireline,
    ch.ktr_currentstudyquestionnairesnapshotline as CURRENTSNP,
    ch.ktr_formerstudyquestionnairesnapshotline as OLDVALUE,
    snpformer.ktr_studyquestionnairelinesnapshotid as NEWVALUE
FROM ktr_StudySnapshotLineChangeLog ch
    INNER JOIN ktr_studyquestionnairelinesnapshot snp ON snp.ktr_study = ch.ktr_currentstudy 
        AND snp.statuscode = 1
        AND snp.ktr_studyquestionnairelinesnapshotid = ch.ktr_currentstudyquestionnairesnapshotline
    INNER JOIN ktr_studyquestionnairelinesnapshot snpformer ON snpformer.ktr_study = ch.ktr_formerstudy
        AND snpformer.ktr_questionnaireline = snp.ktr_questionnaireline
        AND snpformer.statuscode = 1
WHERE 
    ch.statuscode = 1
    and ch.ktr_formerstudyquestionnairesnapshotline = ch.ktr_currentstudyquestionnairesnapshotline;

 --UPDATE ROWS:
UPDATE ch
SET ch.ktr_formerstudyquestionnairesnapshotline = snpformer.ktr_studyquestionnairelinesnapshotid
FROM ktr_StudySnapshotLineChangeLog ch
    INNER JOIN ktr_studyquestionnairelinesnapshot snp ON snp.ktr_study = ch.ktr_currentstudy 
        AND snp.statuscode = 1
        AND snp.ktr_studyquestionnairelinesnapshotid = ch.ktr_currentstudyquestionnairesnapshotline
    INNER JOIN ktr_studyquestionnairelinesnapshot snpformer ON snpformer.ktr_study = ch.ktr_formerstudy
        AND snpformer.ktr_questionnaireline = snp.ktr_questionnaireline
        AND snpformer.statuscode = 1
WHERE 
    ch.statuscode = 1
    and ch.ktr_formerstudyquestionnairesnapshotline = ch.ktr_currentstudyquestionnairesnapshotline;