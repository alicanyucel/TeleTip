namespace MyProject.Infrastructure.Ai
{
    public sealed class OllamaOptions
    {
        public const string SectionName = "Ollama";

        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string Model { get; set; } = "qwen2.5:7b";
        public int TimeoutSeconds { get; set; } = 120;
        public int MaxInputMeasurements { get; set; } = 100;
    }
}
