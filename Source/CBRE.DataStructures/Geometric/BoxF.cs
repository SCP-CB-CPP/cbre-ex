using CBRE.DataStructures.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.DataStructures.Geometric
{
    public record struct BoxF(CoordinateF Start, CoordinateF End)
    {
        public readonly static BoxF Empty = new BoxF(CoordinateF.Zero, CoordinateF.Zero);

        public CoordinateF Center { get; private set; } = (Start + End) / 2f;

        /// <summary>
        /// The X value difference of this box
        /// </summary>
        public float Width
        {
            get { return End.X - Start.X; }
        }

        /// <summary>
        /// The Y value difference of this box
        /// </summary>
        public float Length
        {
            get { return End.Y - Start.Y; }
        }

        /// <summary>
        /// The Z value difference of this box
        /// </summary>
        public float Height
        {
            get { return End.Z - Start.Z; }
        }

        public CoordinateF Dimensions
        {
            get { return new CoordinateF(Width, Length, Height); }
        }

        public static BoxF FromPoints(IEnumerable<CoordinateF> coordinates) {
            if (!coordinates.Any())
            {
                throw new Exception("Cannot create a bounding box out of zero coordinates.");
            }
            CoordinateF min = new CoordinateF(float.MaxValue, float.MaxValue, float.MaxValue);
            CoordinateF max = new CoordinateF(float.MinValue, float.MinValue, float.MinValue);
            foreach (CoordinateF vertex in coordinates)
            {
                min.X = Math.Min(vertex.X, min.X);
                min.Y = Math.Min(vertex.Y, min.Y);
                min.Z = Math.Min(vertex.Z, min.Z);
                max.X = Math.Max(vertex.X, max.X);
                max.Y = Math.Max(vertex.Y, max.Y);
                max.Z = Math.Max(vertex.Z, max.Z);
            }
            
            return new(min, max);
        }

        public static BoxF FromBoxes(IEnumerable<BoxF> boxes) {
            if (!boxes.Any())
            {
                throw new Exception("Cannot create a bounding box out of zero other boxes.");
            }
            CoordinateF min = new CoordinateF(float.MaxValue, float.MaxValue, float.MaxValue);
            CoordinateF max = new CoordinateF(float.MinValue, float.MinValue, float.MinValue);
            foreach (BoxF box in boxes)
            {
                min.X = Math.Min(box.Start.X, min.X);
                min.Y = Math.Min(box.Start.Y, min.Y);
                min.Z = Math.Min(box.Start.Z, min.Z);
                max.X = Math.Max(box.End.X, max.X);
                max.Y = Math.Max(box.End.Y, max.Y);
                max.Z = Math.Max(box.End.Z, max.Z);
            }
            
            return new(min, max);
        }

        public bool IsEmpty()
        {
            return Width == 0 && Height == 0 && Length == 0;
        }
        public IEnumerable<CoordinateF> GetBoxPoints()
        {
            yield return new CoordinateF(Start.X, End.Y, End.Z);
            yield return End;
            yield return new CoordinateF(Start.X, Start.Y, End.Z);
            yield return new CoordinateF(End.X, Start.Y, End.Z);

            yield return new CoordinateF(Start.X, End.Y, Start.Z);
            yield return new CoordinateF(End.X, End.Y, Start.Z);
            yield return Start;
            yield return new CoordinateF(End.X, Start.Y, Start.Z);
        }

        public PlaneF[] GetBoxPlanes()
        {
            PlaneF[] planes = new PlaneF[6];
            CoordinateF[][] faces = GetBoxFaces();
            for (int i = 0; i < 6; i++)
            {
                planes[i] = new PlaneF(faces[i][0], faces[i][1], faces[i][2]);
            }
            return planes;
        }

        public CoordinateF[][] GetBoxFaces()
        {
            CoordinateF topLeftBack = new CoordinateF(Start.X, End.Y, End.Z);
            CoordinateF topRightBack = End;
            CoordinateF topLeftFront = new CoordinateF(Start.X, Start.Y, End.Z);
            CoordinateF topRightFront = new CoordinateF(End.X, Start.Y, End.Z);

            CoordinateF bottomLeftBack = new CoordinateF(Start.X, End.Y, Start.Z);
            CoordinateF bottomRightBack = new CoordinateF(End.X, End.Y, Start.Z);
            CoordinateF bottomLeftFront = Start;
            CoordinateF bottomRightFront = new CoordinateF(End.X, Start.Y, Start.Z);
            return new[]
                       {
                           new[] {topLeftFront, topRightFront, bottomRightFront, bottomLeftFront},
                           new[] {topRightBack, topLeftBack, bottomLeftBack, bottomRightBack},
                           new[] {topLeftBack, topLeftFront, bottomLeftFront, bottomLeftBack},
                           new[] {topRightFront, topRightBack, bottomRightBack, bottomRightFront},
                           new[] {topLeftBack, topRightBack, topRightFront, topLeftFront},
                           new[] {bottomLeftFront, bottomRightFront, bottomRightBack, bottomLeftBack}
                       };
        }

        public IEnumerable<LineF> GetBoxLines()
        {
            CoordinateF topLeftBack = new CoordinateF(Start.X, End.Y, End.Z);
            CoordinateF topRightBack = End;
            CoordinateF topLeftFront = new CoordinateF(Start.X, Start.Y, End.Z);
            CoordinateF topRightFront = new CoordinateF(End.X, Start.Y, End.Z);

            CoordinateF bottomLeftBack = new CoordinateF(Start.X, End.Y, Start.Z);
            CoordinateF bottomRightBack = new CoordinateF(End.X, End.Y, Start.Z);
            CoordinateF bottomLeftFront = Start;
            CoordinateF bottomRightFront = new CoordinateF(End.X, Start.Y, Start.Z);

            yield return new LineF(topLeftBack, topRightBack);
            yield return new LineF(topLeftFront, topRightFront);
            yield return new LineF(topLeftBack, topLeftFront);
            yield return new LineF(topRightBack, topRightFront);

            yield return new LineF(topLeftBack, bottomLeftBack);
            yield return new LineF(topLeftFront, bottomLeftFront);
            yield return new LineF(topRightBack, bottomRightBack);
            yield return new LineF(topRightFront, bottomRightFront);

            yield return new LineF(bottomLeftBack, bottomRightBack);
            yield return new LineF(bottomLeftFront, bottomRightFront);
            yield return new LineF(bottomLeftBack, bottomLeftFront);
            yield return new LineF(bottomRightBack, bottomRightFront);
        }

        /// <summary>
        /// Returns true if this box overlaps the given box in any way
        /// </summary>
        public bool IntersectsWith(BoxF that)
        {
            if (Start.X >= that.End.X) return false;
            if (that.Start.X >= End.X) return false;

            if (Start.Y >= that.End.Y) return false;
            if (that.Start.Y >= End.Y) return false;

            if (Start.Z >= that.End.Z) return false;
            if (that.Start.Z >= End.Z) return false;

            return true;
        }

        /// <summary>
        /// Returns true if this box is completely inside the given box
        /// </summary>
        public bool ContainedWithin(BoxF that)
        {
            if (Start.X < that.Start.X) return false;
            if (Start.Y < that.Start.Y) return false;
            if (Start.Z < that.Start.Z) return false;

            if (End.X > that.End.X) return false;
            if (End.Y > that.End.Y) return false;
            if (End.Z > that.End.Z) return false;

            return true;
        }

        /* http://www.gamedev.net/community/forums/topic.asp?topic_id=338987 */
        /// <summary>
        /// Returns true if this box intersects the given line
        /// </summary>
        public bool IntersectsWith(LineF that)
        {
            CoordinateF start = that.Start;
            CoordinateF finish = that.End;

            if (start.X < Start.X && finish.X < Start.X) return false;
            if (start.X > End.X && finish.X > End.X) return false;

            if (start.Y < Start.Y && finish.Y < Start.Y) return false;
            if (start.Y > End.Y && finish.Y > End.Y) return false;

            if (start.Z < Start.Z && finish.Z < Start.Z) return false;
            if (start.Z > End.Z && finish.Z > End.Z) return false;

            CoordinateF d = (finish - start) / 2;
            CoordinateF e = (End - Start) / 2;
            CoordinateF c = start + d - ((Start + End) / 2);
            CoordinateF ad = d.Absolute();

            if (Math.Abs(c.X) > e.X + ad.X) return false;
            if (Math.Abs(c.Y) > e.Y + ad.Y) return false;
            if (Math.Abs(c.Z) > e.Z + ad.Z) return false;

            CoordinateF dca = d.Cross(c).Absolute();

            if (dca.X > e.Y * ad.Z + e.Z * ad.Y) return false;
            if (dca.Y > e.Z * ad.X + e.X * ad.Z) return false;
            if (dca.Z > e.X * ad.Y + e.Y * ad.X) return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given coordinate is inside this box.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool CoordinateIsInside(CoordinateF c)
        {
            return c.X >= Start.X && c.Y >= Start.Y && c.Z >= Start.Z
                   && c.X <= End.X && c.Y <= End.Y && c.Z <= End.Z;
        }

        public BoxF Transform(IUnitTransformation transform)
        {
            return BoxF.FromPoints(GetBoxPoints().Select(transform.Transform));
        }
    }
}
