using System.IO;
using System.Numerics;

namespace CBRE.SMFConverter
{
    public static class BinaryExtensions
    {
        public static Vector3 ReadVector3D(this BinaryReader reader)
        {
            Vector3 retVal;
            retVal.X = reader.ReadSingle();
            retVal.Y = reader.ReadSingle();
            retVal.Z = reader.ReadSingle();
            return retVal;
        }

        public static Vector2 ReadVector2D(this BinaryReader reader)
        {
            Vector2 retVal;
            retVal.X = reader.ReadSingle();
            retVal.Y = reader.ReadSingle();
            return retVal;
        }
    }
}
