CREATE OR ALTER FUNCTION dbo.fn_GetProductsAsJson
(
    @CompanyId uniqueidentifier = NULL,
    @StoreId uniqueidentifier = NULL
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    RETURN (
        SELECT 
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StoreId,
            p.CreatedAt,
            p.ModifiedAt,
            p.IsActive,
            s.Name AS StoreName,
            s.CompanyId
        FROM dbo.Products p
        INNER JOIN dbo.Stores s ON p.StoreId = s.Id
        WHERE 
            (@CompanyId IS NULL OR s.CompanyId = @CompanyId)
            AND (@StoreId IS NULL OR p.StoreId = @StoreId)
            AND p.IsActive = 1
            AND s.IsActive = 1
        FOR JSON PATH
    )
END
GO

-- Example usage:
-- Get all products: SELECT dbo.fn_GetProductsAsJson(NULL, NULL)
-- Get company products: SELECT dbo.fn_GetProductsAsJson('company-guid', NULL)
-- Get store products: SELECT dbo.fn_GetProductsAsJson(NULL, 'store-guid')