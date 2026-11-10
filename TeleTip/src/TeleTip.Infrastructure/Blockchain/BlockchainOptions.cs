namespace MyProject.Infrastructure.Blockchain;

public sealed class BlockchainOptions
{
    public const string SectionName = "Blockchain";

    public string RpcUrl { get; set; } = "http://127.0.0.1:8545";
    public int ChainId { get; set; } = 31337;
    public string ContractAddress { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
