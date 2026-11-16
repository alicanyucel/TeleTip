using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;

namespace MyProject.Application.Abstractions.Ai
{
    public sealed record AiAnalysisDraft(string Summary, decimal? RiskScore);

    public interface IAiAnalysisEngine
    {
        Task<Result<AiAnalysisDraft>> AnalyzeAsync(
            AnalysisType analysisType,
            IReadOnlyCollection<HealthMeasurement> measurements,
            CancellationToken cancellationToken);
    }
}
