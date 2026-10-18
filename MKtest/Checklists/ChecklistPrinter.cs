using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// Multi-page A4 print preview with visible paper checkboxes, or one
    /// detailed test card. No Excel or PDF printer dependency.
    /// </summary>
    public static class ChecklistPrinter
    {
        public static void Preview(IWin32Window owner,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases, bool single)
        {
            if (cases.Count == 0)
            {
                MessageBox.Show(owner, "В выбранном списке нет проверок.");
                return;
            }

            using var printer = new PrintDocument();
            printer.DocumentName = document.Title;
            printer.DefaultPageSettings.Landscape = !single;
            printer.DefaultPageSettings.Margins = new Margins(35, 35, 40, 40);
            var a4 = printer.PrinterSettings.PaperSizes.Cast<PaperSize>()
                .FirstOrDefault(p => p.Kind == PaperKind.A4);
            if (a4 != null)
                printer.DefaultPageSettings.PaperSize = a4;

            int row = 0, page = 0;
            printer.PrintPage += (_, args) =>
            {
                page++;
                if (single)
                    PrintCard(args, document, cases[0]);
                else
                    PrintChecklistPage(args, document, cases, ref row, page);
            };

            using var preview = new PrintPreviewDialog
            {
                Document = printer,
                Width = 1150, Height = 760,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Предпросмотр печати — " + document.Title
            };
            preview.ShowDialog(owner);
        }

        private static void PrintChecklistPage(PrintPageEventArgs e,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> items,
            ref int next, int page)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float left = area.Left;
            float right = area.Right;
            float y = area.Top;

            using var title = new Font("Segoe UI", 12, FontStyle.Bold);
            using var body = new Font("Segoe UI", 8);
            using var bold = new Font("Segoe UI", 8, FontStyle.Bold);
            using var small = new Font("Segoe UI", 7);
            using var pen = new Pen(Color.LightGray);

            g.DrawString(document.Title, title, Brushes.Black,
                new RectangleF(left, y, area.Width, 25));
            y += 28;
            string details = $"Версия: {document.Version}    Устройство: {document.Environment}" +
                $"    Проверял: {document.Tester}    Дата: {document.Date:dd.MM.yyyy}";
            g.DrawString(details, body, Brushes.Black,
                new RectangleF(left, y, area.Width, 32));
            y += 36;

            // Relative widths adapt to A4 landscape and the active printer.
            float[] relative = { .07f, .13f, .08f, .25f, .22f, .10f, .15f };
            float[] widths = relative.Select(x => x * area.Width).ToArray();
            float[] x = new float[widths.Length];
            x[0] = left;
            for (int i = 1; i < x.Length; i++)
                x[i] = x[i - 1] + widths[i - 1];

            const float headingHeight = 24;
            string[] columns =
                { "ID", "Раздел", "ОС", "Проверка", "Ожидается", "Отметка", "Замечание / баг" };
            for (int col = 0; col < columns.Length; col++)
            {
                var rect = new RectangleF(x[col], y, widths[col], headingHeight);
                g.FillRectangle(Brushes.Gainsboro, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawString(columns[col], bold, Brushes.Black,
                    new RectangleF(rect.X + 3, rect.Y + 3, rect.Width - 6, rect.Height - 6));
            }
            y += headingHeight;

            var format = new StringFormat
            {
                Trimming = StringTrimming.EllipsisWord,
                FormatFlags = StringFormatFlags.LineLimit
            };
            while (next < items.Count)
            {
                ChecklistCase item = items[next];
                string[] cells = {
                    item.Id, item.Category,
                    item.Platforms, item.Action, item.Expected,
                    "□ Есть\n□ Баг\n□ Нет", item.Bug + "\n" + item.Notes
                };
                float h = 46;
                for (int i = 0; i < cells.Length; i++)
                {
                    float measured = g.MeasureString(cells[i], small,
                        (int)Math.Max(30, widths[i] - 7)).Height + 9;
                    h = Math.Max(h, Math.Min(155, measured));
                }
                if (y + h > area.Bottom - 22)
                {
                    e.HasMorePages = true;
                    break;
                }

                for (int i = 0; i < cells.Length; i++)
                {
                    var rect = new RectangleF(x[i], y, widths[i], h);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    g.DrawString(cells[i], small, Brushes.Black,
                        new RectangleF(rect.X + 3, rect.Y + 3,
                            rect.Width - 6, rect.Height - 6), format);
                }
                next++;
                y += h;
            }
            g.DrawString($"Страница {page} • {items.Count} проверок • " +
                $"Отметьте один вариант для каждой строки",
                small, Brushes.DimGray,
                new RectangleF(left, area.Bottom - 18, area.Width, 18));
            if (next >= items.Count)
                e.HasMorePages = false;
        }

        private static void PrintCard(PrintPageEventArgs e,
            ChecklistDocument document, ChecklistCase item)
        {
            var g = e.Graphics!;
            var area = e.MarginBounds;
            float x = area.Left, y = area.Top, width = area.Width;
            using var title = new Font("Segoe UI", 14, FontStyle.Bold);
            using var normal = new Font("Segoe UI", 10);
            using var bold = new Font("Segoe UI", 10, FontStyle.Bold);
            using var pen = new Pen(Color.LightGray);

            g.DrawString("ИНДИВИДУАЛЬНЫЙ ТЕСТ-ЛИСТ MKtest",
                title, Brushes.Black, x, y);
            y += 40;
            g.DrawString($"№: {item.Id}    Дата: {document.Date:dd.MM.yyyy}" +
                $"    Проверял: {document.Tester}",
                normal, Brushes.Black, x, y);
            y += 28;
            void Field(string caption, string value, int height)
            {
                g.DrawRectangle(pen, x, y, width, height);
                g.DrawString(caption, bold, Brushes.Black, x + 7, y + 6);
                g.DrawString(value, normal, Brushes.Black,
                    new RectangleF(x + 7, y + 28, width - 14, height - 33));
                y += height;
            }

            Field("Версия / устройство", document.Version + " / " +
                document.Environment, 56);
            Field("Раздел / платформа", item.Category + " / " +
                item.Platforms, 60);
            Field("Модуль / сценарий", item.Module, 58);
            Field("Проверка и шаги", item.Action, 110);
            Field("Ожидаемый результат", item.Expected, 110);
            Field("Фактический результат / замечания", item.Notes, 110);
            Field("Номер бага / ссылка", item.Bug, 60);
            y += 16;
            g.DrawString("Результат:     □ Есть       □ Баг       □ Нет       □ Н/П",
                bold, Brushes.Black, x, y);
            e.HasMorePages = false;
        }
    }
}
