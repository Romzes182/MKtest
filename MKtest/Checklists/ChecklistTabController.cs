using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// Owns the checklist tab. A master library and separately saved individual
    /// runs have the same document format; no protocol service is modified.
    /// </summary>
    public sealed class ChecklistTabController : UserControl
    {
        private ChecklistDocument _document = new();
        private List<ChecklistCase> _masterCatalog = new();
        private readonly ChecklistRunStore _runs = new();
        private readonly DataGridView _grid = new();
        private readonly TextBox _search = new();
        private readonly ComboBox _group = new();
        private readonly ComboBox _platform = new();
        private readonly TextBox _title = new();
        private readonly TextBox _version = new();
        private readonly TextBox _environment = new();
        private readonly TextBox _tester = new();
        private readonly Label _summary = new();
        private readonly string _autosave = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MKtest", "Checklists", "current.json");
        private readonly string _catalogFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MKtest", "Checklists", "catalog.json");

        public ChecklistTabController()
        {
            Dock = DockStyle.Fill;
            InitializeLayout();
            RestoreAutosave();
        }

        private static Button Button(string text, EventHandler action)
        {
            var button = new Button
            {
                Text = text, AutoSize = true, Height = 29,
                Margin = new Padding(2, 2, 5, 2)
            };
            button.Click += action;
            return button;
        }

        private void InitializeLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1,
                RowCount = 4, Padding = new Padding(8)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));

            var metadata = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 8, RowCount = 1
            };
            for (int i = 0; i < 8; i++)
                metadata.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,
                    i is 1 or 3 or 5 or 7 ? 20 : 5));

            var labels = new[] { "Название:", "Версия:", "Устройство:", "Проверял:" };
            var fields = new[] { _title, _version, _environment, _tester };
            for (int i = 0; i < 4; i++)
            {
                metadata.Controls.Add(new Label
                {
                    Text = labels[i], Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, i * 2, 0);
                fields[i].Dock = DockStyle.Fill;
                metadata.Controls.Add(fields[i], i * 2 + 1, 0);
            }

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, AutoScroll = true,
                WrapContents = true, FlowDirection = FlowDirection.LeftToRight
            };
            toolbar.Controls.Add(Button("1. Новый проход", (_, _) => ResetToCatalog()));
            toolbar.Controls.Add(Button("Из выбранных", (_, _) => NewFromSelected()));
            toolbar.Controls.Add(Button("2. Печать списка", (_, _) => PrintList()));
            toolbar.Controls.Add(Button("Печать теста", (_, _) => PrintSingle()));
            toolbar.Controls.Add(Button("3. Отметки с бумаги", (_, _) => EnterPaperResults()));
            toolbar.Controls.Add(Button("История проходов", (_, _) => OpenRunHistory()));
            toolbar.Controls.Add(Button("Сохранить лист", (_, _) => SaveDocument()));
            toolbar.Controls.Add(Button("Открыть лист", (_, _) => OpenDocument()));
            toolbar.Controls.Add(Button("Импорт Excel", (_, _) => ImportExcel()));
            toolbar.Controls.Add(Button("Добавить проверку", (_, _) => AddCase()));
            toolbar.Controls.Add(Button("Изменить", (_, _) => EditCurrent()));
            toolbar.Controls.Add(Button("Удалить", (_, _) => RemoveCurrent()));

            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom, Height = 28,
                FlowDirection = FlowDirection.LeftToRight, WrapContents = false
            };
            filters.Controls.Add(new Label
            {
                Text = "Поиск:", AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(2, 6, 2, 0)
            });
            _search.Width = 270;
            _search.TextChanged += (_, _) => RefreshGrid();
            filters.Controls.Add(_search);
            filters.Controls.Add(new Label
            {
                Text = "Раздел:", AutoSize = true, Margin = new Padding(10, 6, 2, 0)
            });
            _group.DropDownStyle = ComboBoxStyle.DropDownList;
            _group.Width = 160;
            _group.SelectedIndexChanged += (_, _) => RefreshGrid();
            filters.Controls.Add(_group);
            filters.Controls.Add(new Label
            {
                Text = "ОС:", AutoSize = true, Margin = new Padding(10, 6, 2, 0)
            });
            _platform.DropDownStyle = ComboBoxStyle.DropDownList;
            _platform.Width = 130;
            _platform.Items.AddRange(new object[]
                { "Все", "Android", "Android 7", "Android 11", "Linux" });
            _platform.SelectedIndex = 0;
            _platform.SelectedIndexChanged += (_, _) => RefreshGrid();
            filters.Controls.Add(_platform);

            var bar = new Panel { Dock = DockStyle.Fill };
            bar.Controls.Add(filters);
            bar.Controls.Add(toolbar);
            toolbar.Height = 82;
            toolbar.Dock = DockStyle.Top;

            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.RowHeadersVisible = false;
            _grid.AutoGenerateColumns = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.RowTemplate.Height = 48;
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            _grid.EditMode = DataGridViewEditMode.EditOnEnter;
            _grid.DataError += (_, args) => args.ThrowException = false;
            _grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_grid.IsCurrentCellDirty)
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += (_, _) => Autosave();
            foreach (var field in new[] { _title, _version, _environment, _tester })
                field.Leave += (_, _) => Autosave();
            AddColumn(new DataGridViewCheckBoxColumn(),
                "Выбрать", nameof(ChecklistCase.Selected), 56);
            AddColumn(new DataGridViewTextBoxColumn(),
                "ID", nameof(ChecklistCase.Id), 85);
            AddColumn(new DataGridViewTextBoxColumn(),
                "Раздел", nameof(ChecklistCase.Category), 145);
            AddColumn(new DataGridViewTextBoxColumn(),
                "ОС", nameof(ChecklistCase.Platforms), 145);
            AddColumn(new DataGridViewTextBoxColumn(),
                "Сценарий / модуль", nameof(ChecklistCase.Module), 140);
            AddColumn(new DataGridViewTextBoxColumn(),
                "Проверка", nameof(ChecklistCase.Action), 320);
            AddColumn(new DataGridViewTextBoxColumn(),
                "Ожидаемый результат", nameof(ChecklistCase.Expected), 300);
            var status = new DataGridViewComboBoxColumn
            {
                HeaderText = "Результат", DataPropertyName = nameof(ChecklistCase.Result),
                Width = 120, FlatStyle = FlatStyle.Flat
            };
            status.Items.AddRange("Не проверено", "Есть", "Баг", "Нет", "Н/П");
            _grid.Columns.Add(status);
            AddColumn(new DataGridViewTextBoxColumn(),
                "№ бага / ссылка", nameof(ChecklistCase.Bug), 115);
            AddColumn(new DataGridViewTextBoxColumn(),
                "Комментарий", nameof(ChecklistCase.Notes), 200);

            _summary.Dock = DockStyle.Fill;
            _summary.TextAlign = ContentAlignment.MiddleLeft;
            _summary.ForeColor = Color.FromArgb(65, 85, 105);
            root.Controls.Add(metadata, 0, 0);
            root.Controls.Add(bar, 0, 1);
            root.Controls.Add(_grid, 0, 2);
            root.Controls.Add(_summary, 0, 3);
            Controls.Add(root);
        }

        private void AddColumn(DataGridViewColumn col, string header,
            string property, int width)
        {
            col.HeaderText = header;
            col.DataPropertyName = property;
            col.Width = width;
            _grid.Columns.Add(col);
        }

        private void SetDocument(ChecklistDocument document)
        {
            _document = document;
            _title.Text = document.Title;
            _version.Text = document.Version;
            _environment.Text = document.Environment;
            _tester.Text = document.Tester;
            var selection = _group.SelectedItem?.ToString();
            _group.Items.Clear();
            _group.Items.Add("Все");
            foreach (string category in _document.Cases.Select(x => x.Category)
                         .Distinct().OrderBy(x => x))
                _group.Items.Add(category);
            _group.SelectedItem = selection != null && _group.Items.Contains(selection)
                ? selection : "Все";
            RefreshGrid();
            Autosave();
        }

        private void ReadMetadata()
        {
            _document.Title = _title.Text.Trim();
            _document.Version = _version.Text.Trim();
            _document.Environment = _environment.Text.Trim();
            _document.Tester = _tester.Text.Trim();
        }

        private IEnumerable<ChecklistCase> VisibleCases()
        {
            string text = _search.Text.Trim();
            string category = _group.SelectedItem?.ToString() ?? "Все";
            string platform = _platform.SelectedItem?.ToString() ?? "Все";
            return _document.Cases.Where(x =>
                (category == "Все" || x.Category == category) &&
                (platform == "Все" || x.Platforms.Contains(platform,
                    StringComparison.OrdinalIgnoreCase)) &&
                (text.Length == 0 ||
                    new[] { x.Id, x.Category, x.Module, x.Action, x.Expected, x.Bug }
                        .Any(s => s.Contains(text, StringComparison.OrdinalIgnoreCase))));
        }

        private void RefreshGrid()
        {
            if (_grid.IsCurrentCellInEditMode)
                _grid.EndEdit();
            var shown = VisibleCases().ToList();
            _grid.DataSource = new BindingList<ChecklistCase>(shown);
            if (_document.Cases.Count == 0)
            {
                _summary.Text = "В проходе нет проверок. Создайте проход из каталога.";
                return;
            }
            int checkedCount = _document.Cases.Count(x => x.Result != "Не проверено");
            int errors = _document.Cases.Count(x => x.Result == "Баг");
            _summary.Text = $"Проход {_document.RunId[..Math.Min(8, _document.RunId.Length)]} • " +
                $"Отмечено: {checkedCount}/{_document.Cases.Count} • " +
                $"Багов: {errors} • Показано: {shown.Count}. " +
                "Печать → бумажная проверка → перенос галочек в программу.";
        }

        private ChecklistCase? CurrentCase() =>
            _grid.CurrentRow?.DataBoundItem as ChecklistCase;

        private void ImportExcel()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Книги Excel (*.xlsx)|*.xlsx",
                Multiselect = true,
                Title = "Импорт чек-листов Android / Linux / МЧС / Безбилетники"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var imported = ChecklistExcelImporter.Import(dialog.FileNames);
                if (imported.Count == 0)
                    throw new InvalidDataException("В книгах нет строк проверок.");
                _masterCatalog = imported.Select(x => x.NewBlankResult()).ToList();
                Write(_catalogFile, new ChecklistDocument { Cases = _masterCatalog });
                _search.Clear();
                _platform.SelectedIndex = 0;
                SetDocument(new ChecklistDocument
                {
                    Title = "Общий чек-лист медиакомплекса",
                    Cases = imported.Select(x => x.NewBlankResult()).ToList()
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка импорта",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetToCatalog()
        {
            if (_masterCatalog.Count == 0)
            {
                MessageBox.Show(this,
                    "Сначала загрузите общий список кнопкой «Импорт Excel».",
                    "Чек-листы");
                return;
            }
            if (MessageBox.Show(this,
                "Начать новый общий проход? Текущие отметки будут сброшены. " +
                "Сначала сохраните лист, если они нужны.",
                "Новый проход", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            Autosave();
            ReadMetadata();
            var newRun = _document.CreateSubset(_masterCatalog,
                "Общий чек-лист медиакомплекса");
            newRun.Date = DateTime.Today;
            SetDocument(newRun);
        }

        private void NewFromSelected()
        {
            _grid.EndEdit();
            var chosen = _document.Cases.Where(x => x.Selected).ToList();
            if (chosen.Count == 0 && CurrentCase() is ChecklistCase current)
                chosen.Add(current);
            if (chosen.Count == 0) return;
            Autosave();
            ReadMetadata();
            SetDocument(_document.CreateSubset(chosen,
                chosen.Count == 1 ? "Единичный тест-лист" : "Выборочный тест-лист"));
        }

        private void AddCase()
        {
            var item = new ChecklistCase
            {
                Id = "NEW-" + (_document.Cases.Count + 1),
                Category = "Новая проверка", Action = "Новая проверка"
            };
            if (!ChecklistEditDialog.Edit(this, item)) return;
            _document.Cases.Add(item);
            SetDocument(_document);
        }

        private void EditCurrent()
        {
            if (CurrentCase() is not ChecklistCase item) return;
            if (!ChecklistEditDialog.Edit(this, item)) return;
            SetDocument(_document);
        }

        private void RemoveCurrent()
        {
            if (CurrentCase() is not ChecklistCase item) return;
            if (MessageBox.Show(this, "Удалить проверку из текущего листа?",
                "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;
            _document.Cases.Remove(item);
            SetDocument(_document);
        }

        private void SaveDocument()
        {
            _grid.EndEdit();
            ReadMetadata();
            using var dialog = new SaveFileDialog
            {
                Filter = "Тест-лист MKtest (*.json)|*.json",
                FileName = "MKtest_Checklist_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".json",
                DefaultExt = "json", OverwritePrompt = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                Write(dialog.FileName, _document);
                Autosave();
                MessageBox.Show(this, "Тест-лист сохранён.", "MKtest");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка сохранения");
            }
        }

        private void OpenDocument()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Тест-лист MKtest (*.json)|*.json"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                SetDocument(Read(dialog.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка открытия");
            }
        }

        private void EnterPaperResults()
        {
            _grid.EndEdit();
            ReadMetadata();
            if (_document.Cases.Count == 0) return;
            using var dialog = new ChecklistPaperEntryDialog(_document, () =>
            {
                Autosave();
            });
            dialog.ShowDialog(this);
            RefreshGrid();
        }

        private void OpenRunHistory()
        {
            Autosave();
            var runs = _runs.List();
            if (runs.Count == 0)
            {
                MessageBox.Show(this, "Сохранённых проходов ещё нет.", "История");
                return;
            }

            using var window = new Form
            {
                Text = "История обходов MKtest",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(750, 440)
            };
            var list = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                DisplayMember = nameof(InspectionHistoryItem.Caption)
            };
            foreach (var run in runs)
            {
                int filled = run.Cases.Count(x => x.Result != "Не проверено");
                int bugs = run.Cases.Count(x => x.Result == "Баг");
                list.Items.Add(new InspectionHistoryItem(run.RunId,
                    $"{run.CreatedAt:dd.MM.yyyy HH:mm}  •  {run.Title}  •  " +
                    $"{run.Version}  •  {filled}/{run.Cases.Count}  •  Багов: {bugs}"));
            }

            var open = new Button
            {
                Text = "Открыть выбранный проход",
                Dock = DockStyle.Bottom, Height = 40
            };
            open.Click += (_, _) =>
            {
                if (list.SelectedItem is not InspectionHistoryItem selected)
                    return;
                SetDocument(_runs.Load(selected.RunId));
                window.DialogResult = DialogResult.OK;
                window.Close();
            };
            list.DoubleClick += (_, _) => open.PerformClick();
            window.Controls.Add(list);
            window.Controls.Add(open);
            window.ShowDialog(this);
        }

        private sealed record InspectionHistoryItem(string RunId, string Caption);

        private void PrintList()
        {
            _grid.EndEdit();
            ReadMetadata();
            Autosave(); // The printed walk-through now exists in run history.
            ChecklistPrinter.Preview(this, _document,
                VisibleCases().ToList(), false);
        }

        private void PrintSingle()
        {
            _grid.EndEdit();
            ReadMetadata();
            Autosave();
            if (CurrentCase() is ChecklistCase item)
                ChecklistPrinter.Preview(this, _document,
                    new List<ChecklistCase> { item }, true);
        }

        private void RestoreAutosave()
        {
            try
            {
                // A complete default catalog is available at first launch.
                // User-imported or extended catalog takes precedence on later runs.
                _masterCatalog = File.Exists(_catalogFile)
                    ? Read(_catalogFile).Cases.Select(x => x.NewBlankResult()).ToList()
                    : BuiltInChecklistCatalog.Load();

                if (File.Exists(_autosave))
                {
                    var existing = Read(_autosave);
                    SetDocument(existing.Cases.Count > 0 ? existing :
                        new ChecklistDocument
                        {
                            Cases = _masterCatalog.Select(x => x.NewBlankResult()).ToList()
                        });
                }
                else
                    SetDocument(new ChecklistDocument
                    {
                        Cases = _masterCatalog.Select(x => x.NewBlankResult()).ToList()
                    });
            }
            catch (Exception ex)
            {
                // Do not silently conceal an invalid built-in catalog.
                MessageBox.Show(this,
                    "Не удалось загрузить каталог проверок: " + ex.Message,
                    "Чек-листы MKtest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetDocument(new ChecklistDocument());
            }
        }

        private void Autosave()
        {
            try
            {
                ReadMetadata();
                Write(_autosave, _document);
                if (_document.Cases.Count > 0)
                    _runs.Save(_document);
            }
            catch
            {
                // Manual saving is always available.
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_grid.IsCurrentCellInEditMode)
                    _grid.EndEdit();
                Autosave();
            }
            base.Dispose(disposing);
        }

        private static ChecklistDocument Read(string file) =>
            JsonSerializer.Deserialize<ChecklistDocument>(File.ReadAllText(file))
            ?? throw new InvalidDataException("Некорректный тест-лист");

        private static void Write(string path, ChecklistDocument document)
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonSerializer.Serialize(document,
                new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
