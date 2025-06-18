using Microsoft.EntityFrameworkCore;
using QuartileChallenge.Core.Domain;
using QuartileChallenge.Core.Interfaces;
using QuartileChallenge.Infrastructure.Data;

namespace QuartileChallenge.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetByStoreIdAsync(Guid storeId)
    {
        return await _context.Products
            .Where(p => p.StoreId == storeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.Products
            .Join(_context.Stores,
                product => product.StoreId,
                store => store.Id,
                (product, store) => new { Product = product, Store = store })
            .Where(x => x.Store.CompanyId == companyId)
            .Select(x => x.Product)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .ToListAsync();
    }
}