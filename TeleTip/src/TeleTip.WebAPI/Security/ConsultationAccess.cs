using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyProject.Infrastructure.Persistence;
using MyProject.WebAPI.Contracts;

namespace MyProject.WebAPI.Security;

public sealed record ConsultationIdentity(Guid UserId, ConsultationParticipantRole Role);

public static class ConsultationAccess
{
    public static bool TryGetIdentity(ClaimsPrincipal? principal, out ConsultationIdentity identity)
    {
        identity = default!;
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        var roleValues = principal.Claims
            .Where(claim => claim.Type == ClaimTypes.Role || claim.Type is "role" or "roles")
            .Select(claim => claim.Value);
        var isDoctor = roleValues.Any(role => string.Equals(role, "Doctor", StringComparison.OrdinalIgnoreCase));
        var isPatient = roleValues.Any(role => string.Equals(role, "Patient", StringComparison.OrdinalIgnoreCase));
        if (isDoctor == isPatient)
            return false;

        var userIdValue = principal.Claims
            .Where(claim => claim.Type is ClaimTypes.NameIdentifier or "sub" or "userId")
            .Select(claim => claim.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            return false;

        identity = new ConsultationIdentity(
            userId,
            isDoctor ? ConsultationParticipantRole.Doctor : ConsultationParticipantRole.Patient);
        return true;
    }

    public static Task<bool> IsParticipantAsync(
        AppDbContext db,
        Guid consultationId,
        ConsultationIdentity identity,
        CancellationToken cancellationToken = default)
        => db.TelemedicineConsultations
            .AsNoTracking()
            .AnyAsync(consultation => consultation.Id == consultationId &&
                (identity.Role == ConsultationParticipantRole.Doctor
                    ? consultation.DoctorId == identity.UserId
                    : consultation.PatientId == identity.UserId),
                cancellationToken);

    public static Task<bool> CanAccessPatientAsync(
        AppDbContext db,
        Guid patientId,
        ConsultationIdentity identity,
        CancellationToken cancellationToken = default)
        => identity.Role == ConsultationParticipantRole.Patient
            ? Task.FromResult(patientId == identity.UserId)
            : db.TelemedicineConsultations
                .AsNoTracking()
                .AnyAsync(consultation => consultation.DoctorId == identity.UserId &&
                    consultation.PatientId == patientId,
                    cancellationToken);
}
