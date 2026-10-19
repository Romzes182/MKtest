using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    public static class ChecklistEditDialog
    {
        private sealed class PlacementChoice
        {
            public string? AfterId { get; }
            public string Caption { get; }

            public PlacementChoice(string? afterId, string caption)
            {
                AfterId = afterId;
                Caption = caption;
            }

            public override string ToString() => Caption;
        }

        /// <summary>Editing: choose any existing section, or type a new one.</summary>
        public static bool Edit(IWin32Window owner, ChecklistCase item,
            IReadOnlyList<ChecklistCase>? catalog = null) =>
            EditCore(owner, item, catalog ?? Array.Empty<ChecklistCase>(),
                false, null, out _);

        /// <summary>
        /// Adding: in addition to section and algorithm, select the exact check
        /// after which the new entry will appear, or the end of its group.
        /// </summary>
        public static bool Add(IWin32Window owner, ChecklistCase item,
            IReadOnlyList<ChecklistCase> catalog, out string? afterId,
            string? suggestedAfterId = null) =>
            EditCore(owner, item, catalog, true, suggestedAfterId, out afterId);

        private static bool EditCore(IWin32Window owner, ChecklistCase item,
            IReadOnlyList<ChecklistCase> catalog, bool forAddition,
            string? suggestedAfterId, out string? afterId)
        {
            afterId = null;
            using var window = new Form
            {
                Text = forAddition ? "Добавить проверку в каталог" : "Изменить проверку",
                Width = 820, Height = 645,
                MinimumSize = new Size(700, 540),
                StartPosition = FormStartPosition.CenterParent
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(14),
                ColumnCount = 2, RowCount = 9
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int[] sizes = { 38, 38, 38, forAddition ? 56 : 0, 38, 38, 130, 110, 45 };
            foreach (int size in sizes)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, size));

            void Caption(int row, string label) =>
                layout.Controls.Add(new Label
                {
                    Text = label, Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, row);

            TextBox Input(int row, string label, string value, bool multiline = false)
            {
                Caption(row, label);
                var field = new TextBox
                {
                    Text = value, Dock = DockStyle.Fill,
                    Multiline = multiline,
                    ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
                };
                layout.Controls.Add(field, 1, row);
                return field;
            }

            TextBox id = Input(0, "ID проверки", item.Id);
            Caption(1, "В какой раздел?");
            var category = new ComboBox
            {
                Name = "CatalogTargetCategory",
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            var categories = catalog.Select(x => x.Category)
                .Append(ChecklistCatalogRules.FareCategory)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x);
            foreach (string existing in categories)
                category.Items.Add(existing);
            category.Text = item.Category;
            layout.Controls.Add(category, 1, 1);

            Caption(2, "Алгоритм");
            var algorithm = new ComboBox
            {
                Name = "CatalogTargetAlgorithm",
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList
            };
            algorithm.Items.AddRange(new object[]
            {
                "Не используется",
                ChecklistCatalogRules.CityAlgorithm,
                ChecklistCatalogRules.RegionalAlgorithm
            });
            algorithm.SelectedItem = string.IsNullOrWhiteSpace(item.Algorithm)
                ? "Не используется" : item.Algorithm;
            if (algorithm.SelectedIndex < 0) algorithm.SelectedIndex = 0;
            layout.Controls.Add(algorithm, 1, 2);

            var position = new ComboBox
            {
                Name = "CatalogTargetPosition",
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList
            };
            if (forAddition)
            {
                Caption(3, "Куда вставить?");
                layout.Controls.Add(position, 1, 3);
            }

            TextBox platforms = Input(4, "Платформа / ОС", item.Platforms);
            TextBox module = Input(5, "Сценарий / протокол", item.Module);
            TextBox action = Input(6, "Проверка / действия", item.Action, true);
            TextBox expected = Input(7, "Ожидаемый результат", item.Expected, true);

            void UpdatePosition()
            {
                if (!forAddition) return;
                string chosenCategory = category.Text.Trim();
                string chosenAlgorithm = algorithm.Enabled && algorithm.SelectedIndex > 0
                    ? algorithm.SelectedItem?.ToString() ?? "" : "";
                var target = new ChecklistCase
                {
                    Category = chosenCategory, Algorithm = chosenAlgorithm
                };
                var previous = position.SelectedItem as PlacementChoice;
                string? requested = previous?.AfterId ?? suggestedAfterId;
                position.BeginUpdate();
                try
                {
                    position.Items.Clear();
                    position.Items.Add(new PlacementChoice(null,
                        "В конец выбранного раздела / алгоритма"));
                    foreach (ChecklistCase check in ChecklistCatalogPlacement
                                 .GroupMembers(catalog, target))
                    {
                        string description = check.Action.Replace('\r', ' ')
                            .Replace('\n', ' ');
                        if (description.Length > 65)
                            description = description[..62] + "...";
                        position.Items.Add(new PlacementChoice(check.Id,
                            $"После {check.Id} — {description}"));
                    }
                    int selected = 0;
                    for (int i = 1; i < position.Items.Count; i++)
                        if ((position.Items[i] as PlacementChoice)?.AfterId == requested)
                        {
                            selected = i;
                            break;
                        }
                    position.SelectedIndex = selected;
                }
                finally { position.EndUpdate(); }
            }

            void RefreshAlgorithm()
            {
                bool fare = category.Text.Trim().Equals(
                    ChecklistCatalogRules.FareCategory, StringComparison.OrdinalIgnoreCase)
                    || category.Text.Trim().Equals("Статистика не оплативших",
                        StringComparison.OrdinalIgnoreCase);
                algorithm.Enabled = fare;
                if (!fare)
                    algorithm.SelectedIndex = 0;
                else if (algorithm.SelectedIndex == 0)
                    algorithm.SelectedItem =
                        category.Text.Trim().Equals("Статистика не оплативших",
                            StringComparison.OrdinalIgnoreCase)
                            ? ChecklistCatalogRules.CityAlgorithm
                            : ChecklistCatalogRules.RegionalAlgorithm;
                UpdatePosition();
            }

            category.TextChanged += (_, _) => RefreshAlgorithm();
            algorithm.SelectedIndexChanged += (_, _) => UpdatePosition();
            RefreshAlgorithm();

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft
            };
            var save = new Button
            {
                Text = forAddition ? "Добавить проверку" : "Сохранить",
                Width = 160, Height = 32, DialogResult = DialogResult.OK
            };
            var cancel = new Button
            {
                Text = "Отмена", Width = 105, Height = 32,
                DialogResult = DialogResult.Cancel
            };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, 8);
            window.Controls.Add(layout);
            window.AcceptButton = save;
            window.CancelButton = cancel;
            window.FormClosing += (_, args) =>
            {
                if (window.DialogResult != DialogResult.OK) return;
                if (!string.IsNullOrWhiteSpace(id.Text) &&
                    !string.IsNullOrWhiteSpace(category.Text) &&
                    !string.IsNullOrWhiteSpace(action.Text))
                    return;
                MessageBox.Show(window,
                    "Заполните ID, раздел и описание проверки.",
                    "Проверка данных", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                args.Cancel = true;
            };

            if (window.ShowDialog(owner) != DialogResult.OK)
                return false;

            item.Id = id.Text.Trim();
            item.Category = category.Text.Trim();
            item.Algorithm = algorithm.Enabled && algorithm.SelectedIndex > 0
                ? algorithm.SelectedItem?.ToString() ?? "" : "";
            item.Platforms = platforms.Text.Trim();
            item.Module = module.Text.Trim();
            item.Action = action.Text.Trim();
            item.Expected = expected.Text.Trim();
            afterId = forAddition
                ? (position.SelectedItem as PlacementChoice)?.AfterId : null;
            return true;
        }
    }
}
