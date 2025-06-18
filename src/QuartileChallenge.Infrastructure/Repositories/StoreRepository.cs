using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.Infrastructure.Data;

namespace QuartileChallenge.Infrastructure.Repositories;

public class StoreRepository : BaseRepository<Store>, IStoreRepository
{
    public StoreRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Store>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Stores
            .Where(s => s.CompanyId == companyId)
            .ToListAsync();
    }

    public override async Task<Store?> GetByIdAsync(Guid id)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public override async Task<IEnumerable<Store>> GetAllAsync()
    {
        return await _context.Stores
            .ToListAsync();
    }
}