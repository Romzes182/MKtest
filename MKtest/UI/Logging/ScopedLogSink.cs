using System;

namespace MKtest.UI.Logging
{
    /// <summary>
    /// Sends entries to the application-wide log and to one section only.
    /// Disposing a section does not dispose the shared application sink.
    /// </summary>
    public sealed class ScopedLogSink : IUiLogSink
    {
        private readonly IUiLogSink _shared;
        private readonly IUiLogSink _local;

        public ScopedLogSink(IUiLogSink shared, IUiLogSink local)
        {
            _shared = shared ?? throw new ArgumentNullException(nameof(shared));
            _local = local ?? throw new ArgumentNullException(nameof(local));
        }

        public void Append(string formattedLine)
        {
            _shared.Append(formattedLine);
            _local.Append(formattedLine);
        }

        // Clear only the section; the global log keeps the full context.
        public void Clear() => _local.Clear();

        public void Dispose() => _local.Dispose();
    }
}
