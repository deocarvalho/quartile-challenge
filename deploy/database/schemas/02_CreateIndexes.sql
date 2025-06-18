-- Indexes for Stores table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stores_CompanyId' AND object_id = OBJECT_ID('dbo.Stores'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Stores_CompanyId] ON [dbo].[Stores]
    (
        [CompanyId] ASC
    )
    INCLUDE ([Name], [Location], [IsActive])
END
GO

-- Indexes for Products table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_StoreId' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_StoreId] ON [dbo].[Products]
    (
        [StoreId] ASC
    )
    INCLUDE ([Name], [Price], [IsActive])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Name' AND object_id = OBJECT_ID('dbo.Products'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_Name] ON [dbo].[Products]
    (
        [Name] ASC
    )
    WHERE [IsActive] = 1
END
GO