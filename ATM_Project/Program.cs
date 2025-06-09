using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<AppDbContext>();
        services.AddTransient<App>();
        services.AddScoped<IRepository<Supply>, SupplyRepository>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>();
services.AddSingleton<CurrencyService>();
services.AddTransient<App>();

var serviceProvider = services.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

var app = serviceProvider.GetRequiredService<App>();
app.Run();

public class App
{
    private readonly AppDbContext _context;
    private readonly CurrencyService _currencyService;

    public App(AppDbContext context, CurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            DisplayCurrencyRates();
            Console.WriteLine("=== Система обслуживания банкоматов ==="); 
            Console.WriteLine("1. Управление банкоматами");
            Console.WriteLine("2. Управление обслуживанием");
            Console.WriteLine("3. Управление расходными материалами"); 
            Console.WriteLine("4. Выход");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ShowATMManagementMenu();
                    break;
                case "2":
                    ShowMaintenanceMenu();
                    break;
                case "3":
                    ShowSuppliesMenu();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private async void ShowATMManagementMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Управление банкоматами ===");
            Console.WriteLine("1. Просмотр всех банкоматов");
            Console.WriteLine("2. Добавить банкомат");
            Console.WriteLine("3. Редактировать банкомат");
            Console.WriteLine("4. Удалить банкомат");
            Console.WriteLine("5. Назад");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ShowAllATMs();
                    break;
                case "2":
                    await AddATM();
                    break;
                case "3":
                    await EditATM();
                    break;
                case "4":
                    await DeleteATM();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private async Task ShowAllATMs()
    {
        Console.Clear();
        Console.WriteLine("=== Список банкоматов ===");

        var atms = await _context.ATMs.ToListAsync();

        foreach (var atm in atms)
        {
            Console.WriteLine($"ID: {atm.Id}");
            Console.WriteLine($"Серийный номер: {atm.SerialNumber}");
            Console.WriteLine($"Местоположение: {atm.Location}");
            Console.WriteLine($"Баланс: {atm.CurrentBalance}");
            Console.WriteLine($"Статус: {(atm.IsActive ? "Активен" : "Неактивен")}");
            Console.WriteLine(new string('-', 30));
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата...");
        Console.ReadKey();
    }

    private async Task AddATM()
    {
        Console.Clear();
        Console.WriteLine("=== Добавление банкомата ===");

        var atm = new ATM();

        Console.Write("Серийный номер: ");
        atm.SerialNumber = Console.ReadLine();

        Console.Write("Местоположение: ");
        atm.Location = Console.ReadLine();

        atm.InstallationDate = DateTime.Now;
        atm.LastMaintenanceDate = DateTime.Now;

        Console.Write("Начальный баланс: ");
        decimal.TryParse(Console.ReadLine(), out var balance);
        atm.CurrentBalance = balance;

        atm.IsActive = true;

        await _context.ATMs.AddAsync(atm);
        await _context.SaveChangesAsync();

        Console.WriteLine("Банкомат успешно добавлен!");
        Thread.Sleep(2000);
    }
    private async Task EditATM()
    {
        Console.Clear();
        Console.WriteLine("=== Редактирование банкомата ===");

        Console.Write("Введите ID банкомата для редактирования: ");
        if (!int.TryParse(Console.ReadLine(), out var atmId))
        {
            Console.WriteLine("Неверный формат ID!");
            Thread.Sleep(2000);
            return;
        }

        var atm = await _context.ATMs.FindAsync(atmId);
        if (atm == null)
        {
            Console.WriteLine("Банкомат не найден!");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine("\nТекущие данные:");
        Console.WriteLine($"1. Серийный номер: {atm.SerialNumber}");
        Console.WriteLine($"2. Местоположение: {atm.Location}");
        Console.WriteLine($"3. Баланс: {atm.CurrentBalance}");
        Console.WriteLine($"4. Статус: {(atm.IsActive ? "Активен" : "Неактивен")}");

        Console.WriteLine("\nВведите номера полей для изменения (через запятую):");
        var fieldsToUpdate = Console.ReadLine()?.Split(',');

        if (fieldsToUpdate == null || fieldsToUpdate.Length == 0)
        {
            Console.WriteLine("Ничего не изменено.");
            Thread.Sleep(2000);
            return;
        }

        foreach (var field in fieldsToUpdate)
        {
            switch (field.Trim())
            {
                case "1":
                    Console.Write("Новый серийный номер: ");
                    atm.SerialNumber = Console.ReadLine();
                    break;
                case "2":
                    Console.Write("Новое местоположение: ");
                    atm.Location = Console.ReadLine();
                    break;
                case "3":
                    Console.Write("Новый баланс: ");
                    if (decimal.TryParse(Console.ReadLine(), out var newBalance))
                    {
                        atm.CurrentBalance = newBalance;
                    }
                    break;
                case "4":
                    Console.Write("Активен (y/n): ");
                    atm.IsActive = Console.ReadLine()?.ToLower() == "y";
                    break;
            }
        }

        _context.ATMs.Update(atm);
        await _context.SaveChangesAsync();

        Console.WriteLine("Изменения сохранены!");
        Thread.Sleep(2000);
    }

    private async Task DeleteATM()
    {
        Console.Clear();
        Console.WriteLine("=== Удаление банкомата ===");

        Console.Write("Введите ID банкомата для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out var atmId))
        {
            Console.WriteLine("Неверный формат ID!");
            Thread.Sleep(2000);
            return;
        }

        var atm = await _context.ATMs.FindAsync(atmId);
        if (atm == null)
        {
            Console.WriteLine("Банкомат не найден!");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine($"\nВы действительно хотите удалить банкомат {atm.SerialNumber}? (y/n)");
        var confirm = Console.ReadLine()?.ToLower() == "y";

        if (confirm)
        {
            _context.ATMs.Remove(atm);
            await _context.SaveChangesAsync();
            Console.WriteLine("Банкомат удалён!");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }

        Thread.Sleep(2000);
    }









    private async void ShowMaintenanceMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Управление обслуживанием ===");
            Console.WriteLine("1. Просмотр всех обслуживаний");
            Console.WriteLine("2. Добавить запись об обслуживании");
            Console.WriteLine("3. Найти обслуживание по ID банкомата");
            Console.WriteLine("4. Назад");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ShowAllMaintenances();
                    break;
                case "2":
                    await AddMaintenance();
                    break;
                case "3":
                    await FindMaintenancesByATM();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private async Task ShowAllMaintenances()
    {
        Console.Clear();
        Console.WriteLine("=== Все записи об обслуживании ===");

        var maintenances = await _context.Maintenances
            .Include(m => m.ATM)
            .ToListAsync();

        foreach (var m in maintenances)
        {
            Console.WriteLine($"ID: {m.Id}");
            Console.WriteLine($"Банкомат: {m.ATM.SerialNumber} ({m.ATM.Location})");
            Console.WriteLine($"Дата: {m.MaintenanceDate:dd.MM.yyyy}");
            Console.WriteLine($"Тип: {m.ServiceType}");
            Console.WriteLine($"Техник: {m.TechnicianName}");
            Console.WriteLine(new string('-', 30));
        }

        Console.WriteLine("\nНажмите любую клавишу для возврата...");
        Console.ReadKey();
    }

    private async Task AddMaintenance()
    {
        Console.Clear();
        Console.WriteLine("=== Добавление записи об обслуживании ===");

        var atms = await _context.ATMs.ToListAsync();
        if (!atms.Any())
        {
            Console.WriteLine("Нет доступных банкоматов!");
            Thread.Sleep(2000);
            return;
        }

        Console.WriteLine("Список банкоматов:");
        foreach (var atm in atms)
        {
            Console.WriteLine($"{atm.Id}. {atm.SerialNumber} ({atm.Location})");
        }

        Console.Write("\nВыберите ID банкомата: ");
        if (!int.TryParse(Console.ReadLine(), out var atmId))
        {
            Console.WriteLine("Неверный формат ID!");
            Thread.Sleep(2000);
            return;
        }

        var selectedATM = await _context.ATMs.FindAsync(atmId);
        if (selectedATM == null)
        {
            Console.WriteLine("Банкомат не найден!");
            Thread.Sleep(2000);
            return;
        }

        var maintenance = new Maintenance
        {
            ATMId = atmId,
            MaintenanceDate = DateTime.Now
        };

        Console.Write("Имя техника: ");
        maintenance.TechnicianName = Console.ReadLine();

        Console.WriteLine("Тип обслуживания:");
        var types = Enum.GetValues(typeof(MaintenanceType));
        foreach (MaintenanceType type in types)
        {
            Console.WriteLine($"{(int)type}. {type}");
        }
        Console.Write("Выберите тип: ");
        if (Enum.TryParse(Console.ReadLine(), out MaintenanceType selectedType))
        {
            maintenance.ServiceType = selectedType;
        }

        Console.Write("Описание: ");
        maintenance.Description = Console.ReadLine();

        await _context.Maintenances.AddAsync(maintenance);
        await _context.SaveChangesAsync();

        selectedATM.LastMaintenanceDate = DateTime.Now;
        _context.ATMs.Update(selectedATM);
        await _context.SaveChangesAsync();

        Console.WriteLine("Запись об обслуживании добавлена!");
        Thread.Sleep(2000);
    }

    private async Task FindMaintenancesByATM()
{
    Console.Clear();
    Console.WriteLine("=== Поиск обслуживаний по банкомату ===");
    
    Console.Write("Введите ID банкомата: ");
    if (!int.TryParse(Console.ReadLine(), out var atmId))
    {
        Console.WriteLine("Неверный формат ID!");
        Thread.Sleep(2000);
        return;
    }

    var atm = await _context.ATMs.FindAsync(atmId);
    if (atm == null)
    {
        Console.WriteLine("Банкомат не найден!");
        Thread.Sleep(2000);
        return;
    }

    var maintenances = await _context.Maintenances
        .Where(m => m.ATMId == atmId)
        .OrderByDescending(m => m.MaintenanceDate)
        .ToListAsync();

    Console.WriteLine($"\nИстория обслуживания банкомата {atm.SerialNumber} ({atm.Location}):");
    
    if (!maintenances.Any())
    {
        Console.WriteLine("Нет записей об обслуживании");
    }
    else
    {
        foreach (var m in maintenances)
        {
            Console.WriteLine($"Дата: {m.MaintenanceDate:dd.MM.yyyy}");
            Console.WriteLine($"Тип: {m.ServiceType}");
            Console.WriteLine($"Техник: {m.TechnicianName}");
            Console.WriteLine($"Описание: {m.Description}");
            Console.WriteLine(new string('-', 30));
        }
    }
    Console.WriteLine("\nНажмите любую клавишу для возврата...");
    Console.ReadKey();
    }

    private async void ShowSuppliesMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Управление расходными материалами ===");
            Console.WriteLine("1. Просмотр всех материалов");
            Console.WriteLine("2. Добавить материал");
            Console.WriteLine("3. Назад");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await ShowAllSupplies();
                    break;
                case "2":
                    await AddSupply();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    private async Task ShowAllSupplies()
    {
        Console.Clear();
        var supplies = await _context.Supplies.Include(s => s.ATM).ToListAsync();

        Console.WriteLine("=== Список расходных материалов ===");
        Console.WriteLine("Прогноз замены материалов:\n");

        foreach (var s in supplies)
        {
            Console.WriteLine($"ID: {s.Id}");
            Console.WriteLine($"Тип: {s.Type}");
            Console.WriteLine($"Банкомат: {s.ATM.SerialNumber} ({s.ATM.Location})");
            Console.WriteLine($"Количество: {s.Quantity}");
            Console.WriteLine($"Последнее пополнение: {s.LastReplenishmentDate:dd.MM.yyyy}");

            var forecast = GetSupplyForecast(s);
            Console.ForegroundColor = forecast.color;
            Console.WriteLine($"Прогноз замены: {forecast.message}");
            Console.ResetColor();

            Console.WriteLine(new string('-', 30));
        }

        Console.WriteLine("\nЛегенда:");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Красный");
        Console.ResetColor();
        Console.WriteLine(" - требуется срочная замена");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Желтый");
        Console.ResetColor();
        Console.WriteLine(" - запланируйте замену");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Зеленый");
        Console.ResetColor();
        Console.WriteLine(" - замена не требуется");

        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }

    private async Task AddSupply()
    {
        Console.Clear();
        Console.WriteLine("=== Добавление расходного материала ===");

        var atms = await _context.ATMs.ToListAsync();
        foreach (var a in atms)
        {
            Console.WriteLine($"{a.Id}. {a.SerialNumber} ({a.Location})");
        }
        Console.Write("Выберите ID банкомата: ");
        if (!int.TryParse(Console.ReadLine(), out var atmId))
        {
            Console.WriteLine("Ошибка ввода!");
            return;
        }

        Console.WriteLine("Типы материалов:");
        foreach (SupplyType type in Enum.GetValues(typeof(SupplyType)))
        {
            Console.WriteLine($"{(int)type}. {type}");
        }
        Console.Write("Выберите тип: ");
        if (!Enum.TryParse(Console.ReadLine(), out SupplyType supplyType))
        {
            Console.WriteLine("Неверный тип!");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Некорректное количество!");
            return;
        }

        var supply = new Supply
        {
            ATMId = atmId,
            Type = supplyType,
            Quantity = quantity,
            LastReplenishmentDate = DateTime.Now
        };

        await _context.Supplies.AddAsync(supply);
        await _context.SaveChangesAsync();

        Console.WriteLine("Материал добавлен!");
        Thread.Sleep(2000);
    }







    private async Task DisplayCurrencyRates()
    {
        var rates = await _currencyService.GetCurrencyRatesAsync();
        if (rates != null)
        {
            Console.WriteLine();
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine($" Курсы валют ЦБ РФ на {rates.Date:dd.MM.yyyy}");
            Console.WriteLine($" USD: {rates.Valute.USD.Value} ₽ (изменение: {rates.Valute.USD.Value - rates.Valute.USD.Previous:+#.##;-#.##;0} ₽)");
            Console.WriteLine($" EUR: {rates.Valute.EUR.Value} ₽ (изменение: {rates.Valute.EUR.Value - rates.Valute.EUR.Previous:+#.##;-#.##;0} ₽)");
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine();
        }
    }







    private (string message, ConsoleColor color) GetSupplyForecast(Supply supply)
    {
        int daysUntilReplacement;
        string message;
        ConsoleColor color;

        switch (supply.Type)
        {
            case SupplyType.ReceiptPaper:
                daysUntilReplacement = supply.Quantity * 2;
                message = daysUntilReplacement switch
                {
                    < 7 => $"ТРЕБУЕТСЯ СРОЧНАЯ ЗАМЕНА! (осталось ~{daysUntilReplacement} дней)",
                    < 30 => $"Запланируйте замену через ~{daysUntilReplacement} дней",
                    _ => $"Замена не требуется (хватит на ~{daysUntilReplacement} дней)"
                };
                color = daysUntilReplacement switch
                {
                    < 7 => ConsoleColor.Red,
                    < 30 => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                break;

            case SupplyType.InkCartridge:
                daysUntilReplacement = supply.Quantity * 10;
                message = daysUntilReplacement switch
                {
                    < 7 => $"ТРЕБУЕТСЯ СРОЧНАЯ ЗАМЕНА! (осталось ~{daysUntilReplacement} дней)",
                    < 30 => $"Запланируйте замену через ~{daysUntilReplacement} дней",
                    _ => $"Замена не требуется (хватит на ~{daysUntilReplacement} дней)"
                };
                color = daysUntilReplacement switch
                {
                    < 7 => ConsoleColor.Red,
                    < 30 => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                break;

            case SupplyType.CleaningKit:
                daysUntilReplacement = supply.Quantity * 15;
                message = daysUntilReplacement switch
                {
                    < 7 => $"ТРЕБУЕТСЯ СРОЧНАЯ ЗАМЕНА! (осталось ~{daysUntilReplacement} дней)",
                    < 30 => $"Запланируйте замену через ~{daysUntilReplacement} дней",
                    _ => $"Замена не требуется (хватит на ~{daysUntilReplacement} дней)"
                };
                color = daysUntilReplacement switch
                {
                    < 7 => ConsoleColor.Red,
                    < 30 => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                break;

            case SupplyType.RibbonModule:
                daysUntilReplacement = supply.Quantity * 15;
                message = daysUntilReplacement switch
                {
                    < 7 => $"ТРЕБУЕТСЯ СРОЧНАЯ ЗАМЕНА! (осталось ~{daysUntilReplacement} дней)",
                    < 30 => $"Запланируйте замену через ~{daysUntilReplacement} дней",
                    _ => $"Замена не требуется (хватит на ~{daysUntilReplacement} дней)"
                };
                color = daysUntilReplacement switch
                {
                    < 7 => ConsoleColor.Red,
                    < 30 => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                break;

            default:
                message = "Неизвестный тип материала";
                color = ConsoleColor.Gray;
                break;
        }

        return (message, color);
    }
}