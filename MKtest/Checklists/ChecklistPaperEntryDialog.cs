using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// The operator enters hand-written marks using the same stable number/ID
    /// printed on paper. Changes are persisted immediately, not only on exit.
    /// </summary>
    public sealed class ChecklistPaperEntryDialog : Form
    {
        private readonly ChecklistDocument _document;
        private readonly Action _save;
        private readonly TextBox _search = new() { Width = 225 };
        private readonly Label _position = new() { AutoSize = true };
        private readonly Label _question = new()
        {
            Dock = DockStyle.Fill, Font = new Font("Segoe UI", 12, FontStyle.Bold)
        };
        private readonly Label _expected = new() { Dock = DockStyle.Fill };
        private readonly TextBox _bug = new() { Dock = DockStyle.Fill };
        private readonly TextBox _notes = new()
        {
            Dock = DockStyle.Fill, Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };
        private readonly Label _progress = new() { AutoSize = true };
        private int _index;
        private bool _changing;

        public ChecklistPaperEntryDialog(ChecklistDocument document, Action save)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _save = save ?? throw new ArgumentNullException(nameof(save));
            Text = "Внести отметки с бумажного чек-листа — MKtest";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(690, 480);
            Size = new Size(880, 605);
            KeyPreview = true;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1, RowCount = 8,
                Padding = new Padding(12)
            };
            foreach (int height in new[] { 42, 34, 118, 105, 46, 34, 100, 44 })
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            Controls.Add(layout);

            var finder = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight
            };
            finder.Controls.Add(new Label
            {
                Text = "№ на листе / ID:", AutoSize = true,
                Margin = new Padding(1, 7, 8, 1)
            });
            finder.Controls.Add(_search);
            var go = MakeButton("Найти", (_, _) => FindCase());
            finder.Controls.Add(go);
            _search.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    FindCase();
                    e.SuppressKeyPress = true;
                }
            };
            finder.Controls.Add(_position);
            layout.Controls.Add(finder, 0, 0);

            layout.Controls.Add(_progress, 0, 1);
            _question.Padding = new Padding(9);
            _question.BorderStyle = BorderStyle.FixedSingle;
            layout.Controls.Add(_question, 0, 2);
            _expected.Padding = new Padding(9);
            _expected.BorderStyle = BorderStyle.FixedSingle;
            layout.Controls.Add(_expected, 0, 3);

            var results = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false
            };
            results.Controls.Add(MakeButton("✓ Есть / работает",
                (_, _) => Mark("Есть", true)));
            results.Controls.Add(MakeButton("! Баг",
                (_, _) => Mark("Баг", false)));
            results.Controls.Add(MakeButton("— Нет",
                (_, _) => Mark("Нет", true)));
            results.Controls.Add(MakeButton("Н/П",
                (_, _) => Mark("Н/П", true)));
            results.Controls.Add(MakeButton("Сбросить",
                (_, _) => Mark("Не проверено", false)));
            layout.Controls.Add(results, 0, 4);

            layout.Controls.Add(new Label
            {
                Text = "Номер бага или ссылка:", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 5);
            var notesPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2
            };
            notesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
            notesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            notesPanel.Controls.Add(_bug, 0, 0);
            notesPanel.Controls.Add(new Label
            {
                Text = "Замечания с бумажного листа:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 1, 0);
            notesPanel.Controls.Add(_notes, 0, 1);
            notesPanel.SetColumnSpan(_notes, 2);
            layout.Controls.Add(notesPanel, 0, 6);

            var navigation = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false
            };
            navigation.Controls.Add(MakeButton("← Предыдущий",
                (_, _) => Navigate(-1)));
            navigation.Controls.Add(MakeButton("Сохранить и далее →",
                (_, _) => { SaveCurrent(); Navigate(1); }));
            navigation.Controls.Add(MakeButton("Следующий без отметки",
                (_, _) => NextUnchecked()));
            navigation.Controls.Add(MakeButton("Закрыть",
                (_, _) => { SaveCurrent(); Close(); }));
            layout.Controls.Add(navigation, 0, 7);

            _bug.TextChanged += (_, _) => SaveCurrent();
            _notes.TextChanged += (_, _) => SaveCurrent();
            FormClosing += (_, _) => SaveCurrent();
            ShowCase();
        }

        private static Button MakeButton(string title, EventHandler click)
        {
            var button = new Button
            {
                Text = title, AutoSize = true, Height = 32,
                Margin = new Padding(3, 1, 8, 1)
            };
            button.Click += click;
            return button;
        }

        private void FindCase()
        {
            string search = _search.Text.Trim();
            if (search.Length == 0) return;
            int found;
            if (int.TryParse(search, out int number))
                found = number - 1;
            else
                found = _document.Cases.FindIndex(c =>
                    c.Id.Equals(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Id.Split('/').Any(x => x.Trim().Equals(search,
                        StringComparison.OrdinalIgnoreCase)));

            if (found < 0 || found >= _document.Cases.Count)
            {
                MessageBox.Show(this, "Такого номера или ID нет в текущем проходе.",
                    "Не найдено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SaveCurrent();
            _index = found;
            ShowCase();
        }

        private void Mark(string result, bool advance)
        {
            if (_document.Cases.Count == 0) return;
            _document.Cases[_index].Result = result;
            SaveCurrent();
            if (advance && _index + 1 < _document.Cases.Count)
                Navigate(1);
            else
                ShowProgress();
            if (result == "Баг") _bug.Focus();
        }

        private void Navigate(int step)
        {
            SaveCurrent();
            _index = Math.Clamp(_index + step, 0,
                Math.Max(0, _document.Cases.Count - 1));
            ShowCase();
        }

        private void NextUnchecked()
        {
            SaveCurrent();
            int next = _document.Cases.FindIndex(_index + 1,
                x => x.Result == "Не проверено");
            if (next < 0)
                next = _document.Cases.FindIndex(x => x.Result == "Не проверено");
            if (next >= 0)
            {
                _index = next;
                ShowCase();
            }
        }

        private void ShowCase()
        {
            _changing = true;
            try
            {
                if (_document.Cases.Count == 0)
                {
                    _question.Text = "В текущем проходе нет проверок.";
                    _expected.Text = "";
                    return;
                }
                ChecklistCase item = _document.Cases[_index];
                _position.Text = $"  Пункт {_index + 1} из {_document.Cases.Count}, ID: {item.Id}";
                _question.Text = $"{item.Category} • {item.Platforms}\n\n{item.Action}";
                _expected.Text = "Ожидаемый результат:\n" + item.Expected +
                    "\n\nТекущая отметка: " + item.Result;
                _bug.Text = item.Bug;
                _notes.Text = item.Notes;
                ShowProgress();
            }
            finally { _changing = false; }
        }

        private void ShowProgress()
        {
            int done = _document.Cases.Count(x => x.Result != "Не проверено");
            int bugs = _document.Cases.Count(x => x.Result == "Баг");
            _progress.Text = $"Проход: {_document.RunId[..8]}  •  " +
                $"Отмечено: {done}/{_document.Cases.Count}  •  Багов: {bugs}";
        }

        private void SaveCurrent()
        {
            if (_changing || _document.Cases.Count == 0) return;
            var item = _document.Cases[_index];
            item.Bug = _bug.Text;
            item.Notes = _notes.Text;
            _save();
        }
    }
}
