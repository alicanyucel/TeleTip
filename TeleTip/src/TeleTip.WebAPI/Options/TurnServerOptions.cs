namespace MyProject.WebAPI.Options;

public sealed class TurnServerOptions
{
    public const string SectionName = "Turn";

    public string[] Urls { get; set; } = [];
    public string? SharedSecret { get; set; }
    public int CredentialTtlMinutes { get; set; } = 60;

    public bool IsValid()
        => Urls.Length == 0 ||
            (Urls.All(url => url.StartsWith("turn:", StringComparison.OrdinalIgnoreCase) ||
                             url.StartsWith("turns:", StringComparison.OrdinalIgnoreCase)) &&
             !string.IsNullOrWhiteSpace(SharedSecret) &&
             CredentialTtlMinutes is >= 1 and <= 1440);
}
