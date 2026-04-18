using ApuestasDeportivas.Domain.Enums;

namespace ApuestasDeportivas.Domain.Entities;

/// <summary>
/// Solicitud de depósito creada por usuario.
/// </summary>
public class DepositRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AppUser? User { get; set; }
}
