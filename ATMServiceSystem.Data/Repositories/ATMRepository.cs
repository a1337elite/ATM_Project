// ATMRepository.cs
using Microsoft.EntityFrameworkCore;

public class ATMRepository : IRepository<ATM>
{
    private readonly AppDbContext _context;

    public ATMRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ATM>> GetAllAsync()
    {
        return await _context.ATMs.ToListAsync();
    }

    public async Task<ATM> GetByIdAsync(int id)
    {
        return await _context.ATMs.FindAsync(id);
    }

    public async Task AddAsync(ATM entity)
    {
        await _context.ATMs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ATM entity)
    {
        _context.ATMs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var atm = await GetByIdAsync(id);
        if (atm != null)
        {
            _context.ATMs.Remove(atm);
            await _context.SaveChangesAsync();
        }
    }
}