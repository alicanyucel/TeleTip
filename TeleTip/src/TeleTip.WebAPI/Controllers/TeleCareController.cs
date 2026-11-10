using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Application.Features.TeleCare;
using MyProject.Domain.Abstractions;

namespace MyProject.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/telecare")]
public sealed class TeleCareController(ISender sender) : ControllerBase
{
    [HttpGet("patients")]
    public Task<IActionResult> GetPatients(CancellationToken ct) => SendList(new GetPatientsQuery(), ct);
    [HttpGet("patients/{id:guid}")]
    public Task<IActionResult> GetPatient(Guid id, CancellationToken ct) => SendOne(new GetPatientByIdQuery(id), ct);
    [HttpPost("patients")]
    public Task<IActionResult> CreatePatient(CreatePatientCommand command, CancellationToken ct) => Create(command, nameof(GetPatient), ct);

    [HttpGet("doctors")]
    public Task<IActionResult> GetDoctors(CancellationToken ct) => SendList(new GetDoctorsQuery(), ct);
    [HttpGet("doctors/{id:guid}")]
    public Task<IActionResult> GetDoctor(Guid id, CancellationToken ct) => SendOne(new GetDoctorByIdQuery(id), ct);
    [HttpPost("doctors")]
    public Task<IActionResult> CreateDoctor(CreateDoctorCommand command, CancellationToken ct) => Create(command, nameof(GetDoctor), ct);

    [HttpGet("consultations")]
    public Task<IActionResult> GetConsultations(CancellationToken ct) => SendList(new GetConsultationsQuery(), ct);
    [HttpGet("consultations/{id:guid}")]
    public Task<IActionResult> GetConsultation(Guid id, CancellationToken ct) => SendOne(new GetConsultationByIdQuery(id), ct);
    [HttpPost("consultations")]
    public Task<IActionResult> CreateConsultation(CreateConsultationCommand command, CancellationToken ct) => Create(command, nameof(GetConsultation), ct);

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
    public Task<IActionResult> GetChatMessages(CancellationToken ct) => SendList(new GetChatMessagesQuery(), ct);
    [HttpGet("chat-messages/{id:guid}")]
    public Task<IActionResult> GetChatMessage(Guid id, CancellationToken ct) => SendOne(new GetChatMessageByIdQuery(id), ct);
    [HttpPost("chat-messages")]
    public Task<IActionResult> CreateChatMessage(CreateChatMessageCommand command, CancellationToken ct) => Create(command, nameof(GetChatMessage), ct);

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
