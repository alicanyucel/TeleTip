using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Ai;

namespace MyProject.Tests.Infrastructure;

public sealed class OllamaAiAnalysisEngineTests
{
    [Fact]
    public async Task AnalyzeAsync_ValidOllamaResponse_ReturnsAnalysisDraft()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            request.RequestUri!.AbsolutePath.Should().Be("/api/generate");
            return Task.FromResult(JsonResponse("{\"response\":\"{\\\"summary\\\":\\\"Review the recent measurements.\\\",\\\"riskScore\\\":0.72}\"}"));
        });
        var engine = CreateEngine(handler);
        var measurement = HealthMeasurement.Create(
            Guid.NewGuid(), MeasurementType.HeartRate, 72, "bpm", DateTime.UtcNow).Value;

        var result = await engine.AnalyzeAsync(AnalysisType.General, [measurement], CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Summary.Should().Be("Review the recent measurements.");
        result.Value.RiskScore.Should().Be(0.72m);
    }

    [Fact]
    public async Task AnalyzeAsync_RiskScoreOutsideRange_ReturnsInvalidResponse()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(
            JsonResponse("{\"response\":\"{\\\"summary\\\":\\\"Summary\\\",\\\"riskScore\\\":1.2}\"}")));
        var engine = CreateEngine(handler);
        var measurement = HealthMeasurement.Create(
            Guid.NewGuid(), MeasurementType.HeartRate, 72, "bpm", DateTime.UtcNow).Value;

        var result = await engine.AnalyzeAsync(AnalysisType.General, [measurement], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AiAnalysis.InvalidResponse");
    }

    [Fact]
    public async Task AnalyzeAsync_OllamaReturnsError_ReturnsUnavailable()
    {
        var handler = new StubHttpMessageHandler((_, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        var engine = CreateEngine(handler);
        var measurement = HealthMeasurement.Create(
            Guid.NewGuid(), MeasurementType.HeartRate, 72, "bpm", DateTime.UtcNow).Value;

        var result = await engine.AnalyzeAsync(AnalysisType.General, [measurement], CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AiAnalysis.Unavailable");
    }

    private static OllamaAiAnalysisEngine CreateEngine(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") },
            Options.Create(new OllamaOptions()));

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => send(request, cancellationToken);
    }
}
