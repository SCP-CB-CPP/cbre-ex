using CBRE.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.Geometric
{
    [Serializable]
    public class Coordinate : ISerializable
    {
        public readonly static Coordinate MaxValue = new Coordinate(Decimal.MaxValue, Decimal.MaxValue, Decimal.MaxValue);
        public readonly static Coordinate MinValue = new Coordinate(Decimal.MinValue, Decimal.MinValue, Decimal.MinValue);
        public readonly static Coordinate Zero = new Coordinate(0, 0, 0);
        public readonly static Coordinate One = new Coordinate(1, 1, 1);
        public readonly static Coordinate UnitX = new Coordinate(1, 0, 0);
        public readonly static Coordinate UnitY = new Coordinate(0, 1, 0);
        public readonly static Coordinate UnitZ = new Coordinate(0, 0, 1);

        #region X, Y, Z
        private decimal _z;
        private decimal _y;
        private decimal _x;

        private double _dx;
        private double _dy;
        private double _dz;

        public decimal X
        {
            get { return _x; }
            set
            {
                _x = value;
                _dx = (double)value;
            }
        }

        public decimal Y
        {
            get { return _y; }
            set
            {
                _y = value;
                _dy = (double)value;
            }
        }

        public decimal Z
        {
            get { return _z; }
            set
            {
                _z = value;
                _dz = (double)value;
            }
        }

        public double DX
        {
            get { return _dx; }
            set
            {
                _dx = value;
                _x = (decimal)value;
            }
        }

        public double DY
        {
            get { return _dy; }
            set
            {
                _dy = value;
                _y = (decimal)value;
            }
        }

        public double DZ
        {
            get { return _dz; }
            set
            {
                _dz = value;
                _z = (decimal)value;
            }
        }
        #endregion

        public Coordinate(decimal x, decimal y, decimal z)
        {
            _x = x;
            _y = y;
            _z = z;
            _dx = (double)x;
            _dy = (double)y;
            _dz = (double)z;
        }

        public Coordinate(CoordinateF other)
        {
            _x = (decimal)other.X;
            _y = (decimal)other.Y;
            _z = (decimal)other.Z;
            _dx = other.X;
            _dy = other.Y;
            _dy = other.Z;
        }

        protected Coordinate(SerializationInfo info, StreamingContext context)
        {
            X = info.GetDecimal("X");
            Y = info.GetDecimal("Y");
            Z = info.GetDecimal("Z");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);
            info.AddValue("Z", Z);
        }

        public bool EquivalentTo(Coordinate test, decimal delta = 0.0001m)
        {
            decimal xd = Math.Abs(_x - test._x);
            decimal yd = Math.Abs(_y - test._y);
            decimal zd = Math.Abs(_z - test._z);
            return (xd < delta) && (yd < delta) && (zd < delta);
        }

        public bool Equals(Coordinate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EquivalentTo(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Coordinate) && Equals((Coordinate)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = _x.GetHashCode();
                result = (result * 397) ^ _y.GetHashCode();
                result = (result * 397) ^ _z.GetHashCode();
                return result;
            }
        }

        public decimal Dot(Coordinate c)
        {
            return ((_x * c._x) + (_y * c._y) + (_z * c._z));
        }

        public Coordinate Cross(Coordinate that)
        {
            decimal xv = (_y * that._z) - (_z * that._y);
            decimal yv = (_z * that._x) - (_x * that._z);
            decimal zv = (_x * that._y) - (_y * that._x);
            return new Coordinate(xv, yv, zv);
        }

        public Coordinate Round(int num = 8)
        {
            return new Coordinate(Math.Round(_x, num), Math.Round(_y, num), Math.Round(_z, num));
        }

        public Coordinate Snap(decimal snapTo)
        {
            return new Coordinate(
                Math.Round(_x / snapTo) * snapTo,
                Math.Round(_y / snapTo) * snapTo,
                Math.Round(_z / snapTo) * snapTo
            );
        }

        public decimal VectorMagnitude()
        {
            return (decimal)Math.Sqrt(Math.Pow(_dx, 2) + Math.Pow(_dy, 2) + Math.Pow(_dz, 2));
        }

        public decimal LengthSquared()
        {
            return (decimal)(Math.Pow(_dx, 2) + Math.Pow(_dy, 2) + Math.Pow(_dz, 2));
        }

        public Coordinate Normalise()
        {
            decimal len = VectorMagnitude();
            return len == 0 ? new Coordinate(0, 0, 0) : new Coordinate(_x / len, _y / len, _z / len);
        }

        public Coordinate Absolute()
        {
            return new Coordinate(Math.Abs(_x), Math.Abs(_y), Math.Abs(_z));
        }

        public Coordinate XYZ()
        {
            return new Coordinate(_x, _y, _z);
        }

        public Coordinate ZXY()
        {
            return new Coordinate(_z, _x, _y);
        }

        public Coordinate YZX()
        {
            return new Coordinate(_y, _z, _x);
        }

        public Coordinate ZYX()
        {
            return new Coordinate(_z, _y, _x);
        }

        public Coordinate YXZ()
        {
            return new Coordinate(_y, _x, _z);
        }

        public Coordinate XZY()
        {
            return new Coordinate(_x, _z, _y);
        }


        public static bool operator ==(Coordinate c1, Coordinate c2)
        {
            return Equals(c1, null) ? Equals(c2, null) : c1.Equals(c2);
        }

        public static bool operator !=(Coordinate c1, Coordinate c2)
        {
            return Equals(c1, null) ? !Equals(c2, null) : !c1.Equals(c2);
        }

        public static Coordinate operator +(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1._x + c2._x, c1._y + c2._y, c1._z + c2._z);
        }

        public static Coordinate operator -(Coordinate c1, Coordinate c2)
        {
            return new Coordinate(c1._x - c2._x, c1._y - c2._y, c1._z - c2._z);
        }

        public static Coordinate operator -(Coordinate c1)
        {
            return new Coordinate(-c1._x, -c1._y, -c1._z);
        }

        public static Coordinate operator /(Coordinate c, decimal f)
        {
            return f == 0 ? new Coordinate(0, 0, 0) : new Coordinate(c._x / f, c._y / f, c._z / f);
        }

        public static Coordinate operator *(Coordinate c, decimal f)
        {
            return new Coordinate(c._x * f, c._y * f, c._z * f);
        }

        public static Coordinate operator *(decimal f, Coordinate c)
        {
            return c * f;
        }

        public Coordinate ComponentMultiply(Coordinate c)
        {
            return new Coordinate(_x * c._x, _y * c._y, _z * c._z);
        }

        public Coordinate ComponentDivide(Coordinate c)
        {
            decimal x = c._x == 0 ? 1 : c._x;
            decimal y = c._y == 0 ? 1 : c._y;
            decimal z = c._z == 0 ? 1 : c._z;
            return new Coordinate(_x / x, _y / y, _z / z);
        }

        /// <summary>
        /// Treats this vector as a directional unit vector and constructs a euler angle representation of that angle (in radians)
        /// </summary>
        /// <returns></returns>
        public Coordinate ToEulerAngles()
        {
            // http://www.gamedev.net/topic/399701-convert-vector-to-euler-cardan-angles/#entry3651854
            decimal yaw = DMath.Atan2(_y, _x);
            decimal pitch = DMath.Atan2(-_z, DMath.Sqrt(_x * _x + _y * _y));
            return new Coordinate(0, pitch, yaw); // HL FGD has X = roll, Y = pitch, Z = yaw
        }

        public override string ToString()
        {
            return "(" + _x.ToString("0.0000") + " " + _y.ToString("0.0000") + " " + _z.ToString("0.0000") + ")";
        }

        public string ToDataString()
        {
            return DecimalToData(_x) + " " + DecimalToData(_y) + " " + DecimalToData(_z);
        }

        public static string DecimalToData(decimal v)
            {
            v = Math.Round(v, 5);
            string retVal = v.ToString("F7");
            while (retVal.Contains('.') && (retVal.Last() == '0' || retVal.Last() == '.')) {
                retVal = retVal.Substring(0, retVal.Length - 1);
            }
            return retVal;
        }

        public Coordinate Clone()
        {
            return new Coordinate(_x, _y, _z);
        }

        public static Coordinate Parse(string x, string y, string z)
        {
            const NumberStyles ns = NumberStyles.Float;
            return new Coordinate(decimal.Parse(x, ns), decimal.Parse(y, ns), decimal.Parse(z, ns));
        }

        public CoordinateF ToCoordinateF()
        {
            return new CoordinateF((float)_dx, (float)_dy, (float)_dz);
        }
    }


}
