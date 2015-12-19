using System.Collections.Generic;
using System.Linq;
using CgmInfo.Commands.Enums;
using CgmInfo.Commands.GraphicalPrimitives;
using CgmInfo.Utilities;
using PointF = System.Drawing.PointF;

namespace CgmInfo.TextEncoding
{
    internal static class GraphicalPrimitiveReader
    {
        public static Polyline Polyline(MetafileReader reader)
        {
            var points = ReadPointList(reader);
            return new Polyline(points.ToArray());
        }

        public static Polyline IncrementalPolyline(MetafileReader reader)
        {
            var points = ReadIncrementalPointList(reader);
            return new Polyline(points.ToArray());
        }

        public static DisjointPolyline DisjointPolyline(MetafileReader reader)
        {
            var points = ReadPointList(reader);
            return new DisjointPolyline(points.ToArray());
        }

        public static DisjointPolyline IncrementalDisjointPolyline(MetafileReader reader)
        {
            var points = ReadIncrementalPointList(reader);
            return new DisjointPolyline(points.ToArray());
        }

        public static Polymarker Polymarker(MetafileReader reader)
        {
            var points = ReadPointList(reader);
            return new Polymarker(points.ToArray());
        }

        public static Polymarker IncrementalPolymarker(MetafileReader reader)
        {
            var points = ReadIncrementalPointList(reader);
            return new Polymarker(points.ToArray());
        }

        public static TextCommand Text(MetafileReader reader)
        {
            return new TextCommand(reader.ReadPoint(), ParseFinalFlag(reader.ReadEnum()), reader.ReadString());
        }

        public static RestrictedText RestrictedText(MetafileReader reader)
        {
            return new RestrictedText(reader.ReadVdc(), reader.ReadVdc(), reader.ReadPoint(), ParseFinalFlag(reader.ReadEnum()), reader.ReadString());
        }

        public static AppendText AppendText(MetafileReader reader)
        {
            return new AppendText(ParseFinalFlag(reader.ReadEnum()), reader.ReadString());
        }

        public static Polygon Polygon(MetafileReader reader)
        {
            var points = new List<PointF>();
            while (reader.HasMoreData(2))
            {
                points.Add(reader.ReadPoint());
            }
            return new Polygon(points.ToArray());
        }

        public static Polygon IncrementalPolygon(MetafileReader reader)
        {
            var points = new List<PointF>();
            var lastPoint = reader.ReadPoint();
            points.Add(lastPoint);
            while (reader.HasMoreData(2))
            {
                double deltaX = reader.ReadVdc();
                double deltaY = reader.ReadVdc();
                lastPoint = new PointF((float)(lastPoint.X + deltaX), (float)(lastPoint.Y + deltaY));
                points.Add(lastPoint);
            }
            return new Polygon(points.ToArray());
        }

        public static PolygonSet PolygonSet(MetafileReader reader)
        {
            var points = new List<PointF>();
            var flags = new List<EdgeOutFlags>();
            while (reader.HasMoreData(3))
            {
                points.Add(reader.ReadPoint());
                flags.Add(ParseEdgeOutFlags(reader.ReadEnum()));
            }
            return new PolygonSet(points.ToArray(), flags.ToArray());
        }

        public static PolygonSet IncrementalPolygonSet(MetafileReader reader)
        {
            var points = new List<PointF>();
            var flags = new List<EdgeOutFlags>();
            var lastPoint = reader.ReadPoint();
            points.Add(lastPoint);
            flags.Add(ParseEdgeOutFlags(reader.ReadEnum()));
            while (reader.HasMoreData(3))
            {
                double deltaX = reader.ReadVdc();
                double deltaY = reader.ReadVdc();
                lastPoint = new PointF((float)(lastPoint.X + deltaX), (float)(lastPoint.Y + deltaY));
                points.Add(lastPoint);
                flags.Add(ParseEdgeOutFlags(reader.ReadEnum()));
            }
            return new PolygonSet(points.ToArray(), flags.ToArray());
        }

        public static CellArray CellArray(MetafileReader reader)
        {
            var p = reader.ReadPoint();
            var q = reader.ReadPoint();
            var r = reader.ReadPoint();

            int nx = reader.ReadInteger();
            int ny = reader.ReadInteger();

            // TODO: not really used in text encoding; but in case we ever need it,
            //       the same check for zero as in binary encoding needs to happen.
            //       intentionally unused until that time comes.
            int localColorPrecision = reader.ReadInteger();

            int totalCount = nx * ny;
            var colors = new List<MetafileColor>();
            while (reader.HasMoreData())
            {
                colors.Add(reader.ReadColor());
            }
            // FIXME: for parenthesized lists, every row is enclosed by parenthesis (which right now are ignored by the parser).
            //        The number of cells between parentheses shall be less than or equal to the row length.
            //        If a row is not complete, then the last defined cell in the row is replicated to fill the row.
            //        Since the parser ignores parenthesis, we can only fill the last row with the last color of all rows;
            //        but not every row with the last color of each row.
            if (colors.Count < totalCount)
            {
                var lastColor = colors.Last();
                colors.AddRange(Enumerable.Range(0, totalCount - colors.Count).Select(i => lastColor));
            }
            return new CellArray(p, q, r, nx, ny, colors.ToArray());
        }

