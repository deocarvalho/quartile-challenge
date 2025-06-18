using QuartileChallenge.Core.Domain;

namespace QuartileChallenge.Core.Interfaces;

public interface IStoreRepository : IRepository<Store>
{
    Task<IEnumerable<Store>> GetByCompanyIdAsync(Guid companyId);
}