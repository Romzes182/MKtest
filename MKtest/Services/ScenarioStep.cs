namespace MKtest.Services
{
    public enum StepExecutionMode
    {
        Once,
        PerLoop
    }

    public class ScenarioStep
    {
        public string SourceFile { get; init; } = string.Empty;
        public string TargetFile { get; init; } = string.Empty;
        public int DelayMs { get; init; } = 0;
        public StepExecutionMode ExecutionMode { get; init; } = StepExecutionMode.PerLoop;
        public string Description { get; init; } = string.Empty;
    }
}