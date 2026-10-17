using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Arranges expandable blocks in one column with an explicit scroll range.
    /// Unlike a FlowLayoutPanel, the viewport scrolls ONE content panel, not every
    /// nested WinForms control. This avoids clipped final blocks and scroll smearing.
    /// </summary>
    public sealed class ScrollablePanelStackController : IDisposable
    {
        private const int SidePadding = 3;
        private const int TopPadding = 3;
        private const int BottomSpacing = 10;
        private readonly Control _host;
        private readonly BufferedScrollablePanel _viewport;
        private readonly Panel _content;
        private readonly List<Panel> _panels = new();
        private bool _layoutInProgress;
        private bool _disposed;

        public ScrollablePanelStackController(Control host, params Panel[] panels)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            ArgumentNullException.ThrowIfNull(panels);

            _viewport = new BufferedScrollablePanel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                TabStop = true
            };
            _content = new Panel
            {
                Location = Point.Empty,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            // Reparent existing blocks; original controls and their event handlers
            // remain exactly the same and Designer.cs need not be changed.
            _viewport.Controls.Add(_content);
            _host.Controls.Add(_viewport);
            _viewport.BringToFront();

            foreach (var panel in panels)
            {
                if (panel == null) continue;
                _panels.Add(panel);
                _content.Controls.Add(panel);
                panel.SizeChanged += PanelSizeChanged;
            }

            _viewport.Resize += ViewportResized;
            RefreshLayout();
        }

        private void PanelSizeChanged(object? sender, EventArgs e)
        {
            if (!_layoutInProgress)
                RefreshLayout();
        }

        private void ViewportResized(object? sender, EventArgs e) => RefreshLayout();

        public void RefreshLayout()
        {
            if (_layoutInProgress || _disposed || _viewport.IsDisposed)
                return;

            _layoutInProgress = true;
            _viewport.SuspendLayout();
            _content.SuspendLayout();

            try
            {
                // Reserve the scrollbar width even when not yet visible.
                // Using viewport.Width avoids the fluctuating ClientSize during scroll.
                int contentWidth = Math.Max(1,
                    _viewport.Width - SystemInformation.VerticalScrollBarWidth - 4);
                int blockWidth = Math.Max(1, contentWidth - SidePadding * 2);

                if (_content.Width != contentWidth)
                    _content.Width = contentWidth;

                int y = TopPadding;
                foreach (var panel in _panels)
                {
                    if (panel.Left != SidePadding || panel.Top != y ||
                        panel.Width != blockWidth)
                    {
                        panel.SetBounds(SidePadding, y, blockWidth, panel.Height);
                    }

                    y += panel.Height + BottomSpacing;
                }

                int neededHeight = y + TopPadding;
                if (_content.Height != neededHeight)
                    _content.Height = neededHeight;

                // Explicit bottom edge allows scrolling all the way to МЧС.
                _viewport.AutoScrollMinSize = new Size(0, neededHeight);
            }
            finally
            {
                _content.ResumeLayout(true);
                _viewport.ResumeLayout(true);
                _layoutInProgress = false;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _viewport.Resize -= ViewportResized;
            foreach (var panel in _panels)
                panel.SizeChanged -= PanelSizeChanged;
            _panels.Clear();
        }
    }
}
