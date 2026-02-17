UPDATE ml 
SET ml.ktr_SourceType = 100000001 -- Custom
FROM ktr_managedlist ml 
WHERE ml.ktr_sourcetype IS NULL;