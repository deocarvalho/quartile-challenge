namespace QuartileChallenge.Core.Domain;

public class Store
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }
    public string Location { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ModifiedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Store() { } // For EF Core

    public Store(Guid companyId, string name, string location)
    {
        Id = Guid.NewGuid();
        CompanyId = companyId;
        Name = name;
        Location = location;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void Update(string name, string location)
    {
        Name = name;
        Location = location;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}