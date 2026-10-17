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
        private readonly List<Panel> _panels = new();

        public RightPanelFlowController(Panel host, params Panel[] panels)
        {
            ArgumentNullException.ThrowIfNull(host);
            _flow = new FlowLayoutPanel
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
            int availableWidth = Math.Max(250,
                _flow.ClientSize.Width - _flow.Padding.Horizontal - 26);

            _flow.SuspendLayout();
            try
            {
                foreach (var panel in _panels)
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
