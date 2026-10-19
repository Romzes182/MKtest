using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// Compact A4 portrait forms for inspections on paper. The general list
    /// has four columns; internal IDs, platform, expected result and printed
    /// checkbox options are not table columns. The handwritten result field
    /// is intentionally empty for use during the walk-through.
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
            printer.DefaultPageSettings.Landscape = false;
            printer.DefaultPageSettings.Margins = new Margins(45, 45, 45, 45);
            var a4 = printer.PrinterSettings.PaperSizes.Cast<PaperSize>()
                .FirstOrDefault(p => p.Kind == PaperKind.A4);
            if (a4 != null)
                printer.DefaultPageSettings.PaperSize = a4;

            int row = 0, page = 0;
            // PrintPreviewDialog can render the pages more than once before
            // printing; always restart at the first row on a new print job.
            printer.BeginPrint += (_, _) => { row = 0; page = 0; };
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
                Width = 1050, Height = 830,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Предпросмотр A4 (книжная) — " + document.Title
            };
            preview.ShowDialog(owner);
        }

        private static float DrawHeader(PrintPageEventArgs e,
            ChecklistDocument document, IEnumerable<ChecklistCase> cases, int page,
            bool single)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float y = area.Top;

            using var title = new Font("Segoe UI", 12, FontStyle.Bold);
            using var small = new Font("Segoe UI", 8.5f);
            using var rule = new Pen(Color.LightGray);

            string headline = single
                ? "ИНДИВИДУАЛЬНЫЙ ТЕСТ-ЛИСТ MKtest" : document.Title;
            y = DrawTextRow(g, headline, title, area.Left, y, area.Width, 7);

            string runId = document.RunId.Length >= 8
                ? document.RunId[..8] : document.RunId;
            y = DrawTextRow(g,
                $"Обход: {runId}    Дата: {document.Date:dd.MM.yyyy}" +
                (single ? "" : $"    Страница: {page}"),
                small, area.Left, y, area.Width, 1);
            y = DrawTextRow(g,
                $"Версия ПО: {document.Version}    Устройство: {document.Environment}",
                small, area.Left, y, area.Width, 1);
            y = DrawTextRow(g,
                "ОС / платформа: " + ChecklistPrintLayout.OperatingSystems(cases),
                small, area.Left, y, area.Width, 1);
            y = DrawTextRow(g,
                "Исполнитель: " + document.Tester,
                small, area.Left, y, area.Width, 6);

            g.DrawLine(rule, area.Left, y, area.Right, y);
            return y + 9;
        }

        private static float DrawTextRow(Graphics g, string text, Font font,
            float x, float y, float width, float bottomPadding)
        {
            float height = Math.Max(font.GetHeight(g) + 3,
                g.MeasureString(text, font, Math.Max(40, (int)width)).Height + 2);
            g.DrawString(text, font, Brushes.Black,
                new RectangleF(x, y, width, height));
            return y + height + bottomPadding;
        }

        private static void PrintChecklistPage(PrintPageEventArgs e,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> items,
            ref int next, int page)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float y = DrawHeader(e, document, items, page, false);
            float tableTop = y;

            using var body = new Font("Segoe UI", 8.5f);
            using var bold = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var foot = new Font("Segoe UI", 7.5f);
            using var pen = new Pen(Color.FromArgb(170, 170, 170));
            using var format = new StringFormat
            {
                Trimming = StringTrimming.EllipsisWord
            };

            float[] widths = ChecklistPrintLayout.ListColumnRatios
                .Select(r => r * area.Width).ToArray();
            float[] x = new float[widths.Length];
            x[0] = area.Left;
            for (int i = 1; i < x.Length; i++)
                x[i] = x[i - 1] + widths[i - 1];

            const float headingHeight = 35;
            for (int col = 0; col < widths.Length; col++)
            {
                var rect = new RectangleF(x[col], y, widths[col], headingHeight);
                g.FillRectangle(Brushes.Gainsboro, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawString(ChecklistPrintLayout.ListColumns[col], bold,
                    Brushes.Black, new RectangleF(rect.X + 3, rect.Y + 3,
                        rect.Width - 6, rect.Height - 6), format);
            }
            y += headingHeight;

            while (next < items.Count)
            {
                ChecklistCase item = items[next];
                string[] cells =
                {
                    ChecklistPrintLayout.NumberInRun(document, item).ToString("000"),
                    ChecklistPrintLayout.Section(item),
                    ChecklistPrintLayout.Inspection(item),
                    "" // Spacious blank area for handwritten marks and bug numbers.
                };

                float height = 52;
                for (int i = 0; i < cells.Length - 1; i++)
                {
                    float measured = g.MeasureString(cells[i], body,
                        Math.Max(30, (int)widths[i] - 8)).Height + 14;
                    height = Math.Max(height, measured);
                }
                // Avoid an unusually long scenario forcing an infinite page loop.
                height = Math.Min(height, Math.Max(52, area.Height - 190));

                if (y + height > area.Bottom - 29 &&
                    y > tableTop + headingHeight + 1)
                {
                    e.HasMorePages = true;
                    break;
                }

                for (int i = 0; i < cells.Length; i++)
                {
                    var rect = new RectangleF(x[i], y, widths[i], height);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    if (cells[i].Length > 0)
                        g.DrawString(cells[i], body, Brushes.Black,
                            new RectangleF(rect.X + 4, rect.Y + 5,
                                rect.Width - 8, rect.Height - 10), format);
                }
                next++;
                y += height;
            }

            string footnote = $"Страница {page}  •  {items.Count} проверок" +
                "  •  № совпадает с номером в MKtest";
            g.DrawString(footnote, foot, Brushes.DimGray,
                new RectangleF(area.Left, area.Bottom - 20, area.Width, 18));
            if (next >= items.Count)
                e.HasMorePages = false;
        }

        private static void PrintCard(PrintPageEventArgs e,
            ChecklistDocument document, ChecklistCase item)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float left = area.Left;
            float y = DrawHeader(e, document, new[] { item }, 1, true);
            float width = area.Width;

            using var normal = new Font("Segoe UI", 10);
            using var heading = new Font("Segoe UI", 10, FontStyle.Bold);
            using var border = new Pen(Color.LightGray);

            void Field(string label, string value, float height)
            {
                var rect = new RectangleF(left, y, width, height);
                g.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawString(label, heading, Brushes.Black,
                    new RectangleF(left + 8, y + 6, width - 16, 21));
                if (value.Length > 0)
                    g.DrawString(value, normal, Brushes.Black,
                        new RectangleF(left + 8, y + 29,
                            width - 16, height - 36));
                y += height;
            }

            Field("№ проверки", ChecklistPrintLayout.NumberInRun(
                document, item).ToString("000"), 58);
            Field("Раздел / алгоритм", ChecklistPrintLayout.Section(item), 77);
            if (!string.IsNullOrWhiteSpace(item.Module))
                Field("Сценарий / протокол", item.Module, 80);
            Field("Проверка / действия", item.Action, 190);

            // Empty boxes, no pre-filled checkbox choices or internal test IDs.
            // Operators can write a mark, a bug number and observed behavior.
            Field("Результат проверки (заполняется вручную)", "", 100);
            Field("Номер бага / замечания", "", 190);
            e.HasMorePages = false;
        }
    }
}
