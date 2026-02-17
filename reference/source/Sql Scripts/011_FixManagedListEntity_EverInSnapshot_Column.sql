UPDATE mle 
SET mle.ktr_everinsnapshot = 1
FROM ktr_managedlistentity mle
    INNER JOIN ktr_managedlist ml ON mle.ktr_managedlist = ml.ktr_managedlistid
    INNER JOIN ktr_QuestionnaireLineSharedList qlml ON qlml.ktr_managedlist = ml.ktr_managedlistid
    INNER JOIN ktr_StudyQuestionManagedListSnapshot snp ON snp.ktr_questionnairelinemanagedlist = qlml.ktr_questionnairelinesharedlistid 
WHERE mle.ktr_everinsnapshot IS NULL;