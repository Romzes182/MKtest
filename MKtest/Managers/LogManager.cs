using System;
using System.Windows.Forms;

namespace MKtest.Managers
{
    public class LogManager
    {
        private TextBox _logTextBox;

        public LogManager(TextBox logTextBox)
        {
            _logTextBox = logTextBox;
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
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action(() => _logTextBox.Clear()));
            }
            else
            {
                _logTextBox.Clear();
            }
        }
    }
}