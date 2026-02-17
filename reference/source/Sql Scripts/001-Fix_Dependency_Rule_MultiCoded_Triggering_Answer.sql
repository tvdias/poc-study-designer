DECLARE @Rule_MultiCoded int = 847610001;

UPDATE dr
SET dr.ktr_triggeringanswer = NULL
FROM ktr_dependencyrule dr 
   INNER JOIN ktr_configurationquestion cq ON cq.ktr_configurationquestionid = dr.ktr_configurationquestion
WHERE cq.ktr_rule = @Rule_MultiCoded AND dr.ktr_triggeringanswer IS NOT NULL;

