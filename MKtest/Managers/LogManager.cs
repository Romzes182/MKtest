using MKtest.Services;
using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class LogManager
    {
        private TextBox _logTextBox;
        private LoggerService _loggerService;

        public LogManager(TextBox logTextBox)
        {
            _logTextBox = logTextBox;
            _loggerService = new LoggerService();
            SetupLogTextBox();
        }

        private void SetupLogTextBox()
        {
            _logTextBox.ReadOnly = true;
            _logTextBox.ScrollBars = ScrollBars.Vertical;
            _logTextBox.Font = new System.Drawing.Font("Consolas", 9);
        }

        public void AppendLog(string message)
        {
            // Записываем в файл через LoggerService
            _loggerService.Log(message);

            // Выводим в TextBox
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action(() =>
                {
                    _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                    _logTextBox.ScrollToCaret();
                }));
            }
            else
            {
                _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
                _logTextBox.ScrollToCaret();
            }
        }

        public void ClearLog()
        {
            // Очищаем лог в файле (создаем новый файл)
            _loggerService.Log("--- Лог очищен ---");

            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action(() => _logTextBox.Clear()));
            }
            else
            {
                _logTextBox.Clear();
            }
        }

        // Метод для получения пути к файлу лога (может пригодиться)
        public string GetLogFilePath()
        {
            // LoggerService сохраняет логи в папке logs
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        }
    }
}