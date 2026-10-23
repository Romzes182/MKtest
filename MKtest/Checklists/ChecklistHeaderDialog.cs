using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>Edits the complete header from the supplied paper test report template.</summary>
    public static class ChecklistHeaderDialog
    {
        public static bool Edit(IWin32Window owner, ChecklistDocument document)
        {
            var current = (document.PrintHeader ?? new ChecklistPrintHeader()).Clone();
            using var window = new Form
            {
                Text = "Шапка печатного чек-листа",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(940, 790),
                MinimumSize = new Size(720, 550)
            };
            var content = new Panel
            {
                Dock = DockStyle.Fill, AutoScroll = true
            };
            var grid = new TableLayoutPanel
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2, RowCount = 0,
                Dock = DockStyle.Top, Padding = new Padding(12)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 255));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 605));
            content.Controls.Add(grid);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 52, Padding = new Padding(12, 6, 12, 6),
                FlowDirection = FlowDirection.RightToLeft
            };
            var ok = new Button
            {
                Text = "Сохранить шапку", Width = 156, Height = 32,
                DialogResult = DialogResult.OK
            };
            var cancel = new Button
            {
                Text = "Отмена", Width = 110, Height = 32,
                DialogResult = DialogResult.Cancel
            };
            footer.Controls.Add(ok);
            footer.Controls.Add(cancel);
            window.Controls.Add(content);
            window.Controls.Add(footer);
            window.AcceptButton = ok;
            window.CancelButton = cancel;

            void AddRow(string caption, Control input, int height = 35)
            {
                int index = grid.RowCount++;
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
                var label = new Label
                {
                    Text = caption, Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                input.Dock = DockStyle.Fill;
                input.Margin = new Padding(4, 3, 4, 3);
                grid.Controls.Add(label, 0, index);
                grid.Controls.Add(input, 1, index);
            }

            void Heading(string title)
            {
                int index = grid.RowCount++;
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
                var label = new Label
                {
                    Text = title, Dock = DockStyle.Fill,
                    ForeColor = Color.FromArgb(38, 72, 112),
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                grid.Controls.Add(label, 0, index);
                grid.SetColumnSpan(label, 2);
            }

            TextBox Field(string caption, string value, bool multiline = false)
            {
                var field = new TextBox
                {
                    Text = value, Multiline = multiline,
                    ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None
                };
                AddRow(caption, field, multiline ? 59 : 35);
                return field;
            }

            Heading("ОБЩИЕ СВЕДЕНИЯ");
            TextBox project = Field("Проект", current.Project);
            TextBox version = Field("Версия ПО", document.Version);
            var platform = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDown
            };
            platform.Items.AddRange(new object[]
            {
                "Android 7", "Android 11", "Linux",
                "Android (все)", "Несколько платформ"
            });
            platform.Text = ChecklistPrintLayout.RunOperatingSystem(
                document, document.Cases);
            AddRow("ОС / платформа", platform);
            TextBox environment = Field("Окружение (оборудование)",
                current.EnvironmentDetails, true);
            TextBox device = Field("Устройство / стенд", document.Environment);
            var date = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd.MM.yyyy",
                Value = document.Date >= DateTimePicker.MinDateTime &&
                        document.Date <= DateTimePicker.MaxDateTime
                    ? document.Date : DateTime.Today
            };
            AddRow("Дата тестирования", date);
            TextBox tester = Field("Исполнитель", document.Tester);
            TextBox purpose = Field("Цель тестирования", current.Purpose, true);

            Heading("ОБЪЁМ ТЕСТИРОВАНИЯ");
            TextBox widgets = Field("Базовые виджеты", current.Widgets, true);
            TextBox announcers = Field("Автоинформаторы", current.Announcers, true);
            TextBox media = Field("Медиа", current.Media, true);
            TextBox serviceData = Field("Служебные данные", current.ServiceData, true);
            TextBox protocols = Field("Протоколы оплаты", current.PaymentProtocols, true);
            TextBox reliability = Field("Надежность", current.Reliability, true);

            Heading("СПЕЦИАЛЬНО РАЗРАБОТАННОЕ АВТОМАТИЗИРОВАННОЕ СРЕДСТВО ТЕСТИРОВАНИЯ");
            TextBox setup = Field("Настройка тестового окружения",
                current.EnvironmentSetup, true);
            TextBox operations = Field("Работа с прикладными протоколами",
                current.ProtocolOperations, true);
            TextBox validation = Field("Валидация данных", current.DataValidation, true);

            var detailed = new CheckBox
            {
                Text = "Показывать расширенную шапку в печатном бланке",
                Checked = current.Detailed, AutoSize = true
            };
            AddRow("Печать", detailed, 36);

            if (window.ShowDialog(owner) != DialogResult.OK)
                return false;

            document.RunPlatform = platform.Text.Trim();
            document.Version = version.Text.Trim();
            document.Environment = device.Text.Trim();
            document.Tester = tester.Text.Trim();
            document.Date = date.Value.Date;
            current.Project = project.Text.Trim();
            current.EnvironmentDetails = environment.Text.Trim();
            current.Purpose = purpose.Text.Trim();
            current.Widgets = widgets.Text.Trim();
            current.Announcers = announcers.Text.Trim();
            current.Media = media.Text.Trim();
            current.ServiceData = serviceData.Text.Trim();
            current.PaymentProtocols = protocols.Text.Trim();
            current.Reliability = reliability.Text.Trim();
            current.EnvironmentSetup = setup.Text.Trim();
            current.ProtocolOperations = operations.Text.Trim();
            current.DataValidation = validation.Text.Trim();
            current.Detailed = detailed.Checked;
            document.PrintHeader = current;
            return true;
        }
    }
}
