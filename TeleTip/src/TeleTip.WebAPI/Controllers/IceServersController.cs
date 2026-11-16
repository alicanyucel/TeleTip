using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyProject.Infrastructure.Persistence;
using MyProject.WebAPI.Options;
using MyProject.WebAPI.Security;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Authorize(Policy = "DoctorOrPatient")]
[Route("api/telecare/ice-servers")]
public sealed class IceServersController(IOptions<TurnServerOptions> options, AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetIceServers([FromQuery] Guid consultationId, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
            !await ConsultationAccess.IsParticipantAsync(db, consultationId, identity, ct))
        {
            return NotFound();
        }

        var iceServers = new List<IceServerConfiguration>
        {
            new(["stun:stun.l.google.com:19302"])
        };

        var settings = options.Value;
        if (settings.Urls.Length > 0)
        {
            var username = $"{DateTimeOffset.UtcNow.AddMinutes(settings.CredentialTtlMinutes).ToUnixTimeSeconds()}:{identity.UserId:D}";
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(settings.SharedSecret!));
            var credential = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(username)));
            iceServers.Add(new IceServerConfiguration(settings.Urls, username, credential));
        }

        return Ok(new IceServerListResponse(iceServers));
    }

    public sealed record IceServerConfiguration(string[] Urls, string? Username = null, string? Credential = null);
    public sealed record IceServerListResponse(IReadOnlyList<IceServerConfiguration> IceServers);
}
