using System;
using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Scroll host with buffered painting and a full redraw after native scrolling.
    /// Native TextBox children otherwise may retain fragments of an earlier frame.
    /// </summary>
    public sealed class BufferedScrollablePanel : Panel
    {
        private bool _repaintQueued;

        public BufferedScrollablePanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            AutoScroll = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            if (_repaintQueued || !IsHandleCreated || IsDisposed)
                return;

            _repaintQueued = true;
            BeginInvoke(new Action(() =>
            {
                _repaintQueued = false;
                if (IsDisposed || !IsHandleCreated) return;
                Invalidate(true);
                Update();
            }));
        }
    }
}
