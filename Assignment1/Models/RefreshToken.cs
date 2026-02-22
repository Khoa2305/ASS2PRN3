namespace Assignment1.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public short AccountId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpirationDate { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
