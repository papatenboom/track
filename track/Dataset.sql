USE [track]
GO

/****** Object:  Table [dbo].[Dataset]    Script Date: 12/22/2018 11:46:25 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Dataset](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[Label] [varchar](50) NULL,
	[Active] [bit] NULL
) ON [PRIMARY]
GO

