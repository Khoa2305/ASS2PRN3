namespace Assignment1.dto.response
{
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public short? UserId { get; set; }
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public string? BeforeData { get; set; }
        public string? AfterData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
