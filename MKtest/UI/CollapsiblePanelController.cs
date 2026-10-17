using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Управляет раскрытием панелей, не зная о назначении конкретных разделов формы.
    /// Новые разделы добавляются регистрацией, а не изменением этого класса.
    /// </summary>
    public sealed class CollapsiblePanelController : IDisposable
    {
        private readonly Action _layoutChanged;
        private readonly List<PanelEntry> _entries = new();
        private readonly Dictionary<Control, PanelEntry> _headers = new();
        private bool _disposed;

        public CollapsiblePanelController(Action layoutChanged)
        {
            _layoutChanged = layoutChanged ?? throw new ArgumentNullException(nameof(layoutChanged));
        }

        public void Register(Panel container, Panel header, Label titleLabel, Panel content, string title)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(container);
            ArgumentNullException.ThrowIfNull(header);
            ArgumentNullException.ThrowIfNull(titleLabel);
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(title);

            var entry = new PanelEntry(container, header, titleLabel, content, title, container.Height);
            _entries.Add(entry);
            _headers.Add(header, entry);
            _headers.Add(titleLabel, entry);

            header.Click += HeaderClicked;
            titleLabel.Click += HeaderClicked;
        }

        public void CollapseAll()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            foreach (var entry in _entries)
                SetExpanded(entry, false);

            _layoutChanged();
        }

        private void HeaderClicked(object? sender, EventArgs e)
        {
            if (sender is not Control control || !_headers.TryGetValue(control, out var entry))
                return;

            bool expand = !entry.Content.Visible;

            // Do not paint a partially resized TextBox or scrollbar.
            // All nested size changes become visible in one frame.
            using (new PanelExpansionLayoutScope(entry.Container, entry.Content))
            {
                SetExpanded(entry, expand);
                _layoutChanged();
            }
        }

        private static void SetExpanded(PanelEntry entry, bool expanded)
        {
            entry.Content.Visible = expanded;
            entry.Container.Height = expanded ? entry.ExpandedHeight : entry.Header.Height;
            entry.TitleLabel.Text = entry.Title + (expanded ? " ▼" : " ▶");
        }

        public void Dispose()
        {
            if (_disposed) return;
            foreach (var entry in _entries)
            {
                entry.Header.Click -= HeaderClicked;
                entry.TitleLabel.Click -= HeaderClicked;
            }

            _entries.Clear();
            _headers.Clear();
            _disposed = true;
        }

        private sealed record PanelEntry(
            Panel Container,
            Panel Header,
            Label TitleLabel,
            Panel Content,
            string Title,
            int ExpandedHeight);
    }
}
