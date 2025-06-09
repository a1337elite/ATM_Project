public class SupplyUsageHistory
{
    public int Id { get; set; }
    public int SupplyId { get; set; }
    public Supply Supply { get; set; }
    public DateTime RecordDate { get; set; }
    public double AmountUsed { get; set; }
    public string Notes { get; set; }
}