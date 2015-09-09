using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CgmInfoGui.Visuals
{
    public class LineVisual : VisualBase
    {
        public LineVisual(Point[] points)
        {
            Points = points;
        }
        public Point[] Points { get; private set; }

        protected internal override void DrawTo(DrawingContext drawingContext)
        {
            var geo = new StreamGeometry();
            using (var ctx = geo.Open())
            {
                ctx.BeginFigure(Points[0], false, false);
                ctx.PolyLineTo(Points.Skip(1).ToList(), true, false);
            }
            geo.Freeze();
            drawingContext.DrawGeometry(null, GetBlack(), geo);
        }
    }
}