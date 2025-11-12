namespace OIS.Contracts.Events;

public record IssuancePublished(
    Guid issuanceId,
    Guid assetId,
    Guid issuerId,
    decimal? totalAmount,
    object? schedule,
    DateTime publishedAt,
    string? dltTxHash);

public record IssuanceClosed(
    Guid issuanceId,
    DateTime closedAt,
    string? dltTxHash);

public record OrderCreated(
    Guid orderId,
    Guid investorId,
    Guid issuanceId,
    decimal amount,
    DateTime createdAt);

public record OrderReserved(
    Guid orderId,
    Guid investorId,
    Guid issuanceId,
    decimal amount,
    DateTime reservedAt,
    string? bankTransferId);

public record OrderPaid(
    Guid orderId,
    Guid investorId,
    Guid issuanceId,
    decimal amount,
    DateTime paidAt,
    string txHash);

public record RegistryTransferred(
    Guid orderId,
    Guid issuanceId,
    Guid investorId,
    decimal amount,
    string txHash,
    Guid walletId,
    DateTime? transferredAt);

public record PayoutExecuted(
    Guid batchId,
    Guid? issuanceId,
    DateTime executedAt,
    decimal totalAmount,
    int itemCount,
    IReadOnlyList<PayoutItem> items);

public record PayoutItem(
    Guid itemId,
    Guid investorId,
    decimal amount,
    string status,
    string? bankRef,
    string? dltTxHash,
    string? failureReason);

public record ComplianceFlagged(
    Guid investorId,
    string reason,
    string severity,
    DateTime flaggedAt,
    object? details);

public record KycUpdated(
    Guid investorId,
    string status,
    string? reason,
    DateTime updatedAt);

public record AuditLogged(
    Guid id,
    Guid? actor,
    string? actorName,
    string action,
    string entity,
    Guid? entityId,
    object? payload,
    string? ip,
    string? userAgent,
    DateTime timestamp,
    string? result);

