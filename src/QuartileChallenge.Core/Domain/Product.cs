namespace QuartileChallenge.Core.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public Guid StoreId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { } // For EF Core

    public Product(string name, string description, decimal price, Guid storeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null, empty, or whitespace.", nameof(name));
        
        if (price <= 0)
            throw new ArgumentException("Product price must be greater than zero.", nameof(price));
        
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        StoreId = storeId;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Update(string name, string description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}