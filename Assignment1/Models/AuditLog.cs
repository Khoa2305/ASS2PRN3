namespace Assignment1.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public short? UserId { get; set; }
    public string Action { get; set; } = null!;       // Create | Update | Delete
    public string EntityName { get; set; } = null!;
    public string? BeforeData { get; set; }
    public string? AfterData { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
