using MediatR;
using MyProject.Application.Abstractions.Messaging;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Application.Features.TeleCare;

public sealed record GetPatientsQuery : IQuery<IReadOnlyList<Patient>>;
public sealed record GetPatientByIdQuery(Guid Id) : IQuery<Patient>;
public sealed record GetDoctorsQuery : IQuery<IReadOnlyList<Doctor>>;
public sealed record GetDoctorByIdQuery(Guid Id) : IQuery<Doctor>;
public sealed record GetConsultationsQuery : IQuery<IReadOnlyList<TelemedicineConsultation>>;
public sealed record GetConsultationByIdQuery(Guid Id) : IQuery<TelemedicineConsultation>;
public sealed record GetIoTDevicesQuery : IQuery<IReadOnlyList<IoTDevice>>;
public sealed record GetIoTDeviceByIdQuery(Guid Id) : IQuery<IoTDevice>;
public sealed record GetHealthMeasurementsQuery : IQuery<IReadOnlyList<HealthMeasurement>>;
public sealed record GetHealthMeasurementByIdQuery(Guid Id) : IQuery<HealthMeasurement>;
public sealed record GetPrescriptionsQuery : IQuery<IReadOnlyList<Prescription>>;
public sealed record GetPrescriptionByIdQuery(Guid Id) : IQuery<Prescription>;
public sealed record GetMedicalReportsQuery : IQuery<IReadOnlyList<MedicalReport>>;
public sealed record GetMedicalReportByIdQuery(Guid Id) : IQuery<MedicalReport>;
public sealed record GetAiAnalysesQuery : IQuery<IReadOnlyList<AiAnalysis>>;
public sealed record GetAiAnalysisByIdQuery(Guid Id) : IQuery<AiAnalysis>;
public sealed record GetNotificationsQuery : IQuery<IReadOnlyList<Notification>>;
public sealed record GetNotificationByIdQuery(Guid Id) : IQuery<Notification>;
public sealed record GetChatMessagesQuery : IQuery<IReadOnlyList<ChatMessage>>;
public sealed record GetChatMessageByIdQuery(Guid Id) : IQuery<ChatMessage>;

internal abstract class GetAllEntityQueryHandler<TQuery, TEntity>(ITeleCareRepository repository)
    : IRequestHandler<TQuery, Result<IReadOnlyList<TEntity>>>
    where TQuery : IQuery<IReadOnlyList<TEntity>>
    where TEntity : Entity
{
    public async Task<Result<IReadOnlyList<TEntity>>> Handle(TQuery request, CancellationToken cancellationToken)
        => Result.Success(await repository.GetAllAsync<TEntity>(cancellationToken));
}

internal abstract class GetEntityByIdQueryHandler<TQuery, TEntity>(ITeleCareRepository repository)
    : IRequestHandler<TQuery, Result<TEntity>>
    where TQuery : IQuery<TEntity>
    where TEntity : Entity
{
    public async Task<Result<TEntity>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync<TEntity>(GetId(request), cancellationToken);
        return entity is null
            ? Result.Failure<TEntity>(Error.NotFound(typeof(TEntity).Name))
            : Result.Success(entity);
    }

    protected abstract Guid GetId(TQuery request);
}

internal sealed class GetPatientsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetPatientsQuery, Patient>(repository);
internal sealed class GetDoctorsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetDoctorsQuery, Doctor>(repository);
internal sealed class GetConsultationsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetConsultationsQuery, TelemedicineConsultation>(repository);
internal sealed class GetIoTDevicesQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetIoTDevicesQuery, IoTDevice>(repository);
internal sealed class GetHealthMeasurementsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetHealthMeasurementsQuery, HealthMeasurement>(repository);
internal sealed class GetPrescriptionsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetPrescriptionsQuery, Prescription>(repository);
internal sealed class GetMedicalReportsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetMedicalReportsQuery, MedicalReport>(repository);
internal sealed class GetAiAnalysesQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetAiAnalysesQuery, AiAnalysis>(repository);
internal sealed class GetNotificationsQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetNotificationsQuery, Notification>(repository);
internal sealed class GetChatMessagesQueryHandler(ITeleCareRepository repository)
    : GetAllEntityQueryHandler<GetChatMessagesQuery, ChatMessage>(repository);

internal sealed class GetPatientByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetPatientByIdQuery, Patient>(repository)
{
    protected override Guid GetId(GetPatientByIdQuery request) => request.Id;
}

internal sealed class GetDoctorByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetDoctorByIdQuery, Doctor>(repository)
{
    protected override Guid GetId(GetDoctorByIdQuery request) => request.Id;
}

internal sealed class GetConsultationByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetConsultationByIdQuery, TelemedicineConsultation>(repository)
{
    protected override Guid GetId(GetConsultationByIdQuery request) => request.Id;
}

internal sealed class GetIoTDeviceByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetIoTDeviceByIdQuery, IoTDevice>(repository)
{
    protected override Guid GetId(GetIoTDeviceByIdQuery request) => request.Id;
}

internal sealed class GetHealthMeasurementByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetHealthMeasurementByIdQuery, HealthMeasurement>(repository)
{
    protected override Guid GetId(GetHealthMeasurementByIdQuery request) => request.Id;
}

internal sealed class GetPrescriptionByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetPrescriptionByIdQuery, Prescription>(repository)
{
    protected override Guid GetId(GetPrescriptionByIdQuery request) => request.Id;
}

internal sealed class GetMedicalReportByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetMedicalReportByIdQuery, MedicalReport>(repository)
{
    protected override Guid GetId(GetMedicalReportByIdQuery request) => request.Id;
}

internal sealed class GetAiAnalysisByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetAiAnalysisByIdQuery, AiAnalysis>(repository)
{
    protected override Guid GetId(GetAiAnalysisByIdQuery request) => request.Id;
}

internal sealed class GetNotificationByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetNotificationByIdQuery, Notification>(repository)
{
    protected override Guid GetId(GetNotificationByIdQuery request) => request.Id;
}

internal sealed class GetChatMessageByIdQueryHandler(ITeleCareRepository repository)
    : GetEntityByIdQueryHandler<GetChatMessageByIdQuery, ChatMessage>(repository)
{
    protected override Guid GetId(GetChatMessageByIdQuery request) => request.Id;
}
