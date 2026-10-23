using System;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>Настройки одного запуска, не связанные с состоянием следующего.</summary>
    public sealed record DemoRunOptions(
        bool ManualMode,
        Func<Task>? WaitForNext,
        Action<string> ReportProgress)
    {
        /// <summary>Optional reversible manual mode with Next/Previous actions.</summary>
        public Func<Task<ManualStepDirection>>? WaitForDirection { get; init; }
    }
}
