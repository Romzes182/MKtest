using System;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Suspends only managed layout while a block changes height.
    /// WM_SETREDRAW is deliberately not used: suppressing native child paints
    /// caused stale pixels to appear when the scroll viewport was moved.
    /// </summary>
    public sealed class PanelExpansionLayoutScope : IDisposable
    {
        private readonly Control _container;
        private readonly Control _content;
        private bool _disposed;

        public PanelExpansionLayoutScope(Control container, Control content)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _content = content ?? throw new ArgumentNullException(nameof(content));

            _container.SuspendLayout();
            _content.SuspendLayout();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _content.ResumeLayout(true);
            _container.ResumeLayout(true);
        }
    }
}
