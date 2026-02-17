
UPDATE ml 
SET ml.ktr_everinsnapshot = 1
FROM ktr_managedlist ml
    INNER JOIN ktr_QuestionnaireLineSharedList qlml ON qlml.ktr_managedlist = ml.ktr_managedlistid
    INNER JOIN ktr_StudyQuestionManagedListSnapshot snp ON snp.ktr_questionnairelinemanagedlist = qlml.ktr_questionnairelinesharedlistid 
WHERE ml.ktr_everinsnapshot IS NULL;




