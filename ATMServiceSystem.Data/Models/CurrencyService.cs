using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class CurrencyService
{
    private readonly HttpClient _httpClient;
    private const string CbrApiUrl = "https://www.cbr-xml-daily.ru/daily_json.js";

    public CurrencyService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<CurrencyRates> GetCurrencyRatesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(CbrApiUrl);
            var rates = JsonSerializer.Deserialize<CurrencyRates>(response);
            return rates;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении курсов валют: {ex.Message}");
            return null;
        }
    }
}

public class CurrencyRates
{
    public DateTime Date { get; set; }
    public Valute Valute { get; set; }
}

public class Valute
{
    public Currency USD { get; set; }
    public Currency EUR { get; set; }
}

public class Currency
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    public decimal Previous { get; set; }
}