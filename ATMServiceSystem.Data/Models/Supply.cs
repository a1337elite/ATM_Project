using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Supply
{
    public int Id { get; set; }

    [Required]
    public SupplyType Type { get; set; }  // Тип материала

    [Range(0, 1000)]
    public int Quantity { get; set; }     // Количество

    public DateTime LastReplenishmentDate { get; set; } // Дата последнего пополнения

    [ForeignKey("ATM")]
    public int ATMId { get; set; }       // Связь с банкоматом
    public ATM ATM { get; set; }
}

public enum SupplyType
{
    ReceiptPaper,    // Чековая лента
    InkCartridge,    // Картридж для печати
    CleaningKit,     // Набор для чистки
    RibbonModule,    // Модуль красящей ленты
    Other            // Прочее
}