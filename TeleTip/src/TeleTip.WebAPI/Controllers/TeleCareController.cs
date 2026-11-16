using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Application.Features.TeleCare;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Persistence;
using MyProject.WebAPI.Security;

namespace MyProject.WebAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/telecare")]
    public sealed class TeleCareController(ISender sender, AppDbContext db) : ControllerBase
    {
        [HttpGet("patients")]
        [Authorize(Policy = "Doctor")]
        public Task<IActionResult> GetPatients(CancellationToken ct) => SendList(new GetPatientsQuery(), ct);

        [HttpGet("patients/{id:guid}")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> GetPatient(Guid id, CancellationToken ct)
        {
            if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
                (identity.Role == MyProject.WebAPI.Contracts.ConsultationParticipantRole.Patient && identity.UserId != id))
            {
                return Forbid();
            }

            return await SendOne(new GetPatientByIdQuery(id), ct);
        }

        [HttpPost("patients")]
        [Authorize(Policy = "Doctor")]
        public Task<IActionResult> CreatePatient(CreatePatientCommand command, CancellationToken ct) => Create(command, nameof(GetPatient), ct);

        [HttpGet("doctors")]
        [Authorize(Policy = "DoctorOrPatient")]
        public Task<IActionResult> GetDoctors(CancellationToken ct) => SendList(new GetDoctorsQuery(), ct);

        [HttpGet("doctors/{id:guid}")]
        [Authorize(Policy = "DoctorOrPatient")]
        public Task<IActionResult> GetDoctor(Guid id, CancellationToken ct) => SendOne(new GetDoctorByIdQuery(id), ct);

        [HttpPost("doctors")]
        [Authorize(Policy = "Doctor")]
        public Task<IActionResult> CreateDoctor(CreateDoctorCommand command, CancellationToken ct) => Create(command, nameof(GetDoctor), ct);

        [HttpGet("consultations")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> GetConsultations(CancellationToken ct)
        {
            if (!ConsultationAccess.TryGetIdentity(User, out var identity))
                return Forbid();

            var consultations = await db.TelemedicineConsultations
                .AsNoTracking()
                .Where(consultation => identity.Role == MyProject.WebAPI.Contracts.ConsultationParticipantRole.Doctor
                    ? consultation.DoctorId == identity.UserId
                    : consultation.PatientId == identity.UserId)
                .ToListAsync(ct);

            return Ok(consultations);
        }

        [HttpGet("consultations/{id:guid}")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> GetConsultation(Guid id, CancellationToken ct)
        {
            if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
                !await ConsultationAccess.IsParticipantAsync(db, id, identity, ct))
            {
                return NotFound();
            }

            return await SendOne(new GetConsultationByIdQuery(id), ct);
        }

        [HttpPost("consultations")]
        [Authorize(Policy = "DoctorOrPatient")]
        public Task<IActionResult> CreateConsultation(CreateConsultationCommand command, CancellationToken ct)
        {
            if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
                (identity.Role == MyProject.WebAPI.Contracts.ConsultationParticipantRole.Doctor && command.DoctorId != identity.UserId) ||
                (identity.Role == MyProject.WebAPI.Contracts.ConsultationParticipantRole.Patient && command.PatientId != identity.UserId))
            {
                return Task.FromResult<IActionResult>(Forbid());
            }

            return Create(command, nameof(GetConsultation), ct);
        }

        [HttpGet("devices")]
        public Task<IActionResult> GetDevices(CancellationToken ct) => SendList(new GetIoTDevicesQuery(), ct);
        [HttpGet("devices/{id:guid}")]
        public Task<IActionResult> GetDevice(Guid id, CancellationToken ct) => SendOne(new GetIoTDeviceByIdQuery(id), ct);
        [HttpPost("devices")]
        public Task<IActionResult> CreateDevice(CreateIoTDeviceCommand command, CancellationToken ct) => Create(command, nameof(GetDevice), ct);

        [HttpGet("measurements")]
        public Task<IActionResult> GetMeasurements(CancellationToken ct) => SendList(new GetHealthMeasurementsQuery(), ct);
        [HttpGet("measurements/{id:guid}")]
        public Task<IActionResult> GetMeasurement(Guid id, CancellationToken ct) => SendOne(new GetHealthMeasurementByIdQuery(id), ct);
        [HttpPost("measurements")]
        public Task<IActionResult> CreateMeasurement(CreateHealthMeasurementCommand command, CancellationToken ct) => Create(command, nameof(GetMeasurement), ct);

        [HttpGet("prescriptions")]
        public Task<IActionResult> GetPrescriptions(CancellationToken ct) => SendList(new GetPrescriptionsQuery(), ct);
        [HttpGet("prescriptions/{id:guid}")]
        public Task<IActionResult> GetPrescription(Guid id, CancellationToken ct) => SendOne(new GetPrescriptionByIdQuery(id), ct);
        [HttpPost("prescriptions")]
        public Task<IActionResult> CreatePrescription(CreatePrescriptionCommand command, CancellationToken ct) => Create(command, nameof(GetPrescription), ct);

        [HttpGet("reports")]
        public Task<IActionResult> GetReports(CancellationToken ct) => SendList(new GetMedicalReportsQuery(), ct);
        [HttpGet("reports/{id:guid}")]
        public Task<IActionResult> GetReport(Guid id, CancellationToken ct) => SendOne(new GetMedicalReportByIdQuery(id), ct);
        [HttpPost("reports")]
        public Task<IActionResult> CreateReport(CreateMedicalReportCommand command, CancellationToken ct) => Create(command, nameof(GetReport), ct);

        [HttpGet("analyses")]
        public Task<IActionResult> GetAnalyses(CancellationToken ct) => SendList(new GetAiAnalysesQuery(), ct);
        [HttpGet("analyses/{id:guid}")]
        public Task<IActionResult> GetAnalysis(Guid id, CancellationToken ct) => SendOne(new GetAiAnalysisByIdQuery(id), ct);
        [HttpPost("analyses")]
        public Task<IActionResult> CreateAnalysis(CreateAiAnalysisCommand command, CancellationToken ct) => Create(command, nameof(GetAnalysis), ct);

        [HttpGet("notifications")]
        public Task<IActionResult> GetNotifications(CancellationToken ct) => SendList(new GetNotificationsQuery(), ct);
        [HttpGet("notifications/{id:guid}")]
        public Task<IActionResult> GetNotification(Guid id, CancellationToken ct) => SendOne(new GetNotificationByIdQuery(id), ct);
        [HttpPost("notifications")]
        public Task<IActionResult> CreateNotification(CreateNotificationCommand command, CancellationToken ct) => Create(command, nameof(GetNotification), ct);

        [HttpGet("chat-messages")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> GetChatMessages(
            [FromQuery] Guid consultationId,
            [FromQuery] ConsultationParticipantRole? senderRole,
            [FromQuery] DateTimeOffset? sentAfter,
            [FromQuery] DateTimeOffset? sentBefore,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            if (consultationId == Guid.Empty || page < 1 || pageSize is < 1 or > 100 ||
                page > int.MaxValue / pageSize ||
                (sentAfter.HasValue && sentBefore.HasValue && sentAfter > sentBefore))
            {
                return BadRequest("Consultation, date range, or pagination parameters are invalid.");
            }

            if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
                !await ConsultationAccess.IsParticipantAsync(db, consultationId, identity, ct))
            {
                return NotFound();
            }

            var query = db.ChatMessages.AsNoTracking()
                .Where(message => message.ConsultationId == consultationId);
            if (senderRole.HasValue)
                query = query.Where(message => message.SenderRole == senderRole.Value);
            if (sentAfter.HasValue)
                query = query.Where(message => message.SentAt >= sentAfter.Value.UtcDateTime);
            if (sentBefore.HasValue)
                query = query.Where(message => message.SentAt <= sentBefore.Value.UtcDateTime);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(message => message.SentAt)
                .ThenBy(message => message.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Ok(new ChatMessageHistoryPage(items, page, pageSize, totalCount));
        }

        [HttpGet("chat-messages/{id:guid}")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> GetChatMessage(Guid id, CancellationToken ct)
        {
            var message = await db.ChatMessages.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
            if (message is null || !ConsultationAccess.TryGetIdentity(User, out var identity) ||
                !await ConsultationAccess.IsParticipantAsync(db, message.ConsultationId, identity, ct))
            {
                return NotFound();
            }

            return Ok(message);
        }

        [HttpPost("chat-messages")]
        [Authorize(Policy = "DoctorOrPatient")]
        public async Task<IActionResult> CreateChatMessage(CreateChatMessageRequest request, CancellationToken ct)
        {
            if (!ConsultationAccess.TryGetIdentity(User, out var identity) ||
                !await ConsultationAccess.IsParticipantAsync(db, request.ConsultationId, identity, ct))
            {
                return NotFound();
            }

            var senderRole = identity.Role == MyProject.WebAPI.Contracts.ConsultationParticipantRole.Doctor
                ? MyProject.Domain.Entities.ConsultationParticipantRole.Doctor
                : MyProject.Domain.Entities.ConsultationParticipantRole.Patient;
            var command = new CreateChatMessageCommand(
                request.ConsultationId,
                identity.UserId.ToString(),
                senderRole,
                request.Message);
            var result = await sender.Send(command, ct);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetChatMessage), new { id = result.Value }, result.Value)
                : BadRequest(result.Error);
        }

        public sealed record CreateChatMessageRequest(Guid ConsultationId, string Message);
        public sealed record ChatMessageHistoryPage(
            IReadOnlyList<ChatMessage> Items,
            int Page,
            int PageSize,
            int TotalCount);

        private async Task<IActionResult> SendList<T>(IRequest<Result<T>> request, CancellationToken ct)
        {
            var result = await sender.Send(request, ct);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        private async Task<IActionResult> SendOne<T>(IRequest<Result<T>> request, CancellationToken ct)
        {
            var result = await sender.Send(request, ct);
            return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
        }

        private async Task<IActionResult> Create<TCommand>(TCommand command, string action, CancellationToken ct)
            where TCommand : IRequest<MyProject.Domain.Abstractions.Result<Guid>>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? CreatedAtAction(action, new { id = result.Value }, result.Value)
                : BadRequest(result.Error);
        }
    }
}
