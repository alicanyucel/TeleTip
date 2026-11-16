using FluentAssertions;
using MyProject.Application.Abstractions.Ai;
using MyProject.Application.Abstractions.Persistence;
using MyProject.Application.Features.TeleCare;
using MyProject.Domain.Abstractions;
using MyProject.Domain.Entities;
using NSubstitute;

namespace MyProject.Tests.Application.TeleCare;

public sealed class GenerateAiAnalysisCommandHandlerTests
{
    private readonly ITeleCareRepository _repository = Substitute.For<ITeleCareRepository>();
    private readonly IAiAnalysisEngine _engine = Substitute.For<IAiAnalysisEngine>();

    [Fact]
    public async Task Handle_WithMeasurementsAndValidDraft_PersistsAnalysis()
    {
        var patientId = Guid.NewGuid();
        var measurement = HealthMeasurement.Create(
            patientId, MeasurementType.HeartRate, 72, "bpm", DateTime.UtcNow).Value;
        _repository.GetRecentHealthMeasurementsAsync(patientId, 100, Arg.Any<CancellationToken>())
            .Returns([measurement]);
        _engine.AnalyzeAsync(AnalysisType.General, Arg.Any<IReadOnlyCollection<HealthMeasurement>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new AiAnalysisDraft("Measurement summary", 0.4m)));
        var handler = new GenerateAiAnalysisCommandHandler(_repository, _engine);

        var result = await handler.Handle(
            new GenerateAiAnalysisCommand(patientId, null, AnalysisType.General), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _repository.Received(1).AddAsync(Arg.Is<AiAnalysis>(analysis =>
            analysis.PatientId == patientId && analysis.AnalysisResult == "Measurement summary" && analysis.RiskScore == 0.4m),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutMeasurements_DoesNotCallEngineOrPersist()
    {
        var patientId = Guid.NewGuid();
        _repository.GetRecentHealthMeasurementsAsync(patientId, 100, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<HealthMeasurement>());
        var handler = new GenerateAiAnalysisCommandHandler(_repository, _engine);

        var result = await handler.Handle(
            new GenerateAiAnalysisCommand(patientId, null, AnalysisType.General), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Validation.Measurements");
        await _engine.DidNotReceive().AnalyzeAsync(
            Arg.Any<AnalysisType>(), Arg.Any<IReadOnlyCollection<HealthMeasurement>>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
