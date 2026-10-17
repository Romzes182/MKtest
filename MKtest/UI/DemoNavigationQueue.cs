using System.Threading.Channels;
using System.Threading.Tasks;
using MKtest.Services.Demoscripts;

namespace MKtest.UI
{
    /// <summary>Stores clicks until the manual runner is ready for the next action.</summary>
    public sealed class DemoNavigationQueue
    {
        private readonly Channel<ManualStepDirection> _channel =
            Channel.CreateBounded<ManualStepDirection>(new BoundedChannelOptions(16)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.DropOldest
            });

        public Task<ManualStepDirection> NextAsync() =>
            _channel.Reader.ReadAsync().AsTask();

        public void Enqueue(ManualStepDirection direction) =>
            _channel.Writer.TryWrite(direction);
    }
}
