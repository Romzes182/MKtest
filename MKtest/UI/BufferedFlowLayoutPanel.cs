using System.Windows.Forms;

namespace MKtest.UI
{
    /// <summary>
    /// Double-buffered scroll host for collapsible WinForms panels.
    /// Reduces flicker when a large panel introduces the vertical scrollbar.
    /// </summary>
    public sealed class BufferedFlowLayoutPanel : FlowLayoutPanel
    {
        public BufferedFlowLayoutPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint, true);
        }
    }
}
