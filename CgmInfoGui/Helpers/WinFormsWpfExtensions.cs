using System.Linq;
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
        public static WpfPoint[] ToPoints(this WinFormsPointF[] pointsF)
        {
            return pointsF.Select(p => p.ToPoint()).ToArray();
        }
    }
}
