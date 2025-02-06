USE [SqlMakerDb]
GO

SET NOCOUNT ON;

SET IDENTITY_INSERT [dbo].[Clients] ON 
GO

INSERT [dbo].[Clients] ([ClientID], [ClientName], [ClientContact], [ClientPhone], [ClientEmail], [ClientNotes]) 
VALUES 
	(1, N'Client 1', NULL, NULL, NULL, NULL),
	(2, N'Street Lending', NULL, NULL, NULL, NULL),
	(3, N'Unassigned', N'Unassigned', NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Clients] OFF
GO

SET IDENTITY_INSERT [dbo].[Agents] ON 
GO

INSERT [dbo].[Agents] ([AgentID], [FirstName], [LastName], [FullName], [EmailAddress], [Address1], [City], [PropState], [PostalCode]) 
VALUES 
	(307, N'PAULA', N'Smith', N'PAULA Smith', N'example@example.com', N'4110 Old Redmond Rd', N'Redmond', N'WA', N'98052'),
	(351, N'James', N'Smith', N'James Smith', N'example@example.com', N'4110 Old Redmond Rd', N'Redmond', N'MA', N'98052'),
	(986, N'Brian', N'Smith', N'Brian Smith', N'example@example.com', N'4110 Old Redmond Rd', N'Redmond ', N'WA', N'98052'),
	(1024, N'Marguerite', N'Smith', N'Marguerite Smith', N'example@example.com', N'4110 Old Redmond Rd ', N'Redmond', N'WA', N'98052')
GO

SET IDENTITY_INSERT [dbo].[Agents] OFF
GO

INSERT [dbo].[PropertyStatusDesc] ([StatusID], [StatusDesc]) 
VALUES 
	(10, N'Acquired'),
	(11, N'Closed/Sold'),
	(12, N'Eviction'),
	(13, N'Hold'),
	(14, N'Listed'),
	(15, N'Listed Occupied')
GO

SET IDENTITY_INSERT [dbo].[PropertyData] ON 
GO

INSERT [dbo].[PropertyData] ([PropertyID], [CreatedDate], [LoanNumber], [Address1], [City], [PostalCode], [PropState], [StatusID], [AppraisalValue], [AppraisalDate], [AgentID], [ClientID]) 
VALUES 
	(1, GETDATE(), N'89fbd2', N'3410 Blonde St.', 'Palo Alto', '94301', 'CA', 10, 1024, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0), 307, 1),
	(2, GETDATE(), N'999900ab', N'5720 McAuley St..', 'Oakland', '94609', 'CA', 11, 42000.0000, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0), 1024, 1),
	(3, GETDATE(), N'88fkf82', N'5420 College Av.', 'Oakland', '94609', 'CA', 12, 420.0000, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 2, 0), 351, 1),
	(4, GETDATE(), N'1893983', N'301 Putnam', 'Vacaville', '95688', 'CA', 13, 55899.0000, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 2, 0), 307, 2),
	(5, GETDATE(), N'AFF19191', N'3 Balding Pl.', 'Gary', '46403', 'IN', 10, 100000.0000, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 3, 0), 351, 2),
	(6, GETDATE(), N'4940303', N'55 Hillsdale Bl.', 'Corvallis', '97330', 'OR', 10, 120000.0000, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) - 1, 0), 307, NULL)

SET IDENTITY_INSERT [dbo].[PropertyData] OFF
GO

INSERT [dbo].[TaskNames] ([TaskNumber], [TaskName], [AdHocTask]) 
VALUES 
	(1, N'Property flag', 1),
	(2, N'Initial grade', NULL)
GO

SET IDENTITY_INSERT [dbo].[TaskAssignments] ON 
GO

INSERT [dbo].[TaskAssignments] ([EventID], [PropertyID], [TaskNumber], [TaskStatus], [OpenDate], [CloseDate]) 
VALUES 
	(1, 1, 1, 1, CAST(N'2019-05-15T10:09:00.533' AS DateTime), NULL),
	(2, 1, 1, 1, CAST(N'2019-05-15T10:09:20.443' AS DateTime), NULL),
	(3, 5, 1, 1, CAST(N'2019-05-15T10:09:20.450' AS DateTime), NULL),
	(4, 3, 1, 1, CAST(N'2019-05-15T10:15:46.427' AS DateTime), NULL),
	(5, 2, 2, 1, CAST(N'2019-05-15T10:15:53.360' AS DateTime), NULL),
	(6, 2, 2, 1, CAST(N'2019-05-15T10:15:53.360' AS DateTime), NULL),
	(7, 6, 2, 1, CAST(N'2019-05-15T10:19:11.557' AS DateTime), NULL)
GO
SET IDENTITY_INSERT [dbo].[TaskAssignments] OFF
GO

INSERT [dbo].[PropertyFlag] ([EventID], [Comments]) 
VALUES 
	(1, N'REC''V REFERRAL CK# 017624 - COE 5/8/19'),
	(2, NULL),
	(3, NULL),
	(4, N'REFERRAL CK RECV 9/26/19 - $925.00 - Per WIll Smith')
GO

INSERT [dbo].[Grades] ([EventID], [DelayReason], [Grade], [Comments]) 
VALUES 
	(5, NULL, NULL, NULL),
	(6, NULL, NULL, NULL),
	(7, NULL, NULL, NULL)
GO