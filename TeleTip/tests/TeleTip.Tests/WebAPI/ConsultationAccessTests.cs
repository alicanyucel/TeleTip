using System.Security.Claims;
using FluentAssertions;
using MyProject.WebAPI.Contracts;
using MyProject.WebAPI.Options;
using MyProject.WebAPI.Security;

namespace MyProject.Tests.WebAPI;

public sealed class ConsultationAccessTests
{
    [Theory]
    [InlineData("Doctor", ConsultationParticipantRole.Doctor)]
    [InlineData("Patient", ConsultationParticipantRole.Patient)]
    public void TryGetIdentity_WithSupportedRoleAndGuidSubject_ReturnsIdentity(
        string role,
        ConsultationParticipantRole expectedRole)
    {
        var userId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", userId.ToString()),
            new Claim("role", role)
        ], "test"));

        var result = ConsultationAccess.TryGetIdentity(principal, out var identity);

        result.Should().BeTrue();
        identity.Should().Be(new ConsultationIdentity(userId, expectedRole));
    }

    [Fact]
    public void TryGetIdentity_WithAmbiguousRoles_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Doctor"),
            new Claim(ClaimTypes.Role, "Patient")
        ], "test"));

        ConsultationAccess.TryGetIdentity(principal, out _).Should().BeFalse();
    }

    [Fact]
    public void TryGetIdentity_WithNonGuidSubject_ReturnsFalse()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "doctor-1"),
            new Claim(ClaimTypes.Role, "Doctor")
        ], "test"));

        ConsultationAccess.TryGetIdentity(principal, out _).Should().BeFalse();
    }

    [Fact]
    public void TurnServerOptions_WithoutTurnUrls_IsValid()
    {
        new TurnServerOptions().IsValid().Should().BeTrue();
    }

    [Fact]
    public void TurnServerOptions_WithRestCredentialsAndSecureTtl_IsValid()
    {
        var options = new TurnServerOptions
        {
            Urls = ["turn:turn.example.com:3478", "turns:turn.example.com:5349"],
            SharedSecret = "shared-secret",
            CredentialTtlMinutes = 60
        };

        options.IsValid().Should().BeTrue();
    }

    [Fact]
    public void TurnServerOptions_WithUrlsButNoSharedSecret_IsInvalid()
    {
        var options = new TurnServerOptions { Urls = ["turn:turn.example.com:3478"] };

        options.IsValid().Should().BeFalse();
    }
}
