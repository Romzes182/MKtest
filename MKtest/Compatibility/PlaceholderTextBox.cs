using System;
using System.Drawing;
using System.Windows.Forms;

namespace MKtest.Compatibility
{
    // .NET Framework's native TextBox lacks PlaceholderText. Paint the hint
    // separately so it never becomes user input, including in multiline fields.
    internal sealed class PlaceholderTextBox : TextBox
    {
        private string _placeholderText = string.Empty;
        public string PlaceholderText
        {
            get => _placeholderText;
            set { _placeholderText = value ?? string.Empty; Invalidate(); }
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            const int WmPaint = 0x000F;
            if (m.Msg != WmPaint || Focused || TextLength != 0 || _placeholderText.Length == 0)
                return;

            using var graphics = Graphics.FromHwnd(Handle);
            var bounds = ClientRectangle;
            bounds.Inflate(-1, -1);
            var flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding |
                (Multiline ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine);
            TextRenderer.DrawText(graphics, _placeholderText, Font, bounds, SystemColors.GrayText, flags);
        }
    }
}
