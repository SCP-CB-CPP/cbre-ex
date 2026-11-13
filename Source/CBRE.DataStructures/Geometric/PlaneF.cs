using System;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.Geometric
{
    /// <summary>
    /// Defines a plane in the form Ax + By + Cz + D = 0
    /// </summary>
    public struct PlaneF
    {
        public CoordinateF Normal { get; private set; }
        public float DistanceFromOrigin { get; private set; }
        public float A { get; private set; }
        public float B { get; private set; }
        public float C { get; private set; }
        public float D { get; private set; }
        public CoordinateF PointOnPlane { get; private set; }

        public PlaneF(CoordinateF p1, CoordinateF p2, CoordinateF p3)
        {
            CoordinateF ab = p2 - p1;
            CoordinateF ac = p3 - p1;

            Normal = ac.Cross(ab).Normalise();
            DistanceFromOrigin = Normal.Dot(p1);
            PointOnPlane = p1;

            A = Normal.X;
            B = Normal.Y;
            C = Normal.Z;
            D = -DistanceFromOrigin;
        }

        public PlaneF(Plane p)
        {
            Normal = new CoordinateF(p.Normal);
            DistanceFromOrigin = (float)p.DistanceFromOrigin;
            PointOnPlane = new CoordinateF(p.PointOnPlane);

            A = Normal.X;
            B = Normal.Y;
            C = Normal.Z;
            D = -DistanceFromOrigin;
        }

        public PlaneF(CoordinateF norm, CoordinateF pointOnPlane)
        {
            Normal = norm.Normalise();
            DistanceFromOrigin = Normal.Dot(pointOnPlane);
            PointOnPlane = pointOnPlane;

            A = Normal.X;
            B = Normal.Y;
            C = Normal.Z;
            D = -DistanceFromOrigin;
        }

        public PlaneF(CoordinateF norm, float distanceFromOrigin)
        {
            Normal = norm.Normalise();
            DistanceFromOrigin = distanceFromOrigin;
            PointOnPlane = Normal * DistanceFromOrigin;

            A = Normal.X;
            B = Normal.Y;
            C = Normal.Z;
            D = -DistanceFromOrigin;
        }

        ///  <summary>Finds if the given point is above, below, or on the plane.</summary>
        ///  <param name="co">The coordinate to test</param>
        /// <param name="epsilon">Tolerance value</param>
        /// <returns>
        ///  value == -1 if coordinate is below the plane<br />
        ///  value == 1 if coordinate is above the plane<br />
        ///  value == 0 if coordinate is on the plane.
        /// </returns>
        public int OnPlane(CoordinateF co, float epsilon = 0.5f)
        {
            //eval (s = Ax + By + Cz + D) at point (x,y,z)
            //if s > 0 then point is "above" the plane (same side as normal)
            //if s < 0 then it lies on the opposite side
            //if s = 0 then the point (x,y,z) lies on the plane
            float res = EvalAtPoint(co);
            if (Math.Abs(res) < epsilon) return 0;
            if (res < 0) return -1;
            return 1;
        }

        /// <summary>
        /// Gets the point that the line intersects with this plane.
        /// </summary>
        /// <param name="line">The line to intersect with</param>
        /// <param name="ignoreDirection">Set to true to ignore the direction
        /// of the plane and line when intersecting. Defaults to false.</param>
        /// <param name="ignoreSegment">Set to true to ignore the start and
        /// end points of the line in the intersection. Defaults to false.</param>
        /// <returns>True if the line intersects with the plane, false otherwise</returns>
        public bool GetIntersectionPoint(LineF line, out CoordinateF intersectPoint, bool ignoreDirection = false, bool ignoreSegment = false)
        {
            // http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm#Line%20Intersections
            // http://paulbourke.net/geometry/planeline/
            intersectPoint = default;
            
            CoordinateF dir = line.End - line.Start;
            float denominator = -Normal.Dot(dir);
            if (Math.Abs(denominator) < 0.00001f || (!ignoreDirection && denominator < 0)) return false;
            float numerator = Normal.Dot(line.Start - Normal * DistanceFromOrigin);
            float u = numerator / denominator;
            if (!ignoreSegment && u is < 0 or > 1) return false;
            intersectPoint = line.Start + u * dir;
            return true;
        }

        /// <summary>
        /// Project a point into the space of this plane. I.e. Get the point closest
        /// to the provided point that is on this plane.
        /// </summary>
        /// <param name="point">The point to project</param>
        /// <returns>The point projected onto this plane</returns>
        public CoordinateF Project(CoordinateF point)
        {
            // http://www.gamedev.net/topic/262196-projecting-vector-onto-a-plane/
            // Projected = Point - ((Point - PointOnPlane) . Normal) * Normal
            return point - ((point - PointOnPlane).Dot(Normal)) * Normal;
        }

        public float EvalAtPoint(CoordinateF co)
        {
            return A * co.X + B * co.Y + C * co.Z + D;
        }

        /// <summary>
        /// Gets the axis closest to the normal of this plane
        /// </summary>
        /// <returns>CoordinateF.UnitX, CoordinateF.UnitY, or CoordinateF.UnitZ depending on the plane's normal</returns>
        public CoordinateF GetClosestAxisToNormal()
        {
            // VHE prioritises the axes in order of X, Y, Z.
            CoordinateF norm = Normal.Absolute();

            if (norm.X >= norm.Y && norm.X >= norm.Z) return CoordinateF.UnitX;
            if (norm.Y >= norm.Z) return CoordinateF.UnitY;
            return CoordinateF.UnitZ;
        }

        /// <summary>
        /// Intersects three planes and gets the point of their intersection.
        /// </summary>
        /// <returns>The point that the planes intersect at, or null if they do not intersect at a point.</returns>
        public static bool Intersect(PlaneF p1, PlaneF p2, PlaneF p3, out CoordinateF intersectPoint)
        {
            // http://paulbourke.net/geometry/3planes/
            intersectPoint = default;

            CoordinateF c1 = p2.Normal.Cross(p3.Normal);
            CoordinateF c2 = p3.Normal.Cross(p1.Normal);
            CoordinateF c3 = p1.Normal.Cross(p2.Normal);

            float denom = p1.Normal.Dot(c1);
            if (denom < 0.00001f) return false; // No intersection, planes must be parallel

            CoordinateF numer = (-p1.D * c1) + (-p2.D * c2) + (-p3.D * c3);
            intersectPoint = numer / denom;
            return true;
        }

        public bool EquivalentTo(PlaneF other, float delta = 0.0001f)
        {
            return Normal.EquivalentTo(other.Normal, delta)
                   && Math.Abs(DistanceFromOrigin - other.DistanceFromOrigin) < delta;
        }

        public bool Equals(PlaneF other)
        {
            return Equals(other.Normal, Normal) && other.DistanceFromOrigin == DistanceFromOrigin;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(PlaneF)) return false;
            return Equals((PlaneF)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Normal != null ? Normal.GetHashCode() : 0) * 397) ^ DistanceFromOrigin.GetHashCode();
            }
        }

        public static bool operator ==(PlaneF left, PlaneF right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PlaneF left, PlaneF right)
        {
            return !Equals(left, right);
        }
    }
}
