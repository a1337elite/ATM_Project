public class Maintenance
{
    public int Id { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string TechnicianName { get; set; }
    public MaintenanceType ServiceType { get; set; }
    public string Description { get; set; }
    public int ATMId { get; set; }
    public ATM ATM { get; set; }
}