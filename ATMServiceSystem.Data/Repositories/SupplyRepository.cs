// ATMServiceSystem.Data/Repositories/SupplyRepository.cs
using Microsoft.EntityFrameworkCore;

public class SupplyRepository : IRepository<Supply>
{
    private readonly AppDbContext _context;

    public SupplyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Supply>> GetAllAsync()
    {
        return await _context.Supplies
            .Include(s => s.ATM)
            .ToListAsync();
    }

    public async Task<Supply> GetByIdAsync(int id)
    {
        return await _context.Supplies
            .Include(s => s.ATM)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Supply entity)
    {
        await _context.Supplies.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supply entity)
    {
        _context.Supplies.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var supply = await GetByIdAsync(id);
        if (supply != null)
        {
            _context.Supplies.Remove(supply);
            await _context.SaveChangesAsync();
        }
    }

    // Специфичные методы для Supplies
    public async Task<IEnumerable<Supply>> GetByATMIdAsync(int atmId)
    {
        return await _context.Supplies
            .Where(s => s.ATMId == atmId)
            .ToListAsync();
    }

    public async Task ReplenishSupply(int supplyId, int quantity)
    {
        var supply = await GetByIdAsync(supplyId);
        if (supply != null)
        {
            supply.Quantity += quantity;
            supply.LastReplenishmentDate = DateTime.Now;
            await UpdateAsync(supply);
        }
    }
}