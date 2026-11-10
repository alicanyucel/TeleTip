using MyProject.Domain.Abstractions;

namespace MyProject.Application.Abstractions.Blockchain;

public interface IHealthRecordNotarizer
{
    Task<Result<BlockchainNotarizationResult>> NotarizeAsync(
        Guid recordId,
        string recordType,
        string canonicalPayload,
        CancellationToken cancellationToken = default);
}

public sealed record BlockchainNotarizationResult(
    Guid RecordId,
    string RecordHash,
    string TransactionHash,
    long BlockNumber,
    bool AlreadyRegistered);
