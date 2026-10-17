using System;
using System.Threading;
using System.Threading.Tasks;

namespace MKtest.Services.HermesPassenger
{
    /// <summary>Executes the original process-stop command, without scheduling it.</summary>
    public sealed class HermesKillProcessTask
    {
        private readonly IHermesSshSession _session;
        private readonly Action<string> _log;

        public HermesKillProcessTask(IHermesSshSession session, Action<string> log)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            if (await _session.ExecuteAsync("killall pfsens", token).ConfigureAwait(false))
                _log("Гермес: выполнен killall pfsens");
        }
    }
}
