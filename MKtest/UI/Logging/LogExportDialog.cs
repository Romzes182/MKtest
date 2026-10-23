using MKtest.Managers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKtest.UI.Logging
{
    /// <summary>
    /// Exports the complete current log from disk rather than the bounded
    /// or temporarily paused TextBox. The dialog stays on the UI thread,
    /// copying the file takes place asynchronously.
    /// </summary>
    public static class LogExportDialog
    {
        public static async Task SaveAsync(
            IWin32Window? owner, LogManager log, string sectionName)
        {
            ArgumentNullException.ThrowIfNull(log);

            string name = new string(sectionName.Select(ch =>
                Path.GetInvalidFileNameChars().Contains(ch) || char.IsControl(ch)
                    ? '_' : ch).ToArray());

            using var dialog = new SaveFileDialog
            {
                Title = "Сохранить журнал MKtest",
                Filter = "Текстовый журнал (*.txt)|*.txt",
                DefaultExt = "txt",
                AddExtension = true,
                OverwritePrompt = true,
                FileName = $"MKtest_{name}_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            DialogResult selection = owner == null
                ? dialog.ShowDialog()
                : dialog.ShowDialog(owner);
            if (selection != DialogResult.OK)
                return;

            string destination = dialog.FileName;
            string source = log.GetCurrentSessionLogFile();

            try
            {
                if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination),
                        StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Нельзя перезаписать исходный журнал.");

                await Task.Run(async () =>
                {
                    await using var input = new FileStream(
                        source, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite, 81920, FileOptions.Asynchronous);
                    await using var output = new FileStream(
                        destination, FileMode.Create, FileAccess.Write,
                        FileShare.None, 81920, FileOptions.Asynchronous);
                    await input.CopyToAsync(output);
                });

                MessageBox.Show(owner,
                    $"Журнал сохранён:\n{destination}",
                    "Экспорт журнала", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException
                                       or UnauthorizedAccessException
                                       or ArgumentException
                                       or NotSupportedException)
            {
                MessageBox.Show(owner,
                    $"Не удалось сохранить журнал:\n{ex.Message}",
                    "Ошибка экспорта", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
