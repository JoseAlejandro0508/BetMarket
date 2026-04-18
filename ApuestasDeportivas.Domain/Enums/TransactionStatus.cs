namespace ApuestasDeportivas.Domain.Enums;

/// <summary>
/// Estado de procesamiento de depósitos/retiros.
/// </summary>
public enum TransactionStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
