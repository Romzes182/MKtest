using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MKtest.UI.Logging
{
    /// <summary>
    /// Фоновым службам достаточно поставить сообщение в очередь.
    /// WinForms.Timer пакетно переносит сообщения в TextBox на UI-потоке.
    /// Если поток сообщений чрезмерный, ограничивается только экранная очередь:
    /// дисковый лог сохраняется полностью и независимо.
    /// </summary>
    public sealed class WinFormsLogSink : IUiLogSink
    {
        private const int MaxPendingLines = 3000;
        private const int MaxLinesPerFrame = 300;
        private const int MaxVisibleCharacters = 200_000;
        private const int TrimToCharacters = 120_000;

        private readonly TextBox _textBox;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Queue<string> _pending = new();
        private readonly object _sync = new();
        private int _dropped;
        private bool _disposed;

        public WinFormsLogSink(TextBox textBox)
        {
            _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
            _textBox.ReadOnly = true;
            _textBox.ScrollBars = ScrollBars.Vertical;
            _textBox.Font = new System.Drawing.Font("Consolas", 9);

            // Must be created on the WinForms thread. Timer.Tick is called only there.
            _timer = new System.Windows.Forms.Timer { Interval = 100 };
            _timer.Tick += FlushToUi;
            _timer.Start();
        }

        public void Append(string formattedLine)
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                if (_pending.Count >= MaxPendingLines)
                {
                    _pending.Dequeue();
                    _dropped++;
                }
                _pending.Enqueue(formattedLine);
            }
        }

        private void FlushToUi(object? sender, EventArgs args)
        {
            if (_disposed || _textBox.IsDisposed)
                return;

            var batch = new StringBuilder();
            lock (_sync)
            {
                if (_dropped > 0)
                {
                    batch.AppendLine($"[{DateTime.Now:HH:mm:ss}] " +
                        $"В окне журнала пропущено {_dropped} сообщений из-за нагрузки. " +
                        "Полные записи сохранены в файле.");
                    _dropped = 0;
                }

                for (int i = 0; i < MaxLinesPerFrame && _pending.Count > 0; i++)
                    batch.Append(_pending.Dequeue());
            }

            if (batch.Length == 0)
                return;

            var newText = batch.ToString();

            // Screen text is bounded; historical data remains available in the file.
            if (newText.Length > MaxVisibleCharacters)
                newText = newText.Substring(newText.Length - MaxVisibleCharacters);

            if (_textBox.TextLength + newText.Length > MaxVisibleCharacters)
            {
                string existing = _textBox.Text;
                int toRemove = Math.Max(0,
                    existing.Length + newText.Length - TrimToCharacters);

                if (toRemove >= existing.Length)
                {
                    _textBox.Clear();
                }
                else
                {
                    int boundary = existing.IndexOf('\n', toRemove);
                    _textBox.Text = boundary >= 0
                        ? existing[(boundary + 1)..]
                        : existing[toRemove..];
                }
            }

            _textBox.AppendText(newText);
            _textBox.SelectionStart = _textBox.TextLength;
            _textBox.ScrollToCaret();
        }

        public void Clear()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _pending.Clear();
                _dropped = 0;
            }

            if (_textBox.IsDisposed)
                return;

            if (_textBox.InvokeRequired)
            {
                if (_textBox.IsHandleCreated)
                    _textBox.BeginInvoke(new Action(() => _textBox.Clear()));
                return;
            }

            _textBox.Clear();
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _pending.Clear();
            }

            _timer.Stop();
            _timer.Tick -= FlushToUi;
            _timer.Dispose();
        }
    }
}
