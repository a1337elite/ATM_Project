using Microsoft.EntityFrameworkCore;
using System.Linq;

public class SupplyForecaster
{
    private readonly AppDbContext _context;

    public SupplyForecaster(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<int, (DateTime forecastDate, string message)>> ForecastSuppliesReplacement()
    {
        var result = new Dictionary<int, (DateTime, string)>();
        var supplies = await _context.Supplies.Include(s => s.ATM).ToListAsync();
        var consumptionRates = new Dictionary<SupplyType, double>
        {
            { SupplyType.ReceiptPaper, 5 },
            { SupplyType.InkCartridge, 0.2 },
            { SupplyType.CleaningKit, 0.05 },
            { SupplyType.RibbonModule, 5 }
        };

        foreach (var supply in supplies)
        {
            if (consumptionRates.TryGetValue(supply.Type, out var rate))
            {
                var daysLeft = supply.Quantity / rate;
                var forecastDate = DateTime.Now.AddDays(daysLeft);

                string message;
                if (daysLeft < 7)
                {
                    message = $"ТРЕБУЕТСЯ СРОЧНАЯ ЗАМЕНА! (осталось {daysLeft:0} дней)";
                }
                else if (daysLeft < 30)
                {
                    message = $"Запланируйте замену (осталось {daysLeft:0} дней)";
                }
                else
                {
                    message = $"Замена не требуется (осталось {daysLeft:0} дней)";
                }

                result.Add(supply.Id, (forecastDate, message));
            }
        }

        return result;
    }
}