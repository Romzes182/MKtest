using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace MKtest.Checklists
{
    /// <summary>
    /// A4 portrait checklists. The first page has an editable testing header,
    /// following pages retain compact run metadata. PrintDialog's page range
    /// is applied to logical pages, so pages before the range are not spooled.
    /// </summary>
    public static class ChecklistPrinter
    {
        private readonly record struct PageSpan(int Start, int End);

        public static void Preview(IWin32Window owner,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases, bool single)
        {
            if (!HasTests(owner, cases)) return;

            using var printer = CreateDocument(document, cases, single, 1, int.MaxValue);
            using var dialog = new PrintPreviewDialog
            {
                Document = printer,
                Width = 1050, Height = 830,
                StartPosition = FormStartPosition.CenterParent,
                Text = "Предпросмотр A4 (книжная) — " + document.Title
            };
            try
            {
                dialog.ShowDialog(owner);
            }
            catch (InvalidPrinterException ex)
            {
                ShowPrintError(owner, ex);
            }
        }

        /// <summary>
        /// PrintDialog lets the operator choose a printer, all pages or From/To.
        /// We render exactly the selected logical page indexes ourselves, as
        /// GDI PrintDocument does not guarantee automatic page filtering.
        /// </summary>
        public static void PrintSelectedPages(IWin32Window owner,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases, bool single)
        {
            if (!HasTests(owner, cases)) return;

            try
            {
                // Calculate the actual logical count using the same renderer,
                // print settings and preview graphics, without physical printing.
                using var countDocument = CreateDocument(
                    document, cases, single, 1, int.MaxValue);
                var controller = new PreviewPrintController();
                countDocument.PrintController = controller;
                countDocument.Print();
                int count = Math.Max(1, controller.GetPreviewPageInfo().Length);

                using var chooserDocument = CreateDocument(
                    document, cases, single, 1, int.MaxValue);
                chooserDocument.PrinterSettings.MinimumPage = 1;
                chooserDocument.PrinterSettings.MaximumPage = count;
                chooserDocument.PrinterSettings.FromPage = 1;
                chooserDocument.PrinterSettings.ToPage = count;
                using var dialog = new PrintDialog
                {
                    Document = chooserDocument,
                    AllowSomePages = true,
                    AllowSelection = false,
                    AllowCurrentPage = false,
                    UseEXDialog = true
                };

                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return;

                bool selectedRange =
                    chooserDocument.PrinterSettings.PrintRange == PrintRange.SomePages;
                int from = selectedRange
                    ? Math.Clamp(chooserDocument.PrinterSettings.FromPage, 1, count)
                    : 1;
                int to = selectedRange
                    ? Math.Clamp(chooserDocument.PrinterSettings.ToPage, from, count)
                    : int.MaxValue;

                using var selectedDocument = CreateDocument(
                    document, cases, single, from, to);
                selectedDocument.PrinterSettings = chooserDocument.PrinterSettings;
                selectedDocument.DefaultPageSettings = chooserDocument.DefaultPageSettings;
                selectedDocument.Print();
            }
            catch (Exception ex) when (ex is InvalidPrinterException
                                       or System.ComponentModel.Win32Exception
                                       or ArgumentException)
            {
                ShowPrintError(owner, ex);
            }
        }

        private static bool HasTests(IWin32Window owner,
            IReadOnlyList<ChecklistCase> cases)
        {
            if (cases.Count > 0) return true;
            MessageBox.Show(owner, "В выбранном обходе нет проверок.",
                "Печать", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private static void ShowPrintError(IWin32Window owner, Exception ex) =>
            MessageBox.Show(owner,
                "Печать не выполнена: " + ex.Message,
                "Ошибка печати", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static PrintDocument CreateDocument(
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases,
            bool single, int fromPage, int toPage)
        {
            var printer = new PrintDocument();
            printer.DocumentName = document.Title;
            printer.DefaultPageSettings.Landscape = false;
            printer.DefaultPageSettings.Margins = new Margins(45, 45, 45, 45);
            var a4 = printer.PrinterSettings.PaperSizes.Cast<PaperSize>()
                .FirstOrDefault(p => p.Kind == PaperKind.A4);
            if (a4 != null)
                printer.DefaultPageSettings.PaperSize = a4;

            List<PageSpan>? pages = null;
            int logicalPage = fromPage;
            printer.BeginPrint += (_, _) =>
            {
                pages = null;
                logicalPage = fromPage;
            };
            printer.PrintPage += (_, e) =>
            {
                if (single)
                {
                    PrintCard(e, document, cases[0]);
                    e.HasMorePages = false;
                    return;
                }

                pages ??= Paginate(e.Graphics!, e.MarginBounds, document, cases);
                int last = Math.Min(toPage, pages.Count);
                if (logicalPage > last || logicalPage < 1)
                {
                    e.HasMorePages = false;
                    return;
                }

                DrawChecklistPage(e, document, cases,
                    pages[logicalPage - 1], logicalPage);
                logicalPage++;
                e.HasMorePages = logicalPage <= last;
            };
            return printer;
        }

        private static List<PageSpan> Paginate(Graphics g, Rectangle area,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases)
        {
            var result = new List<PageSpan>();
            using var body = new Font("Segoe UI", 8.5f);
            float[] widths = ChecklistPrintLayout.ListColumnRatios
                .Select(r => r * area.Width).ToArray();
            int index = 0;
            do
            {
                int page = result.Count + 1;
                float top = DrawHeader(g, area, document, cases, page, false,
                    render: false);
                float y = top + 35f; // table's column headers
                int start = index;
                while (index < cases.Count)
                {
                    float height = RowHeight(g, body, widths, document, cases[index],
                        Math.Max(52, area.Height - 190));
                    if (y + height > area.Bottom - 28 && index > start)
                        break;
                    y += height;
                    index++;
                }
                result.Add(new PageSpan(start, index));
            } while (index < cases.Count);
            return result;
        }

        private static float RowHeight(Graphics g, Font font,
            IReadOnlyList<float> widths, ChecklistDocument run,
            ChecklistCase item, float maximum)
        {
            string[] text =
            {
                ChecklistPrintLayout.NumberInRun(run, item).ToString("000"),
                ChecklistPrintLayout.Section(item),
                ChecklistPrintLayout.Inspection(item)
            };
            float height = 52;
            for (int i = 0; i < text.Length; i++)
            {
                float measured = g.MeasureString(text[i], font,
                    Math.Max(30, (int)widths[i] - 8)).Height + 14;
                height = Math.Max(height, measured);
            }
            return Math.Min(height, maximum);
        }

        private static float DrawHeader(Graphics g, Rectangle area,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases,
            int page, bool single, bool render)
        {
            float y = area.Top;
            var header = document.PrintHeader ?? new ChecklistPrintHeader();
            using var title = new Font("Segoe UI", 11, FontStyle.Bold);
            using var text = new Font("Segoe UI", 8.3f);
            using var label = new Font("Segoe UI", 8.2f, FontStyle.Bold);
            using var section = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var pen = new Pen(Color.FromArgb(175, 175, 175));
            using var format = new StringFormat { Trimming = StringTrimming.EllipsisWord };

            string runId = document.RunId.Length >= 8
                ? document.RunId[..8] : document.RunId;
            string operatingSystem = ChecklistPrintLayout.RunOperatingSystem(
                document, cases);

            void Heading(string heading)
            {
                float h = g.MeasureString(heading, section,
                    Math.Max(70, (int)area.Width - 12)).Height + 10;
                if (render)
                {
                    g.FillRectangle(Brushes.Gainsboro,
                        area.Left, y, area.Width, h);
                    g.DrawRectangle(pen, area.Left, y, area.Width, h);
                    g.DrawString(heading, section, Brushes.Black,
                        new RectangleF(area.Left + 5, y + 4, area.Width - 10, h - 6));
                }
                y += h;
            }

            void Field(string caption, string value)
            {
                const float labelRatio = .35f;
                float leftWidth = area.Width * labelRatio;
                float rightWidth = area.Width - leftWidth;
                value ??= "";
                float lh = g.MeasureString(caption, label,
                    Math.Max(32, (int)leftWidth - 10)).Height;
                float rh = g.MeasureString(value, text,
                    Math.Max(32, (int)rightWidth - 12)).Height;
                float h = Math.Max(25f, Math.Max(lh, rh) + 9);
                if (render)
                {
                    g.DrawRectangle(pen, area.Left, y, leftWidth, h);
                    g.DrawRectangle(pen, area.Left + leftWidth, y, rightWidth, h);
                    g.DrawString(caption, label, Brushes.Black,
                        new RectangleF(area.Left + 5, y + 4,
                            leftWidth - 10, h - 7), format);
                    g.DrawString(value, text, Brushes.Black,
                        new RectangleF(area.Left + leftWidth + 6, y + 4,
                            rightWidth - 12, h - 7), format);
                }
                y += h;
            }

            string headline = single ? "ИНДИВИДУАЛЬНЫЙ ТЕСТ-ЛИСТ"
                : document.Title;
            float titleHeight = g.MeasureString(headline, title,
                Math.Max(70, (int)area.Width)).Height + 7;
            if (render)
                g.DrawString(headline, title, Brushes.Black,
                    new RectangleF(area.Left, y, area.Width, titleHeight));
            y += titleHeight;

            if (!single && page == 1 && header.Detailed)
            {
                Field("Проект", header.Project);
                Field("Версия ПО", document.Version);
                Field("Окружение", ChecklistPrintLayout.HeaderEnvironment(
                    document, cases));
                Field("Дата тестирования", document.Date.ToString("dd.MM.yyyy"));
                Field("Исполнитель", document.Tester);
                Field("Цель тестирования", header.Purpose);
                Heading("ОБЪЁМ ТЕСТИРОВАНИЯ");
                foreach (var row in header.ScopeRows())
                    Field(row.Caption, row.Value);
                Heading("СПЕЦИАЛЬНО РАЗРАБОТАННОЕ АВТОМАТИЗИРОВАННОЕ СРЕДСТВО ТЕСТИРОВАНИЯ");
                foreach (var row in header.ToolRows())
                    Field(row.Caption, row.Value);
                float metaHeight = text.GetHeight(g) + 10;
                if (render)
                    g.DrawString("Проход: " + runId + "   •   Лист " + page,
                        text, Brushes.DimGray,
                        new RectangleF(area.Left, y + 3, area.Width, metaHeight));
                y += metaHeight;
            }
            else
            {
                string metadata = $"Проект: {header.Project}   •   ОС: {operatingSystem}" +
                    $"   •   Версия: {document.Version}";
                string subline = $"Проход: {runId}   •   Дата: {document.Date:dd.MM.yyyy}" +
                    $"   •   Исполнитель: {document.Tester}";
                foreach (string line in new[] { metadata, subline })
                {
                    float height = g.MeasureString(line, text,
                        Math.Max(70, (int)area.Width)).Height + 5;
                    if (render)
                        g.DrawString(line, text, Brushes.Black,
                            new RectangleF(area.Left, y, area.Width, height));
                    y += height;
                }
                if (single && !string.IsNullOrWhiteSpace(header.Purpose))
                    Field("Цель тестирования", header.Purpose);
            }
            return y + 11;
        }

        private static void DrawChecklistPage(PrintPageEventArgs e,
            ChecklistDocument document, IReadOnlyList<ChecklistCase> cases,
            PageSpan span, int logicalPage)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float y = DrawHeader(g, area, document, cases,
                logicalPage, false, render: true);
            using var font = new Font("Segoe UI", 8.5f);
            using var bold = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            using var foot = new Font("Segoe UI", 7.5f);
            using var pen = new Pen(Color.FromArgb(170, 170, 170));
            using var format = new StringFormat
                { Trimming = StringTrimming.EllipsisWord };
            float[] widths = ChecklistPrintLayout.ListColumnRatios
                .Select(r => r * area.Width).ToArray();
            float[] xs = new float[widths.Length];
            xs[0] = area.Left;
            for (int i = 1; i < widths.Length; i++)
                xs[i] = xs[i - 1] + widths[i - 1];

            const float headingHeight = 35;
            for (int i = 0; i < widths.Length; i++)
            {
                var rect = new RectangleF(xs[i], y, widths[i], headingHeight);
                g.FillRectangle(Brushes.Gainsboro, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawString(ChecklistPrintLayout.ListColumns[i], bold,
                    Brushes.Black, new RectangleF(rect.X + 3, rect.Y + 4,
                        rect.Width - 6, rect.Height - 7), format);
            }
            y += headingHeight;
            for (int index = span.Start; index < span.End; index++)
            {
                ChecklistCase item = cases[index];
                string[] cells =
                {
                    ChecklistPrintLayout.NumberInRun(document, item).ToString("000"),
                    ChecklistPrintLayout.Section(item),
                    ChecklistPrintLayout.Inspection(item),
                    ""
                };
                float height = RowHeight(g, font, widths, document, item,
                    Math.Max(52, area.Height - 190));
                for (int i = 0; i < cells.Length; i++)
                {
                    var rect = new RectangleF(xs[i], y, widths[i], height);
                    g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
                    if (cells[i].Length > 0)
                        g.DrawString(cells[i], font, Brushes.Black,
                            new RectangleF(rect.X + 4, rect.Y + 5,
                                rect.Width - 8, rect.Height - 10), format);
                }
                y += height;
            }

            g.DrawString($"Страница {logicalPage}  •  {cases.Count} проверок" +
                "  •  № совпадает с номером в MKtest",
                foot, Brushes.DimGray, new RectangleF(area.Left,
                    area.Bottom - 19, area.Width, 18));
        }

        private static void PrintCard(PrintPageEventArgs e,
            ChecklistDocument document, ChecklistCase item)
        {
            Graphics g = e.Graphics!;
            Rectangle area = e.MarginBounds;
            float left = area.Left;
            float y = DrawHeader(g, area, document,
                new[] { item }, 1, true, render: true);
            float width = area.Width;
            using var normal = new Font("Segoe UI", 10);
            using var heading = new Font("Segoe UI", 10, FontStyle.Bold);
            using var border = new Pen(Color.LightGray);

            void Field(string caption, string value, float height)
            {
                var rect = new RectangleF(left, y, width, height);
                g.DrawRectangle(border, rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawString(caption, heading, Brushes.Black,
                    new RectangleF(left + 8, y + 6, width - 16, 21));
                if (value.Length > 0)
                    g.DrawString(value, normal, Brushes.Black,
                        new RectangleF(left + 8, y + 29, width - 16, height - 36));
                y += height;
            }

            Field("№ проверки",
                ChecklistPrintLayout.NumberInRun(document, item).ToString("000"), 52);
            Field("Раздел / алгоритм", ChecklistPrintLayout.Section(item), 70);
            if (!string.IsNullOrWhiteSpace(item.Module))
                Field("Сценарий / протокол", item.Module, 70);
            Field("Проверка / действия", item.Action, 155);
            Field("Результат проверки (отметить вручную)", "", 85);
            Field("Номер бага / замечания", "", 150);
            e.HasMorePages = false;
        }
    }
}
