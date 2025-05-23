public class ATM
{
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public string Location { get; set; }
    public DateTime InstallationDate { get; set; }
    public DateTime LastMaintenanceDate { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public List<Maintenance> Maintenances { get; set; } = new();
}