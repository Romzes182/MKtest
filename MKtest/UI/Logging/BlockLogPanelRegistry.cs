using MKtest.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.UI.Logging
{
    /// <summary>
    /// Creates a distinct, auto-scrolling log view inside each existing WinForms block.
    /// No edits to the Designer-generated layout are needed.
    /// </summary>
    public sealed class BlockLogPanelRegistry : IDisposable
    {
        private const int LogPanelHeight = 140;
        private readonly LogManager _generalLog;
        private readonly List<LogManager> _sections = new();

        public BlockLogPanelRegistry(LogManager generalLog)
        {
            _generalLog = generalLog ?? throw new ArgumentNullException(nameof(generalLog));
        }

        public LogManager Add(string blockName, Panel contentPanel, Panel container)
        {
            ArgumentNullException.ThrowIfNull(contentPanel);
            ArgumentNullException.ThrowIfNull(container);

            var logContainer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = LogPanelHeight,
                Padding = new Padding(5, 4, 5, 4)
            };
            var header = new Panel { Dock = DockStyle.Top, Height = 24 };
            var caption = new Label
            {
                Text = "Журнал: " + blockName,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var clear = new Button
            {
                Text = "Очистить",
                Dock = DockStyle.Right,
                Width = 82,
                Height = 23
            };
            var logText = new TextBox
            {
                Name = $"blockLog_{_sections.Count}",
                Multiline = true,
                WordWrap = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill
            };

            header.Controls.Add(caption);
            header.Controls.Add(clear);
            logContainer.Controls.Add(logText);
            logContainer.Controls.Add(header);

            var scopedLog = _generalLog.CreateScope(logText);
            clear.Click += (_, _) => scopedLog.ClearLog();

            // Keep all controls in their existing coordinates. The extra height
            // is reserved solely for the log docked below them.
            container.Height += LogPanelHeight;
            contentPanel.Controls.Add(logContainer);

            _sections.Add(scopedLog);
            return scopedLog;
        }

        public void Dispose()
        {
            foreach (var section in _sections)
                section.Dispose();
            _sections.Clear();
        }
    }
}
