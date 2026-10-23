using MKtest.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.UI.Logging
{
    /// <summary>
    /// Owns one independent, collapsible log area inside each protocol block.
    /// The owning protocol panel remains controlled by CollapsiblePanelController.
    /// </summary>
    public sealed class BlockLogPanelRegistry : IDisposable
    {
        private const int ExpandedLogHeight = 140;
        private const int CollapsedLogHeight = 32;
        private readonly LogManager _generalLog;
        private readonly List<LogManager> _sections = new();

        /// <summary>
        /// Changes the remembered expanded height of the parent protocol block.
        /// </summary>
        public event Action<Panel, int>? LogHeightChanged;

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
                Height = ExpandedLogHeight,
                Padding = new Padding(5, 4, 5, 4)
            };
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 24
            };
            var caption = new Label
            {
                Text = "Журнал: " + blockName,
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var collapse = new Button
            {
                Text = "Свернуть",
                Dock = DockStyle.Right,
                Width = 77,
                Height = 23
            };
            var toBottom = new Button
            {
                Text = "Вниз ↓",
                Dock = DockStyle.Right,
                Width = 63,
                Height = 23
            };
            var clear = new Button
            {
                Text = "Очистить",
                Dock = DockStyle.Right,
                Width = 75,
                Height = 23
            };
            // Compact icon keeps the name of the protocol block readable.
            var save = new Button
            {
                Text = "💾",
                Dock = DockStyle.Right,
                Width = 33,
                Height = 23,
                AccessibleName = "Сохранить полный журнал блока"
            };
            var saveHint = new ToolTip();
            saveHint.SetToolTip(save, "Сохранить полный журнал блока в файл");
            logContainer.Disposed += (_, _) => saveHint.Dispose();
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
            header.Controls.Add(save);
            header.Controls.Add(toBottom);
            header.Controls.Add(collapse);

            logContainer.Controls.Add(logText);
            logContainer.Controls.Add(header);

            var scopedLog = _generalLog.CreateScope(logText, blockName);
            clear.Click += (_, _) => scopedLog.ClearLog();
            save.Click += async (_, _) =>
                await LogExportDialog.SaveAsync(container.FindForm(), scopedLog, blockName);
            toBottom.Click += (_, _) => scopedLog.ResumeLogScroll();

            bool collapsed = false;
            collapse.Click += (_, _) =>
            {
                int formerHeight = logContainer.Height;
                collapsed = !collapsed;
                int desiredHeight = collapsed ? CollapsedLogHeight : ExpandedLogHeight;

                logContainer.SuspendLayout();
                try
                {
                    logText.Visible = !collapsed;
                    logContainer.Height = desiredHeight;
                    collapse.Text = collapsed ? "Развернуть" : "Свернуть";
                }
                finally
                {
                    logContainer.ResumeLayout(true);
                }

                // The entire protocol block must shrink/grow by the same amount.
                // The layout controller also remembers it when the block is closed.
                LogHeightChanged?.Invoke(container, desiredHeight - formerHeight);
            };

            // Reserve the log height before CollapsiblePanelController registers
            // the original expanded size of the protocol block.
            container.Height += ExpandedLogHeight;
            contentPanel.Controls.Add(logContainer);

            _sections.Add(scopedLog);
            return scopedLog;
        }

        public void Dispose()
        {
            foreach (var section in _sections)
                section.Dispose();
            _sections.Clear();
            LogHeightChanged = null;
        }
    }
}
