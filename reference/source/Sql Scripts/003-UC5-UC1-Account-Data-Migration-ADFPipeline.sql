/****** Object:  Schema [ktr]    Script Date: 7/4/2025 9:11:00 AM ******/
CREATE SCHEMA [ktr]
GO
/****** Object:  Table [ktr].[AccountDELTA]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ktr].[AccountDELTA](
	[accountid] [uniqueidentifier] NULL,
	[accountnumber] [nvarchar](20) NULL,
	[name] [nvarchar](256) NULL,
	[description] [nvarchar](max) NULL,
	[address1_city] [nvarchar](255) NULL,
	[address1_country] [nvarchar](255) NULL,
	[address1_line1] [nvarchar](250) NULL,
	[address1_line2] [nvarchar](250) NULL,
	[address1_line3] [nvarchar](250) NULL,
	[address1_stateorprovince] [nvarchar](50) NULL,
	[address1_telephone2] [nvarchar](50) NULL,
	[address1_telephone3] [nvarchar](50) NULL,
	[address1_postalcode] [nvarchar](255) NULL,
	[emailaddress1] [nvarchar](255) NULL,
	[emailaddress2] [nvarchar](100) NULL,
	[ktr_companycode] [nvarchar](255) NULL,
	[ktr_companynumber] [nvarchar](100) NULL,
	[ktr_customernumber] [nvarchar](100) NULL,
	[parentaccountid] [uniqueidentifier] NULL,
	[statecode] [int] NULL,
	[statecodename] [nvarchar](255) NULL,
	[statuscode] [int] NULL,
	[statuscodename] [nvarchar](255) NULL,
	[telephone1] [nvarchar](255) NULL,
	[telephone2] [nvarchar](50) NULL,
	[telephone3] [nvarchar](50) NULL,
	[createdon] [datetime] NULL,
	[modifiedon] [datetime] NULL,
	[overriddencreatedon] [datetime] NULL,
	[Operation] [nvarchar](100) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [ktr].[AccountDEST]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ktr].[AccountDEST](
	[accountid] [uniqueidentifier] NULL,
	[accountnumber] [nvarchar](20) NULL,
	[name] [nvarchar](256) NULL,
	[description] [nvarchar](max) NULL,
	[address1_city] [nvarchar](255) NULL,
	[address1_country] [nvarchar](255) NULL,
	[address1_line1] [nvarchar](250) NULL,
	[address1_line2] [nvarchar](250) NULL,
	[address1_line3] [nvarchar](250) NULL,
	[address1_stateorprovince] [nvarchar](50) NULL,
	[address1_telephone2] [nvarchar](50) NULL,
	[address1_telephone3] [nvarchar](50) NULL,
	[address1_postalcode] [nvarchar](255) NULL,
	[emailaddress1] [nvarchar](255) NULL,
	[emailaddress2] [nvarchar](100) NULL,
	[ktr_companycode] [nvarchar](255) NULL,
	[ktr_companynumber] [nvarchar](100) NULL,
	[ktr_customernumber] [nvarchar](100) NULL,
	[parentaccountid] [uniqueidentifier] NULL,
	[statecode] [int] NULL,
	[statecodename] [nvarchar](255) NULL,
	[statuscode] [int] NULL,
	[statuscodename] [nvarchar](255) NULL,
	[telephone1] [nvarchar](255) NULL,
	[telephone2] [nvarchar](50) NULL,
	[telephone3] [nvarchar](50) NULL,
	[createdon] [datetime] NULL,
	[modifiedon] [datetime] NULL,
	[overriddencreatedon] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [ktr].[AccountSRC]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ktr].[AccountSRC](
	[accountid] [uniqueidentifier] NULL,
	[accountnumber] [nvarchar](20) NULL,
	[name] [nvarchar](256) NULL,
	[description] [nvarchar](max) NULL,
	[address1_city] [nvarchar](255) NULL,
	[address1_country] [nvarchar](255) NULL,
	[address1_line1] [nvarchar](250) NULL,
	[address1_line2] [nvarchar](250) NULL,
	[address1_line3] [nvarchar](250) NULL,
	[address1_stateorprovince] [nvarchar](50) NULL,
	[address1_telephone2] [nvarchar](50) NULL,
	[address1_telephone3] [nvarchar](50) NULL,
	[address1_postalcode] [nvarchar](255) NULL,
	[emailaddress1] [nvarchar](255) NULL,
	[emailaddress2] [nvarchar](100) NULL,
	[ktr_companycode] [nvarchar](255) NULL,
	[ktr_companynumber] [nvarchar](100) NULL,
	[ktr_customernumber] [nvarchar](100) NULL,
	[parentaccountid] [uniqueidentifier] NULL,
	[statecode] [int] NULL,
	[statecodename] [nvarchar](255) NULL,
	[statuscode] [int] NULL,
	[statuscodename] [nvarchar](255) NULL,
	[telephone1] [nvarchar](255) NULL,
	[telephone2] [nvarchar](50) NULL,
	[telephone3] [nvarchar](50) NULL,
	[createdon] [datetime] NULL,
	[modifiedon] [datetime] NULL,
	[overriddencreatedon] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [ktr].[ADFJobLog]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ktr].[ADFJobLog](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[PipelineName] [nvarchar](255) NULL,
	[RunId] [nvarchar](100) NULL,
	[ActivityName] [nvarchar](255) NULL,
	[Status] [nvarchar](50) NULL,
	[StartTime] [datetime] NULL,
	[EndTime] [datetime] NULL,
	[Duration] [int] NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK__ADFJobLo__5E5499A806069016] PRIMARY KEY CLUSTERED 
(
	[LogID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [ktr].[ADFJobLog] ADD  CONSTRAINT [DF__ADFJobLog__Times__09A971A2]  DEFAULT (getdate()) FOR [Timestamp]
GO
/****** Object:  StoredProcedure [ktr].[sp_ComputeDELTA]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [ktr].[sp_ComputeDELTA]
    
AS
BEGIN
-- CREATE
  INSERT INTO [ktr].AccountDELTA
      ([accountid]
      ,[accountnumber]
      ,[address1_city]
      ,[address1_country]
      ,[address1_line1]
      ,[address1_line2]
      ,[address1_line3]
      ,[address1_stateorprovince]
      ,[address1_telephone2]
      ,[address1_telephone3]
      ,[description]
      ,[emailaddress1]
      ,[emailaddress2]
      ,[ktr_companycode]
      ,[address1_postalcode]
      ,[ktr_companynumber]
      ,[ktr_customernumber]
      ,[name]
      ,[parentaccountid]
      ,[statecode]
      ,[statecodename]
      ,[statuscode]
      ,[statuscodename]
      ,[telephone1]
      ,[telephone2]
      ,[telephone3]
	  ,[createdon]
	  ,[modifiedon]
	  ,[overriddencreatedon]
      ,[Operation])

SELECT		S.[accountid]
      ,S.[accountnumber]
      ,S.[address1_city]
      ,S.[address1_country]
      ,S.[address1_line1]
      ,S.[address1_line2]
      ,S.[address1_line3]
      ,S.[address1_stateorprovince]
      ,S.[address1_telephone2]
      ,S.[address1_telephone3]
      ,S.[description]
      ,S.[emailaddress1]
      ,S.[emailaddress2]
      ,S.[ktr_companycode]
      ,S.[address1_postalcode] 
      ,S.[ktr_companynumber]
      ,S.[ktr_customernumber]
      ,S.[name]
      ,S.[parentaccountid]
      ,S.[statecode]
      ,S.[statecodename]
      ,S.[statuscode]
      ,S.[statuscodename]
      ,S.[telephone1]
      ,S.[telephone2]
      ,S.[telephone3]
	  ,S.[createdon]
	  ,S.[modifiedon]
	  ,S.[overriddencreatedon]
      ,'CREATE' AS 'Operation'
 		   
FROM [ktr].[AccountSRC] S 
LEFT OUTER JOIN [ktr].[AccountDEST] D
	ON S.accountid = D.accountid
	WHERE D.accountid IS NULL 

-- DELETE 
INSERT INTO [ktr].AccountDELTA
      ([accountid]
      ,[accountnumber]
      ,[address1_city]
      ,[address1_country]
      ,[address1_line1]
      ,[address1_line2]
      ,[address1_line3]
      ,[address1_stateorprovince]
      ,[address1_telephone2]
      ,[address1_telephone3]
      ,[description]
      ,[emailaddress1]
      ,[emailaddress2]
      ,[ktr_companycode]
      ,[address1_postalcode] 
      ,[ktr_companynumber]
      ,[ktr_customernumber]
      ,[name]
      ,[parentaccountid]
      ,[statecode]
      ,[statecodename]
      ,[statuscode]
      ,[statuscodename]
      ,[telephone1]
      ,[telephone2]
      ,[telephone3]
	  ,[createdon]
	  ,[modifiedon]
	  ,[overriddencreatedon]
      ,[Operation])

SELECT D.[accountid]
      ,D.[accountnumber]
      ,D.[address1_city]
      ,D.[address1_country]
      ,D.[address1_line1]
      ,D.[address1_line2]
      ,D.[address1_line3]
      ,D.[address1_stateorprovince]
      ,D.[address1_telephone2]
      ,D.[address1_telephone3]
      ,D.[description]
      ,D.[emailaddress1]
      ,D.[emailaddress2]
      ,D.[ktr_companycode]
      ,D.[address1_postalcode] 
      ,D.[ktr_companynumber]
      ,D.[ktr_customernumber]
      ,D.[name]
      ,D.[parentaccountid]
      , 1 as statecode
      ,D.[statecodename]
      , 2 as statuscode
      ,D.[statuscodename]
      ,D.[telephone1]
      ,D.[telephone2]
      ,D.[telephone3]
	  ,D.[createdon]
	  ,D.[modifiedon]
	  ,D.[overriddencreatedon]
      ,'DELETE' AS 'Operation'
FROM [ktr].[AccountDEST] D 
LEFT OUTER JOIN [ktr].[AccountSRC] S
	ON S.accountid = D.accountid
	WHERE D.statecode != 1
    AND D.statuscode != 2
    AND S.accountid IS NULL 


--UPDATE 
INSERT INTO [ktr].AccountDELTA
      ([accountid]
      ,[accountnumber]
      ,[address1_city]
      ,[address1_country]
      ,[address1_line1]
      ,[address1_line2]
      ,[address1_line3]
      ,[address1_stateorprovince]
      ,[address1_telephone2]
      ,[address1_telephone3]
      ,[description]
      ,[emailaddress1]
      ,[emailaddress2]
      ,[ktr_companycode]
      ,[address1_postalcode] 
      ,[ktr_companynumber]
      ,[ktr_customernumber]
      ,[name]
      ,[parentaccountid]
      ,[statecode]
      ,[statecodename]
      ,[statuscode]
      ,[statuscodename]
      ,[telephone1]
      ,[telephone2]
      ,[telephone3]
	  ,[createdon]
	  ,[modifiedon]
	  ,[overriddencreatedon]
      ,[Operation])

SELECT		S.[accountid]
      ,S.[accountnumber]
      ,S.[address1_city]
      ,S.[address1_country]
      ,S.[address1_line1]
      ,S.[address1_line2]
      ,S.[address1_line3]
      ,S.[address1_stateorprovince]
      ,S.[address1_telephone2]
      ,S.[address1_telephone3]
      ,S.[description]
      ,S.[emailaddress1]
      ,S.[emailaddress2]
      ,S.[ktr_companycode]
      ,S.[address1_postalcode] 
      ,S.[ktr_companynumber]
      ,S.[ktr_customernumber]
      ,S.[name]
      ,S.[parentaccountid]
      ,S.[statecode]
      ,S.[statecodename]
      ,S.[statuscode]
      ,S.[statuscodename]
      ,S.[telephone1]
      ,S.[telephone2]
      ,S.[telephone3]
	  ,S.[createdon]
	  ,S.[modifiedon]
	  ,S.[overriddencreatedon]
      ,'UPDATE' AS 'Operation'
FROM [ktr].[AccountSRC] S
INNER JOIN [ktr].[AccountDEST] D
	ON S.accountid = D.accountid
WHERE 
 ISNULL(S.[accountnumber], '')                != ISNULL(D.[accountnumber], '')
OR ISNULL(S.[address1_city], '')				    != ISNULL(D.[address1_city], '')
OR ISNULL(S.[address1_country], '')				!= ISNULL(D.[address1_country], '')
OR ISNULL(S.[address1_line1], '')				!= ISNULL(D.[address1_line1], '')
OR ISNULL(S.[address1_line2], '')				!= ISNULL(D.[address1_line2], '')
OR ISNULL(S.[address1_line3], '')				!= ISNULL(D.[address1_line3], '')
OR ISNULL(S.[address1_stateorprovince], '')		!= ISNULL(D.[address1_stateorprovince], '')
OR ISNULL(S.[address1_telephone2], '')			!= ISNULL(D.[address1_telephone2], '')
OR ISNULL(S.[address1_telephone3], '')			!= ISNULL(D.[address1_telephone3], '')
OR ISNULL(S.[description], '')					!= ISNULL(D.[description], '')
OR ISNULL(S.[emailaddress1], '')					!= ISNULL(D.[emailaddress1], '')
OR ISNULL(S.[emailaddress2], '')				!= ISNULL(D.[emailaddress2], '')
OR ISNULL(S.[address1_postalcode], '')				!= ISNULL(D.[address1_postalcode], '')
OR ISNULL(S.[ktr_companycode], '')				!= ISNULL(D.[ktr_companycode], '')
OR ISNULL(S.[ktr_companynumber], '')				!= ISNULL(D.[ktr_companynumber], '')
OR ISNULL(S.[ktr_customernumber], '')			!= ISNULL(D.[ktr_customernumber], '')
OR ISNULL(S.[name], '')							!= ISNULL(D.[name], '')
--OR ISNULL(S.[parentaccountid], '00000000-0000-0000-0000-000000000000')				!= ISNULL(D.[parentaccountid], '00000000-0000-0000-0000-000000000000')
OR ISNULL(S.[statecode], '')						!= ISNULL(D.[statecode], '')
OR ISNULL(S.[statuscode], '')					!= ISNULL(D.[statuscode], '')
OR ISNULL(S.[telephone1], '')					!= ISNULL(D.[telephone1], '')
OR ISNULL(S.[telephone2], '')					!= ISNULL(D.[telephone2], '')
OR ISNULL(S.[telephone3], '')					!= ISNULL(D.[telephone3], '')
-- UPDATE custom statecode from UC5
--statuscode	statuscodename
--818440002	Dynamics Approved
--2	        Inactive
--818440004	Maconomy Client
--818440003	Maconomy Prospect
--1	        New

UPDATE ktr.accountdelta
SET statuscode = 1
where statuscode IN (818440002,818440004,818440003) 
END
GO
/****** Object:  StoredProcedure [ktr].[sp_GetDELTA]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [ktr].[sp_GetDELTA]
    
AS
BEGIN

--  SELECT * FROM [ktr].AccountDELTA
SELECT 
       [accountid]
      ,[accountnumber]
      ,[address1_city]
      ,[address1_country]
      ,[address1_line1]
      ,[address1_line2]
      ,[address1_line3]
      ,[address1_stateorprovince]
      ,[address1_telephone2]
      ,[address1_telephone3]
      ,[description]
      ,[emailaddress1]
      ,[emailaddress2]
      ,[ktr_companycode]
      ,[address1_postalcode]
      ,[ktr_companynumber]
      ,[ktr_customernumber]
      ,[name]
      ,[parentaccountid]
      ,[statecode]
      ,[statecodename]
      ,[statuscode]
      ,[statuscodename]
      ,[telephone1]
      ,[telephone2]
      ,[telephone3]
	  ,[createdon]
	  ,[modifiedon]
	  ,[createdon] as [overriddencreatedon]
      ,[Operation]

  FROM [ktr].[AccountDELTA]
 END 
GO
/****** Object:  StoredProcedure [ktr].[sp_GetLastRunOK]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


 CREATE PROCEDURE [ktr].[sp_GetLastRunOK]
    
AS
BEGIN
  DECLARE @LastRunOK as datetime

	select @LastRunOK = MAX(A.StartTime) from [ktr].[ADFJobLog] A
	INNER JOIN [ktr].[ADFJobLog] B
		ON A.PipelineName = B.PipelineName
		AND A.RunId = B.RunId
	where A.PipelineName = 'UC5-UC1 Account Synch'
	AND A.ActivityName = 'JOB START'
	AND B.ActivityName = 'JOB END'


	--SELECT ISNULL(@LastRunOK,'2000-01-01') as result
	--DELETED THE DELTA EXECUTE
	--IT DOEN'T works on the update
    SELECT '2000-01-01' as result    
END
GO
/****** Object:  StoredProcedure [ktr].[sp_GetParentDELTA]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [ktr].[sp_GetParentDELTA]
    
AS
BEGIN
 -- 	SELECT accountid, parentaccountid 
	--FROM [ktr].AccountDELTA
 --   WHERE Parentaccountid IS NOT NULL    
 --SELECT D.accountid, P.accountid as parentaccountid 
	--FROM [ktr].AccountDELTA D
	--INNER JOIN [dbo].[Client] C
	--	ON  D.[ktr_customernumber] = C.CUSTOMERNUMBER 
	--INNER JOIN [ktr].AccountDELTA P
	--	ON C.PARENTCUSTOMER = P.[ktr_customernumber] 
	(SELECT S.accountid , S.parentaccountid from ktr.AccountSRC S WHERE S.parentaccountid IS NOT NULL
	EXCEPT 
	SELECT D.accountid , D.parentaccountid from ktr.AccountDEST D WHERE D.parentaccountid IS NOT NULL
	)
	-- UNION 
	--(
	--SELECT S.accountid, P.accountid as parentaccountid 
	--	FROM [ktr].AccountSRC S
	--	INNER JOIN [dbo].[Client] C
	--		ON  S.[ktr_customernumber] = C.CUSTOMERNUMBER 
	--	INNER JOIN [ktr].AccountSRC P
	--   ON C.PARENTCUSTOMER = P.[ktr_customernumber] 
	--EXCEPT 
	--SELECT D.accountid , D.parentaccountid from ktr.AccountDEST D WHERE D.parentaccountid IS NOT NULL
	--)

END

GO
/****** Object:  StoredProcedure [ktr].[sp_LogADFStep]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [ktr].[sp_LogADFStep]
    @PipelineName NVARCHAR(255),
    @ActivityName NVARCHAR(255) = NULL,
    @Status NVARCHAR(50) = NULL,
    @StartTime DATETIME = NULL,
    @EndTime DATETIME = NULL,
    @Duration INT = NULL,
    @ErrorMessage NVARCHAR(MAX) = NULL ,
	@RunId nvarchar(255) = NULL  
AS
BEGIN
    INSERT INTO ADFJobLog (PipelineName, ActivityName, Status, StartTime, EndTime, Duration, ErrorMessage, RunId)
    VALUES (@PipelineName, @ActivityName, @Status, @StartTime, @EndTime, @Duration, @ErrorMessage, @RunId)
END

GO
/****** Object:  StoredProcedure [ktr].[sp_TruncateTBL]    Script Date: 7/4/2025 9:11:00 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [ktr].[sp_TruncateTBL]
    
AS
BEGIN
    TRUNCATE TABLE [ktr].[AccountDELTA]
    TRUNCATE TABLE [ktr].[AccountDEST] 
	TRUNCATE TABLE [ktr].[AccountSRC] 

	DELETE [ktr].[ADFJobLog] where [Timestamp] < DATEADD(DAY, -30, GETDATE())
END






  
  
  
  




GO
