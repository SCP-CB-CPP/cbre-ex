using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Transformations;
using CBRE.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CBRE.Common.Extensions;

namespace CBRE.Editor.Compiling.Lightmap
{
    class Light
    {
        public CoordinateF Color;
        public float Intensity;
        public bool HasSprite;
        public bool AlwaysExport;
        public CoordinateF Origin;
        public float Range;

        public CoordinateF? Direction;
        public float? innerCos;
        public float? outerCos;

        public static void FindLights(Map map, out List<Light> lightEntities)
        {
            lightEntities = new List<Light>();
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x =>
                {
                    float range;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("range"), out range))
                    {
                        range = 100.0f;
                    }
                    float intensity;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("intensity"), out intensity))
                    {
                        intensity = 1.0f;
                    }
                    bool hasSprite = x.EntityData.GetPropertyValue("hassprite")?.ToBool() ?? true;
                    bool alwaysExport = x.EntityData.GetPropertyValue("alwaysexport")?.ToBool() ?? false;

                    // TODO: RGB\A color
                    Color c = x.EntityData.GetPropertyColor("color", System.Drawing.Color.Black);

                    return new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = range,
                        Color = new CoordinateF(c.R, c.G, c.B),
                        Intensity = intensity,
                        HasSprite = hasSprite,
                        AlwaysExport = alwaysExport,
                        Direction = null,
                        innerCos = null,
                        outerCos = null
                    };
                }));
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "spotlight").OfType<Entity>()
                .Select(x =>
                {
                    float range;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("range"), out range))
                    {
                        range = 100.0f;
                    }
                    float intensity;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("intensity"), out intensity))
                    {
                        intensity = 1.0f;
                    }
                    bool hasSprite = x.EntityData.GetPropertyValue("hassprite")?.ToBool() ?? true;
                    bool alwaysExport = x.EntityData.GetPropertyValue("alwaysexport")?.ToBool() ?? false;
                    float innerCos = 0.5f;
                    if (float.TryParse(x.EntityData.GetPropertyValue("innerconeangle"), out innerCos))
                    {
                        innerCos = (float)Math.Cos(innerCos * (float)Math.PI / 180.0f);
                    }
                    float outerCos = 0.75f;
                    if (float.TryParse(x.EntityData.GetPropertyValue("outerconeangle"), out outerCos))
                    {
                        outerCos = (float)Math.Cos(outerCos * (float)Math.PI / 180.0f);
                    }

                    Color c = x.EntityData.GetPropertyColor("color", System.Drawing.Color.Black);

                    Light light = new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = range,
                        Color = new CoordinateF(c.R, c.G, c.B),
                        Intensity = intensity,
                        HasSprite = hasSprite,
                        AlwaysExport = alwaysExport,
                        Direction = null,
                        innerCos = innerCos,
                        outerCos = outerCos
                    };

                    Coordinate angles = x.EntityData.GetPropertyCoordinate("angles");

                    Matrix pitch = Matrix.Rotation(Quaternion.EulerAngles(DMath.DegreesToRadians(angles.X), 0, 0));
                    Matrix yaw = Matrix.Rotation(Quaternion.EulerAngles(0, 0, -DMath.DegreesToRadians(angles.Y)));
                    Matrix roll = Matrix.Rotation(Quaternion.EulerAngles(0, DMath.DegreesToRadians(angles.Z), 0));

                    UnitMatrixMult m = new UnitMatrixMult(yaw * pitch * roll);

                    light.Direction = new CoordinateF(m.Transform(Coordinate.UnitY));
                    //TODO: make sure this matches 3dws

                    return light;
                }));
        }
    }
}
