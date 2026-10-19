namespace PharmacyMS.Infrastructure.Data;
/// <summary>Metadata-only audit trail; request bodies and personal secrets are never persisted.</summary>
public sealed class AuditEvent
{
    public long Id { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string ActorId { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
