using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyProject.Application.Abstractions.MultiTenancy;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Persistence;
using MyProject.WebAPI.Controllers;
using NSubstitute;

namespace MyProject.Tests.WebAPI;

public sealed class TeleCareSecurityIntegrationTests
{
    [Fact]
    public async Task AnonymousUser_CannotReadChatHistory()
    {
        await using var application = await SecurityTestApplication.StartAsync();

        var response = await application.Client.GetAsync($"/api/telecare/chat-messages?consultationId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Patient_CannotEnumeratePatientRecords()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var patient = Patient.Create("Test", "Patient", new DateOnly(1990, 1, 1)).Value;
        await application.SeedAsync(patient);
        application.AuthenticateAs("Patient", patient.Id);

        var response = await application.Client.GetAsync("/api/telecare/patients");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Patient_CannotReadAnotherPatientsRecordByChangingId()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var currentPatient = Patient.Create("Current", "Patient", new DateOnly(1990, 1, 1)).Value;
        var otherPatient = Patient.Create("Other", "Patient", new DateOnly(1991, 1, 1)).Value;
        await application.SeedAsync(currentPatient, otherPatient);
        application.AuthenticateAs("Patient", currentPatient.Id);

        var response = await application.Client.GetAsync($"/api/telecare/patients/{otherPatient.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("Other");
    }

    [Fact]
    public async Task Patient_CannotGenerateAiAnalysis()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var patient = Patient.Create("Test", "Patient", new DateOnly(1990, 1, 1)).Value;
        await application.SeedAsync(patient);
        application.AuthenticateAs("Patient", patient.Id);

        var response = await application.Client.PostAsJsonAsync("/api/telecare/analyses/generate", new
        {
            patientId = patient.Id,
            type = "General"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Doctor_CannotGenerateAiAnalysisForUnassignedPatient()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var doctor = Doctor.Create("Test", "Doctor", "General", "LIC-1").Value;
        var patient = Patient.Create("Test", "Patient", new DateOnly(1990, 1, 1)).Value;
        await application.SeedAsync(doctor, patient);
        application.AuthenticateAs("Doctor", doctor.Id);

        var response = await application.Client.PostAsJsonAsync("/api/telecare/analyses/generate", new
        {
            patientId = patient.Id,
            type = "General"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Doctor_CannotReadChatHistoryForUnassignedConsultation()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var doctor = Doctor.Create("Assigned", "Doctor", "General", "LIC-1").Value;
        var otherDoctor = Doctor.Create("Other", "Doctor", "General", "LIC-2").Value;
        var patient = Patient.Create("Test", "Patient", new DateOnly(1990, 1, 1)).Value;
        var consultation = TelemedicineConsultation.Create(patient.Id, otherDoctor.Id, DateTime.UtcNow).Value;
        var message = ChatMessage.Create(consultation.Id, otherDoctor.Id.ToString(),
            ConsultationParticipantRole.Doctor, "private consultation message").Value;
        await application.SeedAsync(doctor, otherDoctor, patient, consultation, message);
        application.AuthenticateAs("Doctor", doctor.Id);

        var response = await application.Client.GetAsync(
            $"/api/telecare/chat-messages?consultationId={consultation.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("private consultation message");
    }

    [Fact]
    public async Task AssignedPatient_CanReadOnlyTheirPagedChatHistory()
    {
        await using var application = await SecurityTestApplication.StartAsync();
        var patient = Patient.Create("Assigned", "Patient", new DateOnly(1990, 1, 1)).Value;
        var otherPatient = Patient.Create("Other", "Patient", new DateOnly(1991, 1, 1)).Value;
        var doctor = Doctor.Create("Assigned", "Doctor", "General", "LIC-1").Value;
        var consultation = TelemedicineConsultation.Create(patient.Id, doctor.Id, DateTime.UtcNow).Value;
        var unrelatedConsultation = TelemedicineConsultation.Create(otherPatient.Id, doctor.Id, DateTime.UtcNow).Value;
        var firstMessage = ChatMessage.Create(consultation.Id, doctor.Id.ToString(),
            ConsultationParticipantRole.Doctor, "first message").Value;
        var secondMessage = ChatMessage.Create(consultation.Id, patient.Id.ToString(),
            ConsultationParticipantRole.Patient, "second message").Value;
        var unrelatedMessage = ChatMessage.Create(unrelatedConsultation.Id, otherPatient.Id.ToString(),
            ConsultationParticipantRole.Patient, "unrelated private message").Value;
        await application.SeedAsync(patient, otherPatient, doctor, consultation,
            unrelatedConsultation, firstMessage, secondMessage, unrelatedMessage);
        application.AuthenticateAs("Patient", patient.Id);

        var response = await application.Client.GetAsync(
            $"/api/telecare/chat-messages?consultationId={consultation.Id}&page=1&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var content = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        content.RootElement.GetProperty("totalCount").GetInt32().Should().Be(2);
        content.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        content.RootElement.GetProperty("pageSize").GetInt32().Should().Be(1);
        var items = content.RootElement.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("message").GetString().Should().BeOneOf("first message", "second message");
        content.RootElement.ToString().Should().NotContain("unrelated private message");
    }

    private sealed class SecurityTestApplication : IAsyncDisposable
    {
        private readonly WebApplication _application;

        private SecurityTestApplication(WebApplication application)
        {
            _application = application;
            Client = application.GetTestClient();
        }

        public HttpClient Client { get; }

        public static async Task<SecurityTestApplication> StartAsync()
        {
            var builder = WebApplication.CreateBuilder();
            var databaseName = Guid.NewGuid().ToString();
            builder.WebHost.UseTestServer();
            builder.Services.AddControllers().AddApplicationPart(typeof(TeleCareController).Assembly);
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
            builder.Services.AddSingleton<IPublisher>(Substitute.For<IPublisher>());
            builder.Services.AddSingleton<ITenantService, TestTenantService>();
            builder.Services.AddSingleton(Substitute.For<ISender>());
            builder.Services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Doctor", policy => policy.RequireRole("Doctor"));
                options.AddPolicy("Patient", policy => policy.RequireRole("Patient"));
                options.AddPolicy("DoctorOrPatient", policy => policy.RequireRole("Doctor", "Patient"));
            });

            var application = builder.Build();
            application.UseAuthentication();
            application.UseAuthorization();
            application.MapControllers();
            await application.StartAsync();
            return new SecurityTestApplication(application);
        }

        public void AuthenticateAs(string role, Guid userId)
        {
            Client.DefaultRequestHeaders.Remove("X-Test-Role");
            Client.DefaultRequestHeaders.Remove("X-Test-User");
            Client.DefaultRequestHeaders.Add("X-Test-Role", role);
            Client.DefaultRequestHeaders.Add("X-Test-User", userId.ToString());
        }

        public async Task SeedAsync(params object[] entities)
        {
            await using var scope = _application.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.AddRange(entities);
            await db.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _application.DisposeAsync();
        }
    }

    private sealed class TestTenantService : ITenantService
    {
        public string? GetCurrentTenantId() => null;
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Role", out var roles) ||
                !Request.Headers.TryGetValue("X-Test-User", out var userIds))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new[]
            {
                new Claim("sub", userIds.ToString()),
                new Claim(ClaimTypes.Role, roles.ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
