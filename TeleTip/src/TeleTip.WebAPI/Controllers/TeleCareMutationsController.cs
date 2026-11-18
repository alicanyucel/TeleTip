using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Persistence;
using MyProject.WebAPI.Security;
using ApiParticipantRole = MyProject.WebAPI.Contracts.ConsultationParticipantRole;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Authorize(Policy = "DoctorOrPatient")]
[Route("api/telecare")]
public sealed class TeleCareMutationsController(AppDbContext db) : ControllerBase
{
    [HttpPut("patients/{id:guid}")]
    public async Task<IActionResult> UpdatePatient(Guid id, UpdatePatientRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await UpdateEntity<Patient>(id,
            patient => patient.UpdateProfile(request.FirstName, request.LastName, request.DateOfBirth, request.PhoneNumber, request.Email),
            patient => Task.FromResult(identity.Role == ApiParticipantRole.Patient && identity.UserId == patient.Id), ct);
    }

    [HttpDelete("patients/{id:guid}")]
    public async Task<IActionResult> DeletePatient(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await SoftDeleteEntity<Patient>(id,
            patient => Task.FromResult(identity.Role == ApiParticipantRole.Patient && identity.UserId == patient.Id), ct);
    }

    [HttpPut("doctors/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateDoctor(Guid id, UpdateDoctorRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<Doctor>(id,
            doctor => doctor.UpdateProfile(request.FirstName, request.LastName, request.Specialization, request.LicenseNumber, request.PhoneNumber, request.Email),
            doctor => Task.FromResult(identity.UserId == doctor.Id), ct);
    }

    [HttpDelete("doctors/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeleteDoctor(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<Doctor>(id, doctor => Task.FromResult(identity.UserId == doctor.Id), ct);
    }

    [HttpPut("consultations/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateConsultation(Guid id, UpdateConsultationRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<TelemedicineConsultation>(id,
            consultation => consultation.Reschedule(request.ScheduledAt),
            consultation => Task.FromResult(consultation.DoctorId == identity.UserId), ct);
    }

    [HttpDelete("consultations/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeleteConsultation(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<TelemedicineConsultation>(id,
            consultation => Task.FromResult(consultation.DoctorId == identity.UserId), ct);
    }

    [HttpPut("devices/{id:guid}")]
    public async Task<IActionResult> UpdateDevice(Guid id, UpdateDeviceRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await UpdateEntity<IoTDevice>(id,
            device => device.UpdateDetails(request.Name, request.SerialNumber, request.Type),
            device => ConsultationAccess.CanAccessPatientAsync(db, device.PatientId, identity, ct), ct);
    }

    [HttpDelete("devices/{id:guid}")]
    public async Task<IActionResult> DeleteDevice(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await SoftDeleteEntity<IoTDevice>(id,
            device => ConsultationAccess.CanAccessPatientAsync(db, device.PatientId, identity, ct), ct);
    }

    [HttpPut("measurements/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateMeasurement(Guid id, UpdateMeasurementRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<HealthMeasurement>(id,
            measurement => measurement.UpdateMeasurement(request.Type, request.Value, request.Unit, request.MeasuredAt, request.DeviceId),
            measurement => ConsultationAccess.CanAccessPatientAsync(db, measurement.PatientId, identity, ct), ct);
    }

    [HttpDelete("measurements/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeleteMeasurement(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<HealthMeasurement>(id,
            measurement => ConsultationAccess.CanAccessPatientAsync(db, measurement.PatientId, identity, ct), ct);
    }

    [HttpPut("prescriptions/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdatePrescription(Guid id, UpdatePrescriptionRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<Prescription>(id,
            prescription => prescription.UpdateDetails(request.MedicationName, request.Dosage, request.Instructions, request.ExpiresAt),
            async prescription => prescription.DoctorId == identity.UserId &&
                await ConsultationAccess.CanAccessPatientAsync(db, prescription.PatientId, identity, ct), ct);
    }

    [HttpDelete("prescriptions/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeletePrescription(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<Prescription>(id,
            async prescription => prescription.DoctorId == identity.UserId &&
                await ConsultationAccess.CanAccessPatientAsync(db, prescription.PatientId, identity, ct), ct);
    }

    [HttpPut("reports/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateReport(Guid id, UpdateReportRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<MedicalReport>(id,
            report => report.UpdateDetails(request.Title, request.Content),
            async report => report.DoctorId == identity.UserId &&
                await ConsultationAccess.CanAccessPatientAsync(db, report.PatientId, identity, ct), ct);
    }

    [HttpDelete("reports/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeleteReport(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<MedicalReport>(id,
            async report => report.DoctorId == identity.UserId &&
                await ConsultationAccess.CanAccessPatientAsync(db, report.PatientId, identity, ct), ct);
    }

    [HttpPut("analyses/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateAnalysis(Guid id, UpdateAnalysisRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<AiAnalysis>(id,
            analysis => analysis.UpdateAnalysis(request.Type, request.AnalysisResult, request.RiskScore),
            analysis => ConsultationAccess.CanAccessPatientAsync(db, analysis.PatientId, identity, ct), ct);
    }

    [HttpDelete("analyses/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> DeleteAnalysis(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await SoftDeleteEntity<AiAnalysis>(id,
            analysis => ConsultationAccess.CanAccessPatientAsync(db, analysis.PatientId, identity, ct), ct);
    }

    [HttpPut("notifications/{id:guid}")]
    [Authorize(Policy = "Doctor")]
    public async Task<IActionResult> UpdateNotification(Guid id, UpdateNotificationRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity) || identity.Role != ApiParticipantRole.Doctor)
            return Forbid();

