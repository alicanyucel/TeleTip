using MyProject.Domain.Abstractions;

namespace MyProject.Domain.Entities;

public sealed class AiAnalysis : Entity
{
    private AiAnalysis() { }

    public Guid PatientId { get; private set; }
    public Guid? ConsultationId { get; private set; }
    public AnalysisType Type { get; private set; }
    public string AnalysisResult { get; private set; } = default!;
    public decimal? RiskScore { get; private set; }
    public bool RequiresDoctorReview { get; private set; }
    public DateTime AnalyzedAt { get; private set; }

    public static Result<AiAnalysis> Create(
        Guid patientId,
        AnalysisType type,
        string result,
        decimal? riskScore = null,
        Guid? consultationId = null)
    {
        if (patientId == Guid.Empty)
            return Result.Failure<AiAnalysis>(Error.Validation(nameof(PatientId), "Patient is required."));

        if (string.IsNullOrWhiteSpace(result))
            return Result.Failure<AiAnalysis>(Error.Validation(nameof(AnalysisResult), "Analysis result cannot be empty."));

        if (riskScore is < 0 or > 1)
            return Result.Failure<AiAnalysis>(Error.Validation(nameof(RiskScore), "Risk score must be between 0 and 1."));

        return Result.Success(new AiAnalysis
        {
            PatientId = patientId,
            ConsultationId = consultationId,
            Type = type,
            AnalysisResult = result.Trim(),
            RiskScore = riskScore,
            RequiresDoctorReview = riskScore >= 0.7m,
            AnalyzedAt = DateTime.UtcNow
        });
    }
}

public enum AnalysisType
{
    General,
    RiskDetection,
    DoctorAlert
}
