using CBRE.DataStructures.Geometric;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.MapObjects
{
    [Serializable]
    public class Property : ISerializable
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Property()
        {
        }

        protected Property(SerializationInfo info, StreamingContext context)
        {
            Key = info.GetString("Key");
            Value = info.GetString("Value");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Key", Key);
            info.AddValue("Value", Value);
        }

        public Color GetColour(Color defaultIfInvalid)
        {
            string[] spl = Value.Split(' ');
            if (spl.Length < 3 || spl.Length > 4)
            {
                return defaultIfInvalid;
            }
            int r, g, b, a;
            a = 255;
            if (int.TryParse(spl[0], out r) && int.TryParse(spl[1], out g) && int.TryParse(spl[2], out b))
            {
                if (spl.Length == 4 && !int.TryParse(spl[3], out a)) { return defaultIfInvalid; }
                return Color.FromArgb(a, r, g, b);
            }
            return defaultIfInvalid;
        }

        public Coordinate GetCoordinate(Coordinate defaultIfInvalid)
        {
            string[] spl = Value.Trim().Split(' ');
            if (spl.Length != 3) return defaultIfInvalid;
            decimal x, y, z;
            if (decimal.TryParse(spl[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                && decimal.TryParse(spl[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y)
                && decimal.TryParse(spl[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
            {
                return new Coordinate(x, y, z);
            }
            return defaultIfInvalid;
        }

        public static string FromColor(Color color)
        {
            return color.R.ToString() + ' ' + color.G + ' ' + color.B + ' ' + color.A;
        }

        public static string FromCoordinate(Coordinate coordinate)
        {
            return coordinate.X.ToString() + ' ' + coordinate.Y + ' ' + coordinate.Z;
        }

        public Property Clone()
        {
            return new Property
            {
                Key = Key,
                Value = Value
            };
        }
    }
}
