using System;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Backward-compatible right-column facade.
    /// Both columns now share the same explicit scrolling layout.
    /// </summary>
    public sealed class RightPanelFlowController : IDisposable
    {
        private readonly ScrollablePanelStackController _stack;

        public RightPanelFlowController(Panel host, params Panel[] panels)
        {
            _stack = new ScrollablePanelStackController(host, panels);
        }

        public void RefreshLayout() => _stack.RefreshLayout();

        public void Dispose() => _stack.Dispose();
    }
}

