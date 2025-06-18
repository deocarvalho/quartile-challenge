namespace QuartileChallenge.Tests;

public abstract class TestBase
{
    protected static readonly Guid ValidCompanyId = Guid.NewGuid();
    protected static readonly Guid ValidStoreId = Guid.NewGuid();

    protected static class TestData
    {
        public static class Store
        {
            public const string ValidName = "Test Store";
            public const string ValidLocation = "Test Location";
            public const string UpdatedName = "Updated Store";
            public const string UpdatedLocation = "Updated Location";
        }

        public static class Product
        {
            public const string ValidName = "Test Product";
            public const string ValidDescription = "Test Description";
            public const decimal ValidPrice = 99.99m;
            public const string UpdatedName = "Updated Product";
            public const string UpdatedDescription = "Updated Description";
            public const decimal UpdatedPrice = 149.99m;
        }
    }

    protected TestBase()
    {
        // Common test setup can go here
    }
}