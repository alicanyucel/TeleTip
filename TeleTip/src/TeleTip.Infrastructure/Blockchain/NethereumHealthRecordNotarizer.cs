using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MyProject.Application.Abstractions.Blockchain;
using MyProject.Domain.Abstractions;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace MyProject.Infrastructure.Blockchain;

internal sealed class NethereumHealthRecordNotarizer : IHealthRecordNotarizer
{
    private readonly BlockchainOptions _options;

    public NethereumHealthRecordNotarizer(IOptions<BlockchainOptions> options)
    {
        _options = options.Value;
    }

    public async Task<Result<BlockchainNotarizationResult>> NotarizeAsync(
        Guid recordId,
        string recordType,
        string canonicalPayload,
        CancellationToken cancellationToken = default)
    {
        if (recordId == Guid.Empty)
            return Result.Failure<BlockchainNotarizationResult>(
                Error.Validation(nameof(recordId), "Record id is required."));

        if (string.IsNullOrWhiteSpace(recordType))
            return Result.Failure<BlockchainNotarizationResult>(
                Error.Validation(nameof(recordType), "Record type is required."));

        if (string.IsNullOrWhiteSpace(canonicalPayload))
            return Result.Failure<BlockchainNotarizationResult>(
                Error.Validation(nameof(canonicalPayload), "Record payload is required."));

        if (string.IsNullOrWhiteSpace(_options.ContractAddress) ||
            string.IsNullOrWhiteSpace(_options.PrivateKey))
        {
            return Result.Failure<BlockchainNotarizationResult>(
                Error.Validation("Blockchain", "ContractAddress and PrivateKey must be configured."));
        }

        try
        {
            var recordHash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalPayload));
            var recordIdHash = SHA256.HashData(
                Encoding.UTF8.GetBytes($"{recordType}:{recordId:N}"));
            var web3 = new Web3(new Account(_options.PrivateKey, _options.ChainId), _options.RpcUrl);

            var query = new IsRegisteredFunction { RecordHash = recordHash };
            var queryHandler = web3.Eth.GetContractQueryHandler<IsRegisteredFunction>();
            var alreadyRegistered = await queryHandler.QueryAsync<bool>(_options.ContractAddress, query);

            if (alreadyRegistered)
            {
                return Result.Success(new BlockchainNotarizationResult(
                    recordId,
                    Convert.ToHexString(recordHash),
                    string.Empty,
                    0,
                    true));
            }

            var transaction = new RegisterHealthRecordFunction
            {
                RecordHash = recordHash,
                RecordId = recordIdHash
            };
            var transactionHandler = web3.Eth.GetContractTransactionHandler<RegisterHealthRecordFunction>();
            var receipt = await transactionHandler.SendRequestAndWaitForReceiptAsync(
                _options.ContractAddress,
                transaction);

            return Result.Success(new BlockchainNotarizationResult(
                recordId,
                Convert.ToHexString(recordHash),
                receipt.TransactionHash,
                (long)receipt.BlockNumber.Value,
                false));
        }
        catch (Exception exception)
        {
            return Result.Failure<BlockchainNotarizationResult>(
                new Error("Blockchain.NotarizationFailed", exception.Message));
        }
    }
}
