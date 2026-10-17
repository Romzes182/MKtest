using System;
using System.Threading.Tasks;

namespace MKtest.Services.Demoscripts
{
    /// <summary>Настройки одного запуска, не связанные с состоянием следующего.</summary>
    public sealed record DemoRunOptions(
        bool ManualMode,
        Func<Task>? WaitForNext,
        Action<string> ReportProgress);
}
