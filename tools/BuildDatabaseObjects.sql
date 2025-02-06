CREATE DATABASE [SqlMakerDb]
GO

USE [SqlMakerDb]
GO

SET ANSI_NULLS, QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Agents](
	[AgentID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[FullName] [varchar](101) NULL,
	[EmailAddress] [varchar](50) NULL,
	[Address1] [varchar](50) NULL,
	[City] [varchar](50) NULL,
	[PropState] [char](2) NULL,
	[PostalCode] [varchar](10) NULL,
	[Latitude] [decimal](8,6) NULL,
	[Longitude] [decimal](9,6) NULL,
	[StreetGeo] [geography] NULL
	CONSTRAINT [PK_Agents_AgentID] PRIMARY KEY CLUSTERED ([AgentID] ASC) 
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE TABLE [dbo].[Clients](
	[ClientID] [smallint] IDENTITY(1,1) NOT NULL,
	[ClientName] [varchar](200) NOT NULL,
	[ClientContact] [varchar](80) NULL,
	[ClientPhone] [varchar](10) NULL,
	[ClientEmail] [varchar](80) NULL,
	[ClientNotes] [varchar](500) NULL,
 CONSTRAINT [PK_Clients_ClientID] PRIMARY KEY CLUSTERED 
(
	[ClientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PropertyStatusDesc](
	[StatusID] [smallint] NOT NULL,
	[StatusDesc] [varchar](50) NULL,
 CONSTRAINT [PK_StatusDescriptions_StatusID] PRIMARY KEY CLUSTERED 
(
	[StatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PropertyData](
	[PropertyID] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[LoanNumber] [varchar](35) NOT NULL,
	[Address1] [varchar](50) NOT NULL,
	[City] [varchar](50) NULL,
	[PostalCode] [varchar](10) NULL,
	[PropState] [char](2) NULL,
	[StatusID] [smallint] NOT NULL,
	[AppraisalValue] [money] NULL,
	[AppraisalDate] [date] NULL,
	[AgentID] [int] NULL,
	[ClientID] [int] NULL,
 CONSTRAINT [PK_PropertyData_PropertyID] PRIMARY KEY CLUSTERED 
(
	[PropertyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Grades](
	[EventID] [int] NOT NULL,
	[RVersion] [timestamp] NULL,
	[DelayReason] [smallint] NULL,
	[Grade] [varchar](7) NULL,
	[Comments] [varchar](1000) NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PropertyFlag](
	[EventID] [int] NOT NULL,
	[RVersion] [timestamp] NULL,
	[Comments] [varchar](1000) NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TaskAssignments](
	[EventID] [int] IDENTITY(500,1) NOT NULL,
	[PropertyID] [int] NOT NULL,
	[TaskNumber] [smallint] NOT NULL,
	[TaskStatus] [tinyint] NULL,
	[OpenDate] [datetime] NULL,
	[CloseDate] [datetime] NULL,
	[PauseDate] [date] NULL,
	[Comments] [varchar](1000) NULL,
 CONSTRAINT [PK_TaskAssignment_EventID] PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[TaskNames](
	[TaskNumber] [smallint] NOT NULL,
	[TaskName] [varchar](50) NULL,
	[AdHocTask] [tinyint] NULL
 CONSTRAINT [PK_TaskNames_TaskNumber] PRIMARY KEY NONCLUSTERED 
(
	[TaskNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[OutputLogs](
	[EventID] [int] NULL,
	[Comments] [varchar](1000) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PropertyData] ADD  CONSTRAINT [DF_PropertyData_CreatedDate]  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[PropertyData] ADD  CONSTRAINT [DF_PropertyData_StatusID]  DEFAULT ((10)) FOR [StatusID]
GO
ALTER TABLE [dbo].[TaskAssignments] ADD  CONSTRAINT [DF_TaskAssignment_TaskStatus]  DEFAULT ((1)) FOR [TaskStatus]
GO
ALTER TABLE [dbo].[TaskAssignments] ADD  CONSTRAINT [DF_TaskAssignment_OpenDate]  DEFAULT (getdate()) FOR [OpenDate]
GO

ALTER TABLE [dbo].[PropertyData]  WITH CHECK ADD  CONSTRAINT [FK_PropertyData_Agents_AgentID] FOREIGN KEY([AgentID])
REFERENCES [dbo].[Agents] ([AgentID])
GO
ALTER TABLE [dbo].[PropertyData] CHECK CONSTRAINT [FK_PropertyData_Agents_AgentID]
GO


ALTER TABLE [dbo].[Grades]  WITH CHECK ADD  CONSTRAINT [FKGrade_Grades_TaskAssingment_EventID] FOREIGN KEY([EventID])
REFERENCES [dbo].[TaskAssignments] ([EventID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Grades] CHECK CONSTRAINT [FKGrade_Grades_TaskAssingment_EventID]
GO
ALTER TABLE [dbo].[PropertyFlag]  WITH CHECK ADD  CONSTRAINT [FK_PropertyFlag_TaskAssignment_EventID] FOREIGN KEY([EventID])
REFERENCES [dbo].[TaskAssignments] ([EventID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PropertyFlag] CHECK CONSTRAINT [FK_PropertyFlag_TaskAssignment_EventID]
GO

ALTER TABLE [dbo].[TaskAssignments]  WITH CHECK ADD  CONSTRAINT [FK_TaskAssignment_PropertyData_PropertyID] FOREIGN KEY([PropertyID])
REFERENCES [dbo].[PropertyData] ([PropertyID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TaskAssignments] CHECK CONSTRAINT [FK_TaskAssignment_PropertyData_PropertyID]
GO
ALTER TABLE [dbo].[TaskAssignments]  WITH CHECK ADD  CONSTRAINT [FK_TaskAssignment_TaskNames_TaskNumber] FOREIGN KEY([TaskNumber])
REFERENCES [dbo].[TaskNames] ([TaskNumber])
GO
ALTER TABLE [dbo].[TaskAssignments] CHECK CONSTRAINT [FK_TaskAssignment_TaskNames_TaskNumber]
GO