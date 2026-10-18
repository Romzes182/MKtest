using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    public static class ChecklistEditDialog
    {
        public static bool Edit(IWin32Window owner, ChecklistCase item)
        {
            using var window = new Form
            {
                Text = "Карточка проверки",
                Width = 760,
                Height = 600,
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(650, 450)
            };

            var fields = new[]
            {
                ("ID", item.Id), ("Раздел", item.Category),
                ("ОС / платформа", item.Platforms),
                ("Сценарий / модуль", item.Module),
                ("Проверка / шаги", item.Action),
                ("Ожидаемый результат", item.Expected),
                ("№ бага / ссылка", item.Bug),
                ("Комментарий", item.Notes)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, Padding = new Padding(10),
                ColumnCount = 2, RowCount = fields.Length + 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            var controls = new List<TextBox>();
            for (int i = 0; i < fields.Length; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Percent,
                    i is 4 or 5 or 7 ? 20 : 8));
                layout.Controls.Add(new Label
                {
                    Text = fields[i].Item1, Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, i);
                var input = new TextBox
                {
                    Text = fields[i].Item2,
                    Multiline = i is 4 or 5 or 7,
                    ScrollBars = i is 4 or 5 or 7 ? ScrollBars.Vertical : ScrollBars.None,
                    Dock = DockStyle.Fill
                };
                controls.Add(input);
                layout.Controls.Add(input, 1, i);
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft
            };
            var accept = new Button
            {
                Text = "Сохранить", DialogResult = DialogResult.OK,
                Width = 110, Height = 28
            };
            var cancel = new Button
            {
                Text = "Отмена", DialogResult = DialogResult.Cancel,
                Width = 95, Height = 28
            };
            buttons.Controls.Add(accept);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 1, fields.Length);
            window.Controls.Add(layout);
            window.AcceptButton = accept;
            window.CancelButton = cancel;

            if (window.ShowDialog(owner) != DialogResult.OK)
                return false;

            if (string.IsNullOrWhiteSpace(controls[4].Text))
            {
                MessageBox.Show(owner, "Поле «Проверка / шаги» не должно быть пустым.");
                return false;
            }

            item.Id = controls[0].Text.Trim();
            item.Category = controls[1].Text.Trim();
            item.Platforms = controls[2].Text.Trim();
            item.Module = controls[3].Text.Trim();
            item.Action = controls[4].Text.Trim();
            item.Expected = controls[5].Text.Trim();
            item.Bug = controls[6].Text.Trim();
            item.Notes = controls[7].Text.Trim();
            return true;
        }
    }
}