        return await UpdateEntity<Notification>(id,
            notification => notification.UpdateDetails(request.Type, request.Title, request.Message),
            async notification => notification.DoctorId == identity.UserId &&
                await ConsultationAccess.CanAccessPatientAsync(db, notification.PatientId, identity, ct), ct);
    }

    [HttpDelete("notifications/{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await SoftDeleteEntity<Notification>(id,
            async notification => identity.Role == ApiParticipantRole.Patient
                ? notification.PatientId == identity.UserId
                : notification.DoctorId == identity.UserId &&
                    await ConsultationAccess.CanAccessPatientAsync(db, notification.PatientId, identity, ct), ct);
    }

    [HttpPut("chat-messages/{id:guid}")]
    public async Task<IActionResult> UpdateChatMessage(Guid id, UpdateChatMessageRequest request, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await UpdateEntity<ChatMessage>(id,
            message => message.EditMessage(request.Message),
            async message => IsMessageAuthor(message, identity) &&
                await ConsultationAccess.IsParticipantAsync(db, message.ConsultationId, identity, ct), ct);
    }

    [HttpDelete("chat-messages/{id:guid}")]
    public async Task<IActionResult> DeleteChatMessage(Guid id, CancellationToken ct)
    {
        if (!ConsultationAccess.TryGetIdentity(User, out var identity))
            return Forbid();

        return await SoftDeleteEntity<ChatMessage>(id,
            async message => IsMessageAuthor(message, identity) &&
                await ConsultationAccess.IsParticipantAsync(db, message.ConsultationId, identity, ct), ct);
    }

    private async Task<IActionResult> UpdateEntity<TEntity>(
        Guid id,
        Func<TEntity, Result> update,
        Func<TEntity, Task<bool>> canAccess,
        CancellationToken ct)
        where TEntity : Entity
    {
        var entity = await db.Set<TEntity>().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null || !await canAccess(entity))
            return NotFound();

        var result = update(entity);
        if (result.IsFailure)
            return BadRequest(result.Error);

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<IActionResult> SoftDeleteEntity<TEntity>(
        Guid id,
        Func<TEntity, Task<bool>> canAccess,
        CancellationToken ct)
        where TEntity : Entity
    {
        var entity = await db.Set<TEntity>().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null || !await canAccess(entity))
            return NotFound();

        entity.SoftDelete();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static bool IsMessageAuthor(ChatMessage message, ConsultationIdentity identity)
        => string.Equals(message.SenderId, identity.UserId.ToString(), StringComparison.OrdinalIgnoreCase) &&
           message.SenderRole == (identity.Role == ApiParticipantRole.Doctor
               ? ConsultationParticipantRole.Doctor
               : ConsultationParticipantRole.Patient);

    public sealed record UpdatePatientRequest(string FirstName, string LastName, DateOnly DateOfBirth, string? PhoneNumber, string? Email);
    public sealed record UpdateDoctorRequest(string FirstName, string LastName, string Specialization, string LicenseNumber, string? PhoneNumber, string? Email);
    public sealed record UpdateConsultationRequest(DateTime ScheduledAt);
    public sealed record UpdateDeviceRequest(string Name, string SerialNumber, DeviceType Type);
    public sealed record UpdateMeasurementRequest(MeasurementType Type, decimal Value, string Unit, DateTime MeasuredAt, Guid? DeviceId);
    public sealed record UpdatePrescriptionRequest(string MedicationName, string Dosage, string Instructions, DateTime? ExpiresAt);
    public sealed record UpdateReportRequest(string Title, string Content);
    public sealed record UpdateAnalysisRequest(AnalysisType Type, string AnalysisResult, decimal? RiskScore);
    public sealed record UpdateNotificationRequest(NotificationType Type, string Title, string Message);
    public sealed record UpdateChatMessageRequest(string Message);
}
