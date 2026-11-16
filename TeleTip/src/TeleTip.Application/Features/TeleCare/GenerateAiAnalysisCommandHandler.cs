using MediatR;
using MyProject.Application.Abstractions.Ai;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Application.Features.TeleCare
{
    public sealed record GenerateAiAnalysisCommand(
        Guid PatientId,
        Guid? ConsultationId,
        AnalysisType Type) : IRequest<Result<Guid>>;

    internal sealed class GenerateAiAnalysisCommandHandler(
        ITeleCareRepository repository,
        IAiAnalysisEngine analysisEngine)
        : IRequestHandler<GenerateAiAnalysisCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(GenerateAiAnalysisCommand request, CancellationToken cancellationToken)
        {
            if (request.PatientId == Guid.Empty || !Enum.IsDefined(request.Type))
                return Result.Failure<Guid>(Error.Validation("Analysis", "Patient and a valid analysis type are required."));

            var measurements = await repository.GetRecentHealthMeasurementsAsync(request.PatientId, 100, cancellationToken);
            if (measurements.Count == 0)
                return Result.Failure<Guid>(Error.Validation("Measurements", "The patient has no health measurements to analyze."));

            var analysisDraft = await analysisEngine.AnalyzeAsync(request.Type, measurements, cancellationToken);
            if (analysisDraft.IsFailure)
                return Result.Failure<Guid>(analysisDraft.Error);

            var analysis = AiAnalysis.Create(
                request.PatientId,
                request.Type,
                analysisDraft.Value.Summary,
                analysisDraft.Value.RiskScore,
                request.ConsultationId);
            if (analysis.IsFailure)
                return Result.Failure<Guid>(analysis.Error);

            await repository.AddAsync(analysis.Value, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return Result.Success(analysis.Value.Id);
        }
    }
}
