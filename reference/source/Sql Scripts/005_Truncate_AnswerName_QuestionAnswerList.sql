UPDATE ktr_questionanswerlist
SET ktr_name = LEFT(ktr_name, 100)
WHERE LEN(ktr_name) > 100;