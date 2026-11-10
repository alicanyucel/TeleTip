using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace MyProject.Infrastructure.Blockchain;

[Function("registerHealthRecord", "bool")]
public sealed class RegisterHealthRecordFunction : FunctionMessage
{
    [Parameter("bytes32", "recordHash", 1)]
    public byte[] RecordHash { get; set; } = [];

    [Parameter("bytes32", "recordId", 2)]
    public byte[] RecordId { get; set; } = [];
}

[Function("isRegistered", "bool")]
public sealed class IsRegisteredFunction : FunctionMessage
{
    [Parameter("bytes32", "recordHash", 1)]
    public byte[] RecordHash { get; set; } = [];
}

[Function("getRegistration", typeof(GetRegistrationOutputDTO))]
public sealed class GetRegistrationFunction : FunctionMessage
{
    [Parameter("bytes32", "recordHash", 1)]
    public byte[] RecordHash { get; set; } = [];
}

[FunctionOutput]
public sealed class GetRegistrationOutputDTO : IFunctionOutputDTO
{
    [Parameter("address", "submitter", 1)]
    public string Submitter { get; set; } = string.Empty;

    [Parameter("uint64", "registeredAt", 2)]
    public ulong RegisteredAt { get; set; }

    [Parameter("uint256", "blockNumber", 3)]
    public BigInteger BlockNumber { get; set; }
}
