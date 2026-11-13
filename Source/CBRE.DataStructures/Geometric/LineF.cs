using CBRE.DataStructures.Transformations;
using System;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.Geometric
{
    public record struct LineF(CoordinateF Start, CoordinateF End)
    {
        public readonly static LineF AxisX = new LineF(CoordinateF.Zero, CoordinateF.UnitX);
        public readonly static LineF AxisY = new LineF(CoordinateF.Zero, CoordinateF.UnitY);
        public static readonly LineF AxisZ = new LineF(CoordinateF.Zero, CoordinateF.UnitZ);

        public LineF Reverse()
        {
            return new LineF(End, Start);
        }

        public CoordinateF ClosestPoint(CoordinateF point)
        {
            // http://paulbourke.net/geometry/pointline/

            CoordinateF delta = End - Start;
            float den = delta.LengthSquared();
            if (den == 0) return Start; // Start and End are the same

            CoordinateF numPoint = (point - Start).ComponentMultiply(delta);
            float num = numPoint.X + numPoint.Y + numPoint.Z;
            float u = num / den;

            if (u < 0) return Start; // Point is before the segment start
            if (u > 1) return End;   // Point is after the segment end
            return Start + u * delta;
        }

        /// <summary>
        /// Determines if this line is behind, in front, or spanning a plane.
        /// </summary>
        /// <param name="p">The plane to test against</param>
        /// <returns>A PlaneClassification value.</returns>
        public PlaneClassification ClassifyAgainstPlane(PlaneF p)
        {
            int start = p.OnPlane(Start);
            int end = p.OnPlane(End);

            if (start == 0 && end == 0) return PlaneClassification.OnPlane;
            if (start <= 0 && end <= 0) return PlaneClassification.Back;
            if (start >= 0 && end >= 0) return PlaneClassification.Front;
            return PlaneClassification.Spanning;
        }

        public LineF Transform(IUnitTransformation transform)
        {
            return new LineF(transform.Transform(Start), transform.Transform(End));
        }

        public bool EquivalentTo(LineF other, float delta = 0.0001f)
        {
            return (Start.EquivalentTo(other.Start, delta) && End.EquivalentTo(other.End, delta))
                   || (End.EquivalentTo(other.Start, delta) && Start.EquivalentTo(other.End, delta));
        }
    }
}
