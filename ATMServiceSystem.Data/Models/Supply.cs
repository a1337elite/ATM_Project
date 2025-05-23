using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Supply
{
    public int Id { get; set; }

    [Required]
    public SupplyType Type { get; set; }  // ��� ���������

    [Range(0, 1000)]
    public int Quantity { get; set; }     // ����������

    public DateTime LastReplenishmentDate { get; set; } // ���� ���������� ����������

    [ForeignKey("ATM")]
    public int ATMId { get; set; }       // ����� � ����������
    public ATM ATM { get; set; }
}

public enum SupplyType
{
    ReceiptPaper,    // ������� �����
    InkCartridge,    // �������� ��� ������
    CleaningKit,     // ����� ��� ������
    RibbonModule,    // ������ �������� �����
    Other            // ������
}