using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// Catalog changes are made against a working copy. Save applies them
    /// atomically; Cancel leaves the catalog and historical runs unchanged.
    /// </summary>
    public sealed class ChecklistCatalogEditor : Form
    {
        private readonly List<ChecklistCase> _draft;
        private readonly DataGridView _grid = new();
        private readonly TextBox _search = new();
        private readonly ComboBox _section = new();
        private readonly ComboBox _algorithm = new();
        private readonly Label _status = new();
        private bool _refreshing;

        public IReadOnlyList<ChecklistCase> EditedCases => _draft;

        public ChecklistCatalogEditor(IEnumerable<ChecklistCase> source)
        {
            _draft = ChecklistCatalogRules.NormalizeCopy(source);
            Text = "Редактирование каталога проверок";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(780, 490);
            Size = new Size(1100, 720);
            BuildLayout();
            UpdateCategories();
            RefreshRows();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 61));
            Controls.Add(root);

            var top = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            top.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Изменения влияют на новые обходы. Уже сохранённые результаты не меняются.",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.DimGray
            }, 0, 0);
            var filters = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true
            };
            _search.PlaceholderText = "Найти по ID, названию, описанию...";
            _search.Width = 335;
            _search.Margin = new Padding(2, 5, 10, 2);
            _search.TextChanged += (_, _) => RefreshRows();
            filters.Controls.Add(_search);
            _section.Width = 220;
            _section.DropDownStyle = ComboBoxStyle.DropDownList;
            _section.Margin = new Padding(2, 5, 10, 2);
            _section.SelectedIndexChanged += (_, _) => RefreshRows();
            filters.Controls.Add(_section);
            _algorithm.Width = 165;
            _algorithm.DropDownStyle = ComboBoxStyle.DropDownList;
            _algorithm.Items.AddRange(new object[]
                { "Все алгоритмы", ChecklistCatalogRules.CityAlgorithm,
                    ChecklistCatalogRules.RegionalAlgorithm });
            _algorithm.SelectedIndex = 0;
            _algorithm.SelectedIndexChanged += (_, _) => RefreshRows();
            _algorithm.Margin = new Padding(2, 5, 10, 2);
            filters.Controls.Add(_algorithm);
            top.Controls.Add(filters, 0, 1);
            root.Controls.Add(top, 0, 0);

            _grid.Dock = DockStyle.Fill;
            _grid.RowHeadersVisible = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.ReadOnly = true;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoGenerateColumns = false;
            _grid.RowTemplate.Height = 38;
            _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            _grid.BackgroundColor = Color.White;
            AddColumn("ID", 112);
            AddColumn("Раздел", 190);
            AddColumn("Алгоритм", 105);
            AddColumn("Платформа", 145);
            AddColumn("Проверка", 375, fill: true);
            _grid.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0) EditSelected();
            };
            root.Controls.Add(_grid, 0, 1);

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            var commands = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, WrapContents = false
            };
            var addButton = MakeButton("Добавить", (_, _) => AddCase(), 106);
            addButton.Name = "CatalogAddButton";
            commands.Controls.Add(addButton);
            var editButton = MakeButton("Изменить", (_, _) => EditSelected(), 106);
            editButton.Name = "CatalogEditButton";
            commands.Controls.Add(editButton);
            var deleteButton = MakeButton("Удалить", (_, _) => DeleteSelected(), 106);
            deleteButton.Name = "CatalogDeleteButton";
            commands.Controls.Add(deleteButton);
            _status.Dock = DockStyle.Bottom;
            _status.ForeColor = Color.DimGray;
            commands.Controls.Add(_status);
            footer.Controls.Add(commands, 0, 0);

            var savePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            var cancel = MakeButton("Отмена", (_, _) => Close(), 110);
            cancel.DialogResult = DialogResult.Cancel;
            var save = MakeButton("Сохранить каталог", (_, _) => ConfirmSave(), 164);
            savePanel.Controls.Add(cancel);
            savePanel.Controls.Add(save);
            footer.Controls.Add(savePanel, 1, 0);
            root.Controls.Add(footer, 0, 2);
            CancelButton = cancel;
        }

        private void AddColumn(string title, int width, bool fill = false)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = title, Width = width,
                AutoSizeMode = fill
                    ? DataGridViewAutoSizeColumnMode.Fill
                    : DataGridViewAutoSizeColumnMode.None
            });
        }

        private static Button MakeButton(string text, EventHandler click, int width)
        {
            var button = new Button
            {
                Text = text, Width = width, Height = 35,
                Margin = new Padding(2, 4, 6, 1)
            };
            button.Click += click;
            return button;
        }

        private void UpdateCategories()
        {
            string selected = _section.SelectedItem?.ToString() ?? "Все разделы";
            _section.Items.Clear();
            _section.Items.Add("Все разделы");
            foreach (string category in _draft.Select(x => x.Category)
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(x => x))
                _section.Items.Add(category);
            _section.SelectedItem = _section.Items.Contains(selected)
                ? selected : "Все разделы";
        }

        private ChecklistCase? CurrentCase =>
            _grid.CurrentRow?.Tag as ChecklistCase;

        private void RefreshRows(string? selectId = null)
        {
            if (_refreshing) return;
            _refreshing = true;
            try
            {
                string? keep = selectId ?? CurrentCase?.Id;
                string query = _search.Text.Trim();
                string group = _section.SelectedItem?.ToString() ?? "Все разделы";
                string algorithm = _algorithm.SelectedItem?.ToString() ?? "Все алгоритмы";
                _grid.Rows.Clear();
                foreach (ChecklistCase item in _draft)
                {
                    if (group != "Все разделы" && item.Category != group)
                        continue;
                    if (algorithm != "Все алгоритмы" && item.Algorithm != algorithm)
                        continue;
                    if (query.Length > 0 &&
                        !new[] { item.Id, item.Category, item.Algorithm, item.Platforms,
                                 item.Module, item.Action, item.Expected }
                            .Any(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    int index = _grid.Rows.Add(item.Id, item.Category, item.Algorithm,
                        item.Platforms, item.Action);
                    _grid.Rows[index].Tag = item;
                }

                _grid.ClearSelection();
                DataGridViewRow? row = _grid.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(r => (r.Tag as ChecklistCase)?.Id == keep)
                    ?? _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault();
                if (row != null)
                {
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[4];
                }
                _status.Text = $"Всего: {_draft.Count}   Показано: {_grid.Rows.Count}";
            }
            finally { _refreshing = false; }
        }

        private void AddCase()
        {
            int number = 1;
            string id;
            do { id = $"USER-{number++:000}"; }
            while (ChecklistCatalogRules.HasDuplicateId(_draft, id));

            var item = new ChecklistCase
            {
                Id = id,
                Category = _section.SelectedItem?.ToString() is string group &&
                           group != "Все разделы" ? group : "Дополнительные проверки",
                Algorithm = _algorithm.SelectedItem?.ToString() is string algorithm &&
                            algorithm != "Все алгоритмы" ? algorithm : ""
            };
            if (!ChecklistEditDialog.Edit(this, item)) return;
            ChecklistCatalogRules.Normalize(item);
            if (!ValidateCase(item)) return;
            _draft.Add(item.NewBlankResult());
            UpdateCategories();
            _section.SelectedItem = "Все разделы";
            _algorithm.SelectedIndex = 0;
            _search.Clear();
            RefreshRows(item.Id);
        }

        private void EditSelected()
        {
            var item = CurrentCase;
            if (item == null) return;
            var edited = item.NewBlankResult();
            if (!ChecklistEditDialog.Edit(this, edited)) return;
            ChecklistCatalogRules.Normalize(edited);
            if (!ValidateCase(edited, item)) return;
            int position = _draft.IndexOf(item);
            _draft[position] = edited;
            UpdateCategories();
            _section.SelectedItem = "Все разделы";
            _algorithm.SelectedIndex = 0;
            _search.Clear();
            RefreshRows(edited.Id);
        }

        private bool ValidateCase(ChecklistCase edited, ChecklistCase? except = null)
        {
            if (string.IsNullOrWhiteSpace(edited.Id) ||
                string.IsNullOrWhiteSpace(edited.Action))
            {
                MessageBox.Show(this, "Укажите ID и описание проверки.",
                    "Проверка данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (ChecklistCatalogRules.HasDuplicateId(_draft, edited.Id, except))
            {
                MessageBox.Show(this, "Проверка с таким ID уже есть в каталоге.",
                    "Повтор ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void DeleteSelected()
        {
            var item = CurrentCase;
            if (item == null) return;
            if (MessageBox.Show(this,
                $"Удалить из каталога проверку {item.Id}?\n" +
                "Она останется в ранее сохранённых обходах.",
                "Удаление проверки", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _draft.Remove(item);
            UpdateCategories();
            RefreshRows();
        }

        private void ConfirmSave()
        {
            if (_draft.GroupBy(x => x.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
            {
                MessageBox.Show(this, "Обнаружены пустые или повторяющиеся ID.",
                    "Каталог", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
