using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Applies a collapse/expand change as a single layout and paint operation.
    /// Suppresses intermediate Win32 paints only while the controls are resized.
    /// </summary>
    public sealed class PanelExpansionLayoutScope : IDisposable
    {
        private const uint WmSetRedraw = 0x000B;
        private readonly Control _scrollHost;
        private readonly Control _container;
        private readonly Control _content;
        private readonly bool _redrawSuspended;
        private bool _disposed;

        public PanelExpansionLayoutScope(Control container, Control content)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _scrollHost = FindScrollHost(container);

            _redrawSuspended =
                _scrollHost.IsHandleCreated && _scrollHost.Visible && !_scrollHost.IsDisposed;

            if (_redrawSuspended)
                SendMessage(_scrollHost.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);

            _scrollHost.SuspendLayout();
            _container.SuspendLayout();
            _content.SuspendLayout();
        }

        private static Control FindScrollHost(Control container)
        {
            for (Control? parent = container.Parent; parent != null; parent = parent.Parent)
            {
                if (parent is ScrollableControl scrollable && scrollable.AutoScroll)
                    return parent;
            }

            return container.Parent ?? container;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _content.ResumeLayout(true);
                _container.ResumeLayout(true);
                _scrollHost.ResumeLayout(true);
            }
            finally
            {
                if (_redrawSuspended && !_scrollHost.IsDisposed &&
                    _scrollHost.IsHandleCreated)
                {
                    SendMessage(_scrollHost.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                    _scrollHost.Invalidate(true);
                    _scrollHost.Update();
                }
            }
        }

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam);
    }
}
