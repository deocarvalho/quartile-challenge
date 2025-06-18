CREATE OR ALTER PROCEDURE dbo.sp_InsertProduct
    @Name nvarchar(200),
    @Description nvarchar(1000),
    @Price decimal(18,2),
    @StoreId uniqueidentifier,
    @Id uniqueidentifier OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Input validation
    IF @Name IS NULL OR LEN(TRIM(@Name)) = 0
        THROW 50000, 'Product name cannot be empty.', 1;
    
    IF @Price < 0
        THROW 50000, 'Price cannot be negative.', 1;
        
    IF NOT EXISTS (SELECT 1 FROM dbo.Stores WHERE Id = @StoreId AND IsActive = 1)
        THROW 50000, 'Store does not exist or is inactive.', 1;

    BEGIN TRY
        SET @Id = NEWID();
        
        INSERT INTO dbo.Products (
            Id,
            Name,
            Description,
            Price,
            StoreId,
            CreatedAt,
            IsActive
        )
        VALUES (
            @Id,
            @Name,
            @Description,
            @Price,
            @StoreId,
            GETUTCDATE(),
            1
        );
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO