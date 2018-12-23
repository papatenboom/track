USE [track]
GO

/****** Object:  Table [dbo].[User]    Script Date: 12/22/2018 11:46:39 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NULL,
	[Password] [varchar](32) NULL
) ON [PRIMARY]
GO

