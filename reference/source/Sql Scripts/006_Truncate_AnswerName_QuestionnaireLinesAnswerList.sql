UPDATE ktr_questionnairelinesanswerlist
SET ktr_name = LEFT(ktr_name, 100)
WHERE LEN(ktr_name) > 100;