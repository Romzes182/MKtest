using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// Paper-first QA workflow: create a run, print its numbered checklist,
    /// walk around the equipment, and enter marks on a focused test card.
    /// All results belong to the run and never modify the reference catalog.
    /// </summary>
    public sealed class ChecklistTabController : UserControl
    {
        private readonly ChecklistRunStore _runs = new();
        private readonly string _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MKtest", "Checklists");
        private readonly string _currentFile;
        private readonly string _catalogFile;
        private List<ChecklistCase> _catalog = new();
        private ChecklistDocument _document = new();

        private readonly DataGridView _list = new();
        private readonly TextBox _search = new();
        private readonly TextBox _paperNumber = new();
        private readonly ComboBox _categoryFilter = new();
        private readonly ComboBox _resultFilter = new();
        private readonly TextBox _version = new();
        private readonly TextBox _device = new();
        private readonly TextBox _tester = new();
        private readonly Label _heading = new();
        private readonly Label _progress = new();
        private readonly Label _number = new();
        private readonly Label _context = new();
        private readonly Label _status = new();
        private readonly TextBox _action = new();
        private readonly TextBox _expected = new();
        private readonly TextBox _bug = new();
        private readonly TextBox _notes = new();
        private readonly Label _hint = new();
        private readonly CheckBox _autoNext = new()
        {
            Text = "Следующая после отметки",
            Checked = true, AutoSize = true, Margin = new Padding(6, 9, 0, 0)
        };
        private readonly System.Windows.Forms.Timer _saveTimer = new() { Interval = 600 };
        private bool _rendering;
        private int _currentIndex = -1;

        public ChecklistTabController()
        {
            _currentFile = Path.Combine(_dataDirectory, "current.json");
            _catalogFile = Path.Combine(_dataDirectory, "catalog.json");
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            BuildLayout();
            _saveTimer.Tick += (_, _) =>
            {
                _saveTimer.Stop();
                SaveEdits();
            };
            foreach (var input in new[] { _version, _device, _tester, _bug, _notes })
                input.TextChanged += (_, _) => ScheduleSave();
            Restore();
        }

        private static Button ActionButton(string text, EventHandler click, int width = 0)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = width == 0,
                Width = width == 0 ? 105 : width,
                Height = 33,
                Margin = new Padding(2, 2, 6, 2),
                FlatStyle = FlatStyle.System
            };
            b.Click += click;
            return b;
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4,
                Padding = new Padding(12), BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 93));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            Controls.Add(root);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2
            };
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            root.Controls.Add(top, 0, 0);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false, AutoScroll = true
            };
            actions.Controls.Add(ActionButton("Новый обход", (_, _) => NewRun(), 124));
            actions.Controls.Add(ActionButton("Печать бланка A4", (_, _) => PrintRun(), 154));
            actions.Controls.Add(ActionButton("История", (_, _) => OpenHistory(), 95));
            actions.Controls.Add(ActionButton("Отдельный тест", (_, _) => MakeSingleTest(), 132));
            actions.Controls.Add(ActionButton("Итоги и баги", (_, _) => ShowReport(), 125));

            var extra = ActionButton("Ещё ▾", (_, _) => ShowMoreMenu(), 75);
            extra.Name = "ChecklistMoreActions";
            actions.Controls.Add(extra);
            top.Controls.Add(actions, 0, 0);

            var metadata = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 1,
                Margin = Padding.Empty
            };
            float[] widths = { 9, 20, 11, 20, 12, 18, 10 };
            for (int i = 0; i < widths.Length; i++)
                metadata.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, widths[i]));
            AddMetadata(metadata, "Версия ПО:", _version, 0);
            AddMetadata(metadata, "Устройство:", _device, 2);
            AddMetadata(metadata, "Исполнитель:", _tester, 4);
            _heading.Dock = DockStyle.Fill;
            _heading.TextAlign = ContentAlignment.MiddleRight;
            _heading.ForeColor = Color.DimGray;
            metadata.Controls.Add(_heading, 6, 0);
            top.Controls.Add(metadata, 0, 1);

            var progressPanel = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(10, 5, 5, 5),
                Margin = new Padding(0, 0, 0, 7)
            };
            _progress.Dock = DockStyle.Fill;
            _progress.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _progress.TextAlign = ContentAlignment.MiddleLeft;
            progressPanel.Controls.Add(_progress);
            root.Controls.Add(progressPanel, 0, 1);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 7,
                Size = new Size(1020, 580),
                Panel1MinSize = 285,
                Panel2MinSize = 450
            };
            split.SplitterDistance = 352;
            split.Margin = Padding.Empty;
            root.Controls.Add(split, 0, 2);
            BuildList(split.Panel1);
            BuildDetail(split.Panel2);

            _hint.Dock = DockStyle.Fill;
            _hint.Text = "На бумаге и в программе одинаковые № и ID. Отметки сохраняются в текущем обходе.";
            _hint.ForeColor = Color.DimGray;
            _hint.Font = new Font("Segoe UI", 8);
            root.Controls.Add(_hint, 0, 3);
        }

        private static void AddMetadata(TableLayoutPanel panel, string name,
            TextBox field, int column)
        {
            panel.Controls.Add(new Label
            {
                Text = name, Dock = DockStyle.Fill, AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            }, column, 0);
            field.Dock = DockStyle.Fill;
            panel.Controls.Add(field, column + 1, 0);
        }

        private void BuildList(Control host)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(0, 0, 7, 0)
            };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            host.Controls.Add(panel);

            _search.Dock = DockStyle.Fill;
            _search.PlaceholderText = "Поиск по проверкам, ID, категории...";
            _search.TextChanged += (_, _) => RefreshList();
            panel.Controls.Add(_search, 0, 0);

            var filters = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1
            };
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            _categoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _resultFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _categoryFilter.Dock = DockStyle.Fill;
            _resultFilter.Dock = DockStyle.Fill;
            _categoryFilter.SelectedIndexChanged += (_, _) => RefreshList();
            _resultFilter.SelectedIndexChanged += (_, _) => RefreshList();
            _resultFilter.Items.AddRange(new object[]
                { "Все результаты", "Не проверено", "Есть", "Баг", "Нет", "Н/П" });
            _resultFilter.SelectedIndex = 0;
            filters.Controls.Add(_categoryFilter, 0, 0);
            filters.Controls.Add(_resultFilter, 1, 0);
            panel.Controls.Add(filters, 0, 1);

            _list.Dock = DockStyle.Fill;
            _list.AllowUserToAddRows = false;
            _list.AllowUserToDeleteRows = false;
            _list.AllowUserToResizeRows = false;
            _list.RowHeadersVisible = false;
            _list.BackgroundColor = Color.White;
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.ReadOnly = true;
            _list.MultiSelect = false;
            _list.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _list.AutoGenerateColumns = false;
            _list.ColumnHeadersHeight = 31;
            _list.RowTemplate.Height = 52;
            _list.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _list.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 238, 252);
            _list.DefaultCellStyle.SelectionForeColor = Color.Black;
            _list.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "№", Width = 55 });
            _list.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Проверка", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    MinimumWidth = 145
                });
            _list.Columns.Add(new DataGridViewTextBoxColumn
                { HeaderText = "Статус", Width = 87 });
            _list.SelectionChanged += (_, _) => ChooseSelected();
            panel.Controls.Add(_list, 0, 2);
        }

        private void BuildDetail(Control host)
        {
            var detail = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 10,
                Padding = new Padding(12, 2, 0, 0)
            };
            foreach (var size in new[] { 40, 27, 111, 28, 95, 44, 37, 29, 0, 40 })
            {
                detail.RowStyles.Add(size == 0
                    ? new RowStyle(SizeType.Percent, 100)
                    : new RowStyle(SizeType.Absolute, size));
            }
            host.Controls.Add(detail);

            var jump = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false
            };
            jump.Controls.Add(new Label
            {
                Text = "№ с распечатки:", AutoSize = true,
                Margin = new Padding(0, 9, 8, 0)
            });
            _paperNumber.Width = 112;
            _paperNumber.Margin = new Padding(0, 6, 4, 0);
            jump.Controls.Add(_paperNumber);
            jump.Controls.Add(ActionButton("Перейти", (_, _) => FindPrintedNumber(), 85));
            jump.Controls.Add(_number);
            _number.AutoSize = true;
            _number.Margin = new Padding(6, 9, 0, 0);
            _paperNumber.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    FindPrintedNumber();
                }
            };
            detail.Controls.Add(jump, 0, 0);

            _context.Dock = DockStyle.Fill;
            _context.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _context.AutoEllipsis = true;
            detail.Controls.Add(_context, 0, 1);

            _action.Dock = DockStyle.Fill;
            _action.Multiline = true;
            _action.ReadOnly = true;
            _action.BackColor = Color.FromArgb(248, 250, 252);
            _action.BorderStyle = BorderStyle.FixedSingle;
            _action.ScrollBars = ScrollBars.Vertical;
            _action.Font = new Font("Segoe UI", 11);
            detail.Controls.Add(_action, 0, 2);
            detail.Controls.Add(new Label
            {
                Text = "Ожидаемый результат",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            }, 0, 3);

            _expected.Dock = DockStyle.Fill;
            _expected.Multiline = true;
            _expected.ReadOnly = true;
            _expected.ScrollBars = ScrollBars.Vertical;
            _expected.BackColor = Color.FromArgb(248, 250, 252);
            _expected.BorderStyle = BorderStyle.FixedSingle;
            detail.Controls.Add(_expected, 0, 4);

            var statuses = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false,
                AutoScroll = true
            };
            statuses.Controls.Add(ActionButton("✓ Работает",
                (_, _) => MarkCurrent("Есть"), 110));
            statuses.Controls.Add(ActionButton("! Баг",
                (_, _) => MarkCurrent("Баг"), 87));
            statuses.Controls.Add(ActionButton("— Нет",
                (_, _) => MarkCurrent("Нет"), 83));
            statuses.Controls.Add(ActionButton("Н/П",
                (_, _) => MarkCurrent("Н/П"), 65));
            statuses.Controls.Add(_autoNext);
            detail.Controls.Add(statuses, 0, 5);

            _status.Dock = DockStyle.Fill;
            _status.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            detail.Controls.Add(_status, 0, 6);

            var bugRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2,
                RowCount = 1
            };
            bugRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            bugRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bugRow.Controls.Add(new Label
            {
                Text = "Номер / ссылка бага:", Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            _bug.Dock = DockStyle.Fill;
            bugRow.Controls.Add(_bug, 1, 0);
            detail.Controls.Add(bugRow, 0, 7);

            var notesPanel = new Panel { Dock = DockStyle.Fill };
            notesPanel.Controls.Add(_notes);
            _notes.Dock = DockStyle.Fill;
            _notes.Multiline = true;
            _notes.ScrollBars = ScrollBars.Vertical;
            _notes.PlaceholderText = "Замечания с бумаги и фактический результат...";
            detail.Controls.Add(notesPanel, 0, 8);

            var navigation = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight, AutoScroll = true
            };
            navigation.Controls.Add(ActionButton("← Назад",
                (_, _) => MoveToAdjacent(-1), 92));
            navigation.Controls.Add(ActionButton("Сохранить →",
                (_, _) => MoveToAdjacent(1), 111));
            navigation.Controls.Add(ActionButton("Следующий без отметки",
                (_, _) => FindUnchecked(), 185));
            navigation.Controls.Add(ActionButton("Печать этого теста",
                (_, _) => PrintSingle(), 156));
            detail.Controls.Add(navigation, 0, 9);
        }

        private void Restore()
        {
            try
            {
                _catalog = File.Exists(_catalogFile)
                    ? Read(_catalogFile).Cases.Select(x => x.NewBlankResult()).ToList()
                    : BuiltInChecklistCatalog.Load();

                ChecklistDocument start = File.Exists(_currentFile)
                    ? Read(_currentFile)
                    : new ChecklistDocument
                    {
                        Cases = _catalog.Select(x => x.NewBlankResult()).ToList()
                    };
                if (start.Cases.Count == 0)
                    start.Cases = _catalog.Select(x => x.NewBlankResult()).ToList();
                SetRun(start, persist: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    "Не удалось прочитать чек-листы: " + ex.Message,
                    "MKtest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetRun(new ChecklistDocument(), persist: false);
            }
        }

        private void SetRun(ChecklistDocument run, bool persist = true)
        {
            _rendering = true;
            _saveTimer.Stop();
            _document = run;
            _currentIndex = -1;
            _version.Text = run.Version;
            _device.Text = run.Environment;
            _tester.Text = run.Tester;
            _heading.Text = $"Обход {ShortId(run.RunId)}";
            _search.Clear();
            _paperNumber.Clear();
            _categoryFilter.Items.Clear();
            _categoryFilter.Items.Add("Все разделы");
            foreach (string section in _document.Cases.Select(x => x.Category)
                         .Distinct().OrderBy(x => x))
                _categoryFilter.Items.Add(section);
            _categoryFilter.SelectedIndex = 0;
            _resultFilter.SelectedIndex = 0;
            _rendering = false;
            RefreshList();
            if (persist) Persist();
        }

        private static string ShortId(string? runId) =>
            string.IsNullOrEmpty(runId)
                ? "--------" : runId[..Math.Min(8, runId.Length)];

        private void RefreshList()
        {
            if (_rendering) return;
            if (_saveTimer.Enabled) SaveEdits();
            int keep = _currentIndex;
            string needle = _search.Text.Trim();
            string category = _categoryFilter.SelectedItem?.ToString() ?? "Все разделы";
            string result = _resultFilter.SelectedItem?.ToString() ?? "Все результаты";

            _rendering = true;
            try
            {
                _list.Rows.Clear();
                for (int i = 0; i < _document.Cases.Count; i++)
                {
                    ChecklistCase item = _document.Cases[i];
                    if (category != "Все разделы" && item.Category != category)
                        continue;
                    if (result != "Все результаты" && item.Result != result)
                        continue;
                    if (needle.Length > 0 && !(item.Id.Contains(needle,
                            StringComparison.OrdinalIgnoreCase) ||
                        item.Category.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                        item.Module.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                        item.Action.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                        item.Expected.Contains(needle, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    string description = string.IsNullOrWhiteSpace(item.Module)
                        ? item.Action : item.Module + "\n" + item.Action;
                    int rowIndex = _list.Rows.Add((i + 1).ToString("000"),
                        description, DisplayStatus(item.Result));
                    _list.Rows[rowIndex].Tag = i;
                    _list.Rows[rowIndex].Cells[2].Style.ForeColor =
                        item.Result == "Баг" ? Color.Firebrick :
                        item.Result == "Есть" ? Color.ForestGreen :
                        item.Result == "Нет" ? Color.DarkOrange : Color.DimGray;
                }
                _list.ClearSelection();
                var match = _list.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => r.Tag is int ix && ix == keep);
                if (match == null && _list.Rows.Count > 0)
                    match = _list.Rows[0];

                if (match != null)
                {
                    match.Selected = true;
                    _list.CurrentCell = match.Cells[1];
                    _currentIndex = (int)match.Tag!;
                }
                else _currentIndex = -1;
                RenderCurrent();
                UpdateProgress();
            }
            finally { _rendering = false; }
        }

        private static string DisplayStatus(string value) => value switch
        {
            "Есть" => "✓ Есть",
            "Баг" => "! Баг",
            "Нет" => "— Нет",
            "Н/П" => "Н/П",
            _ => "○ Ожидает"
        };

        private void ChooseSelected()
        {
            if (_rendering) return;
            if (_list.CurrentRow?.Tag is not int index ||
                index == _currentIndex) return;
            SaveEdits();
            _currentIndex = index;
            RenderCurrent();
        }

        private void RenderCurrent()
        {
            _rendering = true;
            try
            {
                if (_currentIndex < 0 || _currentIndex >= _document.Cases.Count)
                {
                    _number.Text = "Нет выбранной проверки";
                    _context.Text = "";
                    _action.Text = "";
                    _expected.Text = "";
                    _bug.Text = "";
                    _notes.Text = "";
                    _status.Text = "";
                    return;
                }
                ChecklistCase item = _document.Cases[_currentIndex];
                _number.Text = $"Пункт {_currentIndex + 1:000} · {item.Id}";
                _context.Text = $"{item.Category} · {item.Platforms}";
                _action.Text = item.Action;
                if (!string.IsNullOrWhiteSpace(item.Module))
                    _action.Text = $"Сценарий: {item.Module}\r\n\r\n{item.Action}";
                _expected.Text = item.Expected;
                _bug.Text = item.Bug;
                _notes.Text = item.Notes;
                _status.Text = "Результат: " + DisplayStatus(item.Result);
                _status.ForeColor = item.Result == "Баг" ? Color.Firebrick :
                    item.Result == "Есть" ? Color.ForestGreen : Color.DimGray;
            }
            finally { _rendering = false; }
        }

        private void UpdateProgress()
        {
            int all = _document.Cases.Count;
            int passed = _document.Cases.Count(x => x.Result == "Есть");
            int bugs = _document.Cases.Count(x => x.Result == "Баг");
            int absent = _document.Cases.Count(x => x.Result == "Нет");
            int na = _document.Cases.Count(x => x.Result == "Н/П");
            _progress.Text = $"Всего {all}   |   Отмечено {passed + bugs + absent + na}" +
                $"   |   Работает {passed}   |   Баги {bugs}" +
                $"   |   Нет {absent}   |   Н/П {na}";
            _heading.Text = $"Обход {ShortId(_document.RunId)}";
        }

        private void MarkCurrent(string result)
        {
            if (_currentIndex < 0) return;
            SaveEdits();
            _document.Cases[_currentIndex].Result = result;
            Persist();
            UpdateProgress();

            if (result == "Баг")
            {
                RefreshRowStatus();
                _status.Text = "Результат: ! Баг";
                _status.ForeColor = Color.Firebrick;
                _bug.Focus();
            }
            else if (_autoNext.Checked)
                Move(1);
            else
                RefreshRowStatus();
        }

        private void RefreshRowStatus()
        {
            if (_currentIndex >= 0)
            {
                ChecklistCase c = _document.Cases[_currentIndex];
                _status.Text = "Результат: " + DisplayStatus(c.Result);
            }
            RefreshList();
        }

        private void MoveToAdjacent(int offset)
        {
            SaveEdits();
            if (_document.Cases.Count == 0) return;
            int target = Math.Clamp(_currentIndex + offset,
                0, _document.Cases.Count - 1);
            SelectByIndex(target);
        }

        private void FindUnchecked()
        {
            SaveEdits();
            int index = _document.Cases.FindIndex(
                Math.Max(0, _currentIndex + 1),
                x => x.Result == "Не проверено");
            if (index < 0)
                index = _document.Cases.FindIndex(
                    x => x.Result == "Не проверено");
            if (index >= 0) SelectByIndex(index);
            else MessageBox.Show(this, "Все проверки отмечены.",
                "Обход завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FindPrintedNumber()
        {
            string value = _paperNumber.Text.Trim();
            int index = int.TryParse(value, out int number)
                ? number - 1
                : _document.Cases.FindIndex(x =>
                    x.Id.Equals(value, StringComparison.OrdinalIgnoreCase) ||
                    x.Id.Split('/').Any(p => p.Trim().Equals(value,
                        StringComparison.OrdinalIgnoreCase)));
            if (index < 0 || index >= _document.Cases.Count)
            {
                MessageBox.Show(this,
                    "Номер с распечатки или ID не найден в этом обходе.",
                    "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectByIndex(index);
        }

        private void SelectByIndex(int index)
        {
            if (index < 0 || index >= _document.Cases.Count) return;
            SaveEdits();
            // Reveal a line even if the user had filtered it out.
            _rendering = true;
            _categoryFilter.SelectedIndex = 0;
            _resultFilter.SelectedIndex = 0;
            _search.Clear();
            _currentIndex = index;
            _rendering = false;
            RefreshList();
            _paperNumber.SelectAll();
        }

        private void ScheduleSave()
        {
            if (_rendering) return;
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void SaveEdits()
        {
            if (_rendering) return;
            _saveTimer.Stop();
            if (_currentIndex >= 0 && _currentIndex < _document.Cases.Count)
            {
                var item = _document.Cases[_currentIndex];
                item.Bug = _bug.Text;
                item.Notes = _notes.Text;
            }
            _document.Version = _version.Text.Trim();
            _document.Environment = _device.Text.Trim();
            _document.Tester = _tester.Text.Trim();
            Persist();
        }

        private void Persist()
        {
            if (_document.Cases.Count == 0) return;
            try
            {
                Directory.CreateDirectory(_dataDirectory);
                _runs.Save(_document);
                Write(_currentFile, _document);
                _hint.Text = "Сохранено. Номера проверок совпадают с распечаткой.";
                _hint.ForeColor = Color.DimGray;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                       or ArgumentException)
            {
                _hint.Text = "ОШИБКА сохранения: " + ex.Message;
                _hint.ForeColor = Color.Firebrick;
            }
        }

        private void NewRun()
        {
            if (_catalog.Count == 0)
            {
                MessageBox.Show(this, "Каталог проверок пуст. Загрузите Excel через «Ещё».",
                    "Чек-листы", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SaveEdits();
            using var dialog = new Form
            {
                Text = "Новый обход медиакомплекса",
                Size = new Size(480, 245),
                MinimumSize = new Size(460, 245),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(18),
                FlowDirection = FlowDirection.TopDown, WrapContents = false
            };
            panel.Controls.Add(new Label
            {
                Text = "Выбери набор проверок для распечатки:",
                AutoSize = true
            });
            var choice = new ComboBox
            {
                Width = 418, DropDownStyle = ComboBoxStyle.DropDownList
            };
            choice.Items.AddRange(new object[] {
                "Весь медиакомплекс", "Android (все)", "Android 7",
                "Android 11", "Linux", "МЧС", "Безбилетники"
            });
            choice.SelectedIndex = 0;
            panel.Controls.Add(choice);
            panel.Controls.Add(new Label
            {
                Text = "Создаётся новый обход с пустыми отметками.\n" +
                       "Прошлые результаты сохранятся в истории.",
                Width = 415, Height = 43
            });
            var ok = ActionButton("Создать обход", (_, _) => { }, 145);
            ok.DialogResult = DialogResult.OK;
            panel.Controls.Add(ok);
            dialog.AcceptButton = ok;
            dialog.Controls.Add(panel);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            string kind = choice.SelectedItem?.ToString() ?? "Весь медиакомплекс";
            var cases = _catalog.Where(x => MatchesKind(x, kind))
                .Select(x => x.NewBlankResult()).ToList();
            var fresh = _document.CreateSubset(cases,
                kind == "Весь медиакомплекс" ? "Общий чек-лист медиакомплекса"
                    : "Проверка: " + kind);
            fresh.Environment = kind == "Весь медиакомплекс"
                ? _document.Environment : kind;
            SetRun(fresh);
        }

        private static bool MatchesKind(ChecklistCase item, string kind) => kind switch
        {
            "Android (все)" => item.Platforms.Contains("Android",
                StringComparison.OrdinalIgnoreCase),
            "Android 7" => item.Platforms.Contains("Android 7",
                StringComparison.OrdinalIgnoreCase) ||
                item.Platforms.Split(',').Any(x => x.Trim() == "Android"),
            "Android 11" => item.Platforms.Contains("Android 11",
                StringComparison.OrdinalIgnoreCase) ||
                item.Platforms.Split(',').Any(x => x.Trim() == "Android"),
            "Linux" => item.Platforms.Contains("Linux",
                StringComparison.OrdinalIgnoreCase),
            "МЧС" => item.Category.Contains("МЧС",
                StringComparison.OrdinalIgnoreCase),
            "Безбилетники" => item.Category.Contains("Безбилет",
                StringComparison.OrdinalIgnoreCase),
            _ => true
        };

        private void MakeSingleTest()
        {
            if (_currentIndex < 0) return;
            SaveEdits();
            var one = _document.CreateSubset(new[] { _document.Cases[_currentIndex] },
                "Отдельный тест: " + _document.Cases[_currentIndex].Id);
            SetRun(one);
        }

        private void PrintRun()
        {
            if (_document.Cases.Count == 0) return;
            SaveEdits();
            // Print the exact stored run, never only the current search results.
            ChecklistPrinter.Preview(this, _document, _document.Cases, false);
        }

        private void PrintSingle()
        {
            if (_currentIndex < 0) return;
            SaveEdits();
            ChecklistPrinter.Preview(this, _document,
                new[] { _document.Cases[_currentIndex] }, true);
        }

        private void OpenHistory()
        {
            SaveEdits();
            var runs = _runs.List();
            using var history = new Form
            {
                Text = "История проверок",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(830, 520),
                MinimumSize = new Size(650, 390)
            };
            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            foreach (ChecklistDocument run in runs)
            {
                int done = run.Cases.Count(x => x.Result != "Не проверено");
                int defects = run.Cases.Count(x => x.Result == "Баг");
                list.Items.Add(new RunChoice(run.RunId,
                    $"{run.CreatedAt:dd.MM.yyyy HH:mm}   {run.Title}   " +
                    $"{run.Version}   {done}/{run.Cases.Count}   Багов: {defects}"));
            }
            list.DisplayMember = nameof(RunChoice.Title);
            var open = ActionButton("Открыть обход", (_, _) =>
            {
                if (list.SelectedItem is not RunChoice selection) return;
                try
                {
                    SetRun(_runs.Load(selection.RunId));
                    history.DialogResult = DialogResult.OK;
                    history.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(history, ex.Message,
                        "Ошибка открытия", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }, 160);
            open.Dock = DockStyle.Bottom;
            open.Height = 42;
            list.DoubleClick += (_, _) => open.PerformClick();
            history.Controls.Add(list);
            history.Controls.Add(open);
            history.ShowDialog(this);
        }

        private sealed record RunChoice(string RunId, string Title);

        private void ShowMoreMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Импорт Excel в каталог", null, (_, _) => ImportExcel());
            menu.Items.Add("Добавить проверку в каталог", null,
                (_, _) => AddToCatalog());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Сохранить обход в JSON", null, (_, _) => ExportJson());
            menu.Items.Add("Открыть обход из JSON", null, (_, _) => ImportJson());
            menu.Closed += (_, _) => menu.Dispose();
            menu.Show(Cursor.Position);
        }

        private void ImportExcel()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Выберите оригинальные чек-листы Excel",
                Filter = "Книги Excel (*.xlsx)|*.xlsx", Multiselect = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var data = ChecklistExcelImporter.Import(dialog.FileNames);
                if (data.Count == 0)
                    throw new InvalidDataException("В файлах нет проверок.");
                var decision = MessageBox.Show(this,
                    $"Импортировано {data.Count} проверок. Заменить каталог? " +
                    "Сохранённые обходы не изменятся.",
                    "Обновление каталога", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (decision != DialogResult.Yes) return;

                SaveEdits();
                _catalog = data.Select(x => x.NewBlankResult()).ToList();
                Write(_catalogFile, new ChecklistDocument { Cases = _catalog });
                MessageBox.Show(this,
                    "Каталог обновлён. Нажмите «Новый обход» для использования.",
                    "Импорт завершён");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка импорта",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddToCatalog()
        {
            var item = new ChecklistCase
            {
                Id = "USER-" + (_catalog.Count + 1).ToString("000"),
                Category = "Дополнительные проверки"
            };
            if (!ChecklistEditDialog.Edit(this, item)) return;
            _catalog.Add(item.NewBlankResult());
            Write(_catalogFile, new ChecklistDocument { Cases = _catalog });
            MessageBox.Show(this,
                "Проверка добавлена в каталог. Её можно включить в новый обход.",
                "Готово");
        }

        private void ExportJson()
        {
            SaveEdits();
            using var dialog = new SaveFileDialog
            {
                Filter = "Тест-лист MKtest (*.json)|*.json",
                FileName = "MKtest_" + ShortId(_document.RunId) + ".json",
                DefaultExt = "json", OverwritePrompt = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try { Write(dialog.FileName, _document); }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка экспорта");
            }
        }

        private void ImportJson()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Тест-лист MKtest (*.json)|*.json"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                SaveEdits();
                SetRun(Read(dialog.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка загрузки");
            }
        }

        private void ShowReport()
        {
            SaveEdits();
            int passed = _document.Cases.Count(c => c.Result == "Есть");
            int failed = _document.Cases.Count(c => c.Result == "Баг");
            int missing = _document.Cases.Count(c => c.Result == "Нет");
            int skipped = _document.Cases.Count(c => c.Result == "Н/П");
            var report = new StringBuilder();
            report.AppendLine($"MKtest — результат обхода {_document.RunId}");
            report.AppendLine(_document.Title);
            report.AppendLine($"Дата: {_document.Date:dd.MM.yyyy}");
            report.AppendLine($"Версия: {_document.Version}  Устройство: {_document.Environment}");
            report.AppendLine($"Исполнитель: {_document.Tester}");
            report.AppendLine($"Всего: {_document.Cases.Count}; Работает: {passed}; " +
                $"Баг: {failed}; Нет: {missing}; Н/П: {skipped}; " +
                $"Не проверено: {_document.Cases.Count - passed - failed - missing - skipped}");
            report.AppendLine();
            report.AppendLine("БАГИ И ЗАМЕЧАНИЯ");
            for (int i = 0; i < _document.Cases.Count; i++)
            {
                ChecklistCase item = _document.Cases[i];
                if (item.Result != "Баг" && item.Result != "Нет" &&
                    string.IsNullOrWhiteSpace(item.Notes)) continue;
                report.AppendLine($"[{i + 1:000} / {item.Id}] {item.Result} — {item.Action}");
                if (!string.IsNullOrWhiteSpace(item.Bug))
                    report.AppendLine("Номер бага: " + item.Bug);
                if (!string.IsNullOrWhiteSpace(item.Notes))
                    report.AppendLine("Замечание: " + item.Notes);
                report.AppendLine();
            }

            using var window = new Form
            {
                Text = "Итоги проверки",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(790, 630)
            };
            var body = new TextBox
            {
                Text = report.ToString(), Dock = DockStyle.Fill,
                ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10)
            };
            var save = ActionButton("Сохранить отчёт TXT", (_, _) =>
            {
                using var dlg = new SaveFileDialog
                {
                    Filter = "Текстовый отчёт (*.txt)|*.txt",
                    FileName = "MKtest_Report_" + ShortId(_document.RunId) + ".txt"
                };
                if (dlg.ShowDialog(window) != DialogResult.OK) return;
                File.WriteAllText(dlg.FileName, body.Text, Encoding.UTF8);
            }, 190);
            save.Dock = DockStyle.Bottom;
            save.Height = 41;
            window.Controls.Add(body);
            window.Controls.Add(save);
            window.ShowDialog(this);
        }

        private static ChecklistDocument Read(string path) =>
            JsonSerializer.Deserialize<ChecklistDocument>(
                File.ReadAllText(path, Encoding.UTF8))
            ?? throw new InvalidDataException("Пустой или повреждённый чек-лист.");

        private static void Write(string path, ChecklistDocument document)
        {
            string? folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);
            string temp = path + ".tmp";
            File.WriteAllText(temp,
                JsonSerializer.Serialize(document,
                    new JsonSerializerOptions { WriteIndented = true }),
                Encoding.UTF8);
            File.Move(temp, path, true);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.D1))
            {
                MarkCurrent("Есть"); return true;
            }
            if (keyData == (Keys.Control | Keys.D2))
            {
                MarkCurrent("Баг"); return true;
            }
            if (keyData == (Keys.Control | Keys.D3))
            {
                MarkCurrent("Нет"); return true;
            }
            if (keyData == (Keys.Control | Keys.D4))
            {
                MarkCurrent("Н/П"); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _saveTimer.Stop();
                SaveEdits();
                _saveTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
