-- Create Stores table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Stores](
        [Id] [uniqueidentifier] NOT NULL,
        [CompanyId] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Location] [nvarchar](500) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [ModifiedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_Stores] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Create Products table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Products](
        [Id] [uniqueidentifier] NOT NULL,
        [Name] [nvarchar](200) NOT NULL,
        [Description] [nvarchar](1000) NULL,
        [Price] [decimal](18, 2) NOT NULL,
        [StoreId] [uniqueidentifier] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [ModifiedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Products_Stores] FOREIGN KEY ([StoreId]) 
            REFERENCES [dbo].[Stores] ([Id])
            ON DELETE NO ACTION
    )
END
GO