using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Arranges the original right-hand protocol panels in a scrollable column.
    /// Expanded log boxes never overlap neighboring protocol controls.
    /// </summary>
    public sealed class RightPanelFlowController : IDisposable
    {
        private readonly FlowLayoutPanel _flow;
        private readonly Panel _host;
        private int _lastPanelWidth = -1;
        private readonly List<Panel> _panels = new();

        public RightPanelFlowController(Panel host, params Panel[] panels)
        {
            ArgumentNullException.ThrowIfNull(host);
            _host = host;
            _flow = new BufferedFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(3, 3, 3, 3)
            };

            host.Controls.Add(_flow);
            _flow.BringToFront();

            foreach (Panel panel in panels)
            {
                if (panel == null) continue;
                _panels.Add(panel);
                _flow.Controls.Add(panel);
                panel.Margin = new Padding(3, 3, 3, 10);
            }

            _flow.Resize += OnResize;
            RefreshLayout();
        }

        private void OnResize(object? sender, EventArgs e) => RefreshLayout();

        public void RefreshLayout()
        {
            // Use the parent viewport width, not FlowLayoutPanel.ClientSize:
            // the latter changes when the vertical scrollbar appears/disappears.
            // Reserve its width even when currently hidden, avoiding resize flicker.
            int horizontalMargins = _panels.Count > 0
                ? _panels[0].Margin.Horizontal
                : 6;
            int availableWidth = Math.Max(1,
                _host.ClientSize.Width - _flow.Padding.Horizontal -
                horizontalMargins - SystemInformation.VerticalScrollBarWidth - 4);

            if (_lastPanelWidth == availableWidth)
                return;

            _lastPanelWidth = availableWidth;
            _flow.SuspendLayout();
            try
            {
                foreach (var panel in _panels)
                    if (panel.Width != availableWidth)
                        panel.Width = availableWidth;
            }
            finally
            {
                _flow.ResumeLayout(true);
            }
        }

        public void Dispose() => _flow.Resize -= OnResize;
    }
}
