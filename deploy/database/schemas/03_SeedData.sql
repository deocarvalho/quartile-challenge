-- =================================================================
-- Description: Seeds the database with initial sample data.
-- =================================================================

-- Exit if the script has already been run to avoid duplicate data
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products' AND (SELECT COUNT(*) FROM Products) > 0)
BEGIN
    PRINT 'Database has already been seeded. Skipping seed script.';
    RETURN;
END
GO

PRINT 'Seeding initial data into Stores and Products tables...';

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Seed Stores Table
    -- =================================================================
    PRINT 'Inserting sample stores...';
    
    -- Declare GUIDs for Company IDs to be used
    DECLARE @company1Id uniqueidentifier = '5F4CE287-3591-4234-A248-23269E5D9077';
    DECLARE @company2Id uniqueidentifier = 'ABF3AB80-5C27-465B-979C-2A233D41461A';

    -- Declare GUIDs for Store IDs to be created
    DECLARE @store1Id uniqueidentifier = NEWID();
    DECLARE @store2Id uniqueidentifier = NEWID();
    DECLARE @store3Id uniqueidentifier = NEWID();

    INSERT INTO Stores (Id, CompanyId, Name, Location, CreatedAt, IsActive)
    VALUES
        (@store1Id, @company1Id, 'Main Street Corner Market', '123 Main St, Anytown, USA', GETUTCDATE(), 1),
        (@store2Id, @company1Id, 'Downtown Gadget Hub', '456 Tech Ave, Anytown, USA', GETUTCDATE(), 1),
        (@store3Id, @company2Id, 'Suburban Superstore', '789 Retail Rd, Suburbia, USA', GETUTCDATE(), 1);
    
    PRINT 'Stores inserted successfully.';

    -- 2. Seed Products Table
    -- =================================================================
    PRINT 'Inserting sample products...';
    
    -- Insert products for 'Main Street Corner Market' (Store 1)
    INSERT INTO Products (Id, Name, Description, Price, StoreId, CreatedAt, IsActive)
    VALUES
        (NEWID(), 'Organic Bananas', 'A bunch of fresh, organic bananas.', 1.99, @store1Id, GETUTCDATE(), 1),
        (NEWID(), 'Artisanal Bread', 'A loaf of freshly baked sourdough bread.', 5.49, @store1Id, GETUTCDATE(), 1),
        (NEWID(), 'Free-Range Eggs', 'One dozen large brown free-range eggs.', 4.25, @store1Id, GETUTCDATE(), 1),
        (NEWID(), 'Almond Milk', 'Unsweetened vanilla almond milk, 1/2 gallon.', 3.79, @store1Id, GETUTCDATE(), 1);

    -- Insert products for 'Downtown Gadget Hub' (Store 2)
    INSERT INTO Products (Id, Name, Description, Price, StoreId, CreatedAt, IsActive)
    VALUES
        (NEWID(), 'Wireless Mouse', 'Ergonomic wireless mouse with 2.4 GHz connection.', 29.99, @store2Id, GETUTCDATE(), 1),
        (NEWID(), 'Mechanical Keyboard', 'RGB Backlit Mechanical Gaming Keyboard.', 89.95, @store2Id, GETUTCDATE(), 1),
        (NEWID(), '4K Webcam', 'USB webcam with 4K resolution and built-in microphone.', 125.00, @store2Id, GETUTCDATE(), 1),
        (NEWID(), 'USB-C Hub', '7-in-1 USB-C Hub with HDMI, SD card reader, and USB 3.0 ports.', 45.50, @store2Id, GETUTCDATE(), 1);

    -- Insert products for 'Suburban Superstore' (Store 3)
    INSERT INTO Products (Id, Name, Description, Price, StoreId, CreatedAt, IsActive)
    VALUES
        (NEWID(), 'Garden Hose', '50-foot heavy-duty garden hose.', 35.99, @store3Id, GETUTCDATE(), 1),
        (NEWID(), 'Lawn Fertilizer', 'All-season lawn fertilizer, covers 5,000 sq ft.', 22.50, @store3Id, GETUTCDATE(), 1),
        (NEWID(), 'LED Lightbulbs (4-pack)', '4-pack of energy-efficient A19 LED lightbulbs.', 12.99, @store3Id, GETUTCDATE(), 1),
        (NEWID(), 'Dish Soap', 'Lemon-scented dish soap, 28 oz bottle.', 4.99, @store3Id, GETUTCDATE(), 1);

    PRINT 'Products inserted successfully.';

    COMMIT TRANSACTION;
    PRINT 'Data seeding completed successfully.';

END TRY
BEGIN CATCH
    -- If any error occurs, roll back the entire transaction
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    -- Re-throw the error to be caught by the client
    PRINT 'An error occurred during data seeding. Transaction rolled back.';
    THROW;
END CATCH
GO 