        public static Rectangle Rectangle(MetafileReader reader)
        {
            return new Rectangle(reader.ReadPoint(), reader.ReadPoint());
        }

        public static Circle Circle(MetafileReader reader)
        {
            return new Circle(reader.ReadPoint(), reader.ReadVdc());
        }

        public static CircularArc3Point CircularArc3Point(MetafileReader reader)
        {
            return new CircularArc3Point(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint());
        }

        public static CircularArc3PointClose CircularArc3PointClose(MetafileReader reader)
        {
            return new CircularArc3PointClose(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), ParseArcClosure(reader.ReadEnum()));
        }

        public static CircularArcCenter CircularArcCenter(MetafileReader reader)
        {
            return new CircularArcCenter(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadVdc());
        }

        public static CircularArcCenterClose CircularArcCenterClose(MetafileReader reader)
        {
            return new CircularArcCenterClose(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadVdc(), ParseArcClosure(reader.ReadEnum()));
        }

        public static Ellipse Ellipse(MetafileReader reader)
        {
            return new Ellipse(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint());
        }

        public static EllipticalArc EllipticalArc(MetafileReader reader)
        {
            return new EllipticalArc(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint());
        }

        public static EllipticalArcClose EllipticalArcClose(MetafileReader reader)
        {
            return new EllipticalArcClose(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), ParseArcClosure(reader.ReadEnum()));
        }

        public static CircularArcCenterReversed CircularArcCenterReversed(MetafileReader reader)
        {
            return new CircularArcCenterReversed(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadVdc());
        }

        public static ConnectingEdge ConnectingEdge(MetafileReader reader)
        {
            return new ConnectingEdge();
        }

        public static HyperbolicArc HyperbolicArc(MetafileReader reader)
        {
            // NOTE: Text Encoding does not permit start/end vectors to be encoded as point here (vs. ARCBOUNDS definitions),
            //       but it actually makes sense to store them as such - we can still rely on ReadPoint to read 2x VDC for us
            //       since we ignore parenthesis in the parser (which needs to be fixed in case we ever do).
            return new HyperbolicArc(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint());
        }

        public static ParabolicArc ParabolicArc(MetafileReader reader)
        {
            return new ParabolicArc(reader.ReadPoint(), reader.ReadPoint(), reader.ReadPoint());
        }

        private static FinalFlag ParseFinalFlag(string token)
        {
            // assume not final; unless its final
            if (token.ToUpperInvariant() == "FINAL")
                return FinalFlag.Final;
            return FinalFlag.NotFinal;
        }

        private static List<PointF> ReadPointList(MetafileReader reader)
        {
            var points = new List<PointF>();
            while (reader.HasMoreData(2))
            {
                points.Add(reader.ReadPoint());
            }

            return points;
        }
        private static List<PointF> ReadIncrementalPointList(MetafileReader reader)
        {
            var points = new List<PointF>();
            var lastPoint = reader.ReadPoint();
            points.Add(lastPoint);
            while (reader.HasMoreData(2))
            {
                double deltaX = reader.ReadVdc();
                double deltaY = reader.ReadVdc();
                lastPoint = new PointF((float)(lastPoint.X + deltaX), (float)(lastPoint.Y + deltaY));
                points.Add(lastPoint);
            }

            return points;
        }

        private static EdgeOutFlags ParseEdgeOutFlags(string token)
        {
            // assume invisible unless its any of the others
            token = token.ToUpperInvariant();
            if (token == "VIS")
                return EdgeOutFlags.Visible;
            else if (token == "CLOSEINVIS")
                return EdgeOutFlags.InvisibleClose;
            else if (token == "CLOSEVIS")
                return EdgeOutFlags.VisibleClose;
            return EdgeOutFlags.Invisible;
        }

        private static ArcClosureType ParseArcClosure(string token)
        {
            // assume pie unless its chord
            if (token.ToUpperInvariant() == "CHORD")
                return ArcClosureType.Chord;
            return ArcClosureType.Pie;
        }
    }
}
