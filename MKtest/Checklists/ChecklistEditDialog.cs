using System;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    public static class ChecklistEditDialog
    {
        /// <summary>Edit the definition of a catalog item, not a test-run result.</summary>
        public static bool Edit(IWin32Window owner, ChecklistCase item)
        {
            using var window = new Form
            {
                Text = "Проверка каталога",
                Width = 780, Height = 550,
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(670, 470)
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(14),
                ColumnCount = 2, RowCount = 8
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 182));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int size in new[] { 38, 38, 38, 38, 38, 130, 100, 46 })
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, size));

            TextBox Input(int row, string label, string value, bool multiline = false)
            {
                layout.Controls.Add(new Label
                {
                    Text = label, Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, row);
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
            TextBox category = Input(1, "Раздел", item.Category);
            layout.Controls.Add(new Label
            {
                Text = "Алгоритм безбилетников",
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            }, 0, 2);
            var algorithm = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList
            };
            algorithm.Items.AddRange(new object[]
            {
                "Без алгоритма",
                ChecklistCatalogRules.CityAlgorithm,
                ChecklistCatalogRules.RegionalAlgorithm
            });
            algorithm.SelectedItem = string.IsNullOrWhiteSpace(item.Algorithm)
                ? "Без алгоритма" : item.Algorithm;
            if (algorithm.SelectedIndex < 0) algorithm.SelectedIndex = 0;
            layout.Controls.Add(algorithm, 1, 2);

            void RefreshAlgorithm()
            {
                bool fare = category.Text.Trim() is "Безбилетники" or
                    "Статистика не оплативших";
                algorithm.Enabled = fare;
                if (!fare)
                    algorithm.SelectedIndex = 0;
                else if (algorithm.SelectedIndex == 0)
                    algorithm.SelectedItem =
                        category.Text.Trim() == "Статистика не оплативших"
                            ? ChecklistCatalogRules.CityAlgorithm
                            : ChecklistCatalogRules.RegionalAlgorithm;
            }
            category.TextChanged += (_, _) => RefreshAlgorithm();
            RefreshAlgorithm();

            TextBox platforms = Input(3, "ОС / платформа", item.Platforms);
            TextBox module = Input(4, "Сценарий / протокол", item.Module);
            TextBox action = Input(5, "Проверка / действия", item.Action, true);
            TextBox expected = Input(6, "Ожидаемый результат", item.Expected, true);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft
            };
            var save = new Button
            {
                Text = "Сохранить", Width = 125, Height = 32,
                DialogResult = DialogResult.OK
            };
            var cancel = new Button
            {
                Text = "Отмена", Width = 105, Height = 32,
                DialogResult = DialogResult.Cancel
            };
            buttons.Controls.Add(save);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, 7);
            window.Controls.Add(layout);
            window.AcceptButton = save;
            window.CancelButton = cancel;
            window.FormClosing += (_, args) =>
            {
                if (window.DialogResult != DialogResult.OK) return;
                if (!string.IsNullOrWhiteSpace(id.Text) &&
                    !string.IsNullOrWhiteSpace(action.Text)) return;
                MessageBox.Show(window, "ID и описание проверки обязательны.",
                    "Проверка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                args.Cancel = true;
            };

            if (window.ShowDialog(owner) != DialogResult.OK)
                return false;

            item.Id = id.Text.Trim();
            item.Category = category.Text.Trim();
            item.Algorithm = algorithm.Enabled &&
                algorithm.SelectedIndex > 0
                    ? algorithm.SelectedItem?.ToString() ?? "" : "";
            item.Platforms = platforms.Text.Trim();
            item.Module = module.Text.Trim();
            item.Action = action.Text.Trim();
            item.Expected = expected.Text.Trim();
            return true;
        }
    }
}
