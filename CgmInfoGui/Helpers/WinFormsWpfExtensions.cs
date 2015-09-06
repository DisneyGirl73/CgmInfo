using WinFormsPointF = System.Drawing.PointF;
using WpfPoint = System.Windows.Point;

namespace CgmInfoGui
{
    public static class WinFormsWpfExtensions
    {
        public static WpfPoint ToPoint(this WinFormsPointF pointF)
        {
            return new WpfPoint(pointF.X, pointF.Y);
        }
    }
}
