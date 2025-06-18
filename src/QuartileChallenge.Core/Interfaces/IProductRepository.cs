using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByStoreIdAsync(Guid storeId);
    Task<IEnumerable<Product>> GetByCompanyIdAsync(Guid companyId);
}