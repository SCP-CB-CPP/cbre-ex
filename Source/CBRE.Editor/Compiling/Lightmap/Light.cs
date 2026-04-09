using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Transformations;
using CBRE.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CBRE.Editor.Compiling.Lightmap
{
    class Light
    {
        public CoordinateF Color;
        public float Intensity;
        public bool HasSprite;
        public CoordinateF Origin;
        public float Range;
        public float Radius;
        public int LightSeparation;

        public CoordinateF Direction;
        public float? innerCos;
        public float? outerCos;

        public static void FindLights(Map map, out List<Light> lightEntities)
        {
            Predicate<string> parseBooleanProperty = (prop) =>
            {
                return prop.Equals("yes", StringComparison.OrdinalIgnoreCase) || prop.Equals("true", StringComparison.OrdinalIgnoreCase);
            };

            lightEntities = new List<Light>();
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x =>
                {
                    float range;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("range"), out range))
                    {
                        range = 100.0f;
                    }
                    float radius;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("radius"), out radius))
                    {
                        radius = 0f;
                    }
                    int lightSeparation;
                    if (!int.TryParse(x.EntityData.GetPropertyValue("lightSeparation"), out lightSeparation))
                    {
                        lightSeparation = 1;
                    }
                    float intensity;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("intensity"), out intensity))
                    {
                        intensity = 1.0f;
                    }
                    bool hasSprite = parseBooleanProperty(x.EntityData.GetPropertyValue("hassprite") ?? "true");

                    // TODO: RGB\A color
                    Color c = x.EntityData.GetPropertyColor("color", System.Drawing.Color.Black);

                    return new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = range,
                        Radius = radius,
                        LightSeparation = lightSeparation,
                        Color = new CoordinateF(c.R, c.G, c.B),
                        Intensity = intensity,
                        HasSprite = hasSprite,
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
                    float radius;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("radius"), out radius))
                    {
                        radius = 0f;
                    }
                    int lightSeparation;
                    if (!int.TryParse(x.EntityData.GetPropertyValue("lightSeparation"), out lightSeparation))
                    {
                        lightSeparation = 1;
                    }
                    float intensity;
                    if (!float.TryParse(x.EntityData.GetPropertyValue("intensity"), out intensity))
                    {
                        intensity = 1.0f;
                    }
                    bool hasSprite = parseBooleanProperty(x.EntityData.GetPropertyValue("hassprite") ?? "true");
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
                        Radius = radius,
                        LightSeparation = lightSeparation,
                        Color = new CoordinateF(c.R, c.G, c.B),
                        Intensity = intensity,
                        HasSprite = hasSprite,
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

        public static List<Light> CalculateSoftLights(List<Light> lightEntities)
        {
            List<Light> newLightEntities = new List<Light>(lightEntities);
            int i = -1;
            foreach (Light light in lightEntities)
            {
                i++;
                if (light.Radius <= 0) continue;

                int totalLights = 1;

                List<Light> softLights = new List<Light>();

                int r = (int)Math.Floor(light.Radius);
                for (int x = -r; x <= r; x += light.LightSeparation)
                {
                    for (int y = -r; y <= r; y += light.LightSeparation)
                    {
                        for (int z = -r; z <= r; z += light.LightSeparation)
                        {
                            if (x == 0 && y == 0 && z == 0) continue;
                            if (x * x + y * y + z * z <= r * r)
                            {
                                Light newSoftLight = new Light();
                                newSoftLight.Color = light.Color;
                                newSoftLight.HasSprite = false;
                                newSoftLight.Range = light.Range;
                                newSoftLight.Radius = 0f;
                                newSoftLight.LightSeparation = 1;
                                newSoftLight.Direction = light.Direction;
                                newSoftLight.innerCos = light.innerCos;
                                newSoftLight.outerCos = light.outerCos;

                                CoordinateF newOrigin = light.Origin;
                                newOrigin += new CoordinateF(x, y, z);
                                newSoftLight.Origin = newOrigin;

                                softLights.Add(newSoftLight);
                                totalLights++;
                            }
                        }
                    }
                }

                foreach (Light softLight in softLights)
                {
                    softLight.Intensity = light.Intensity / totalLights;
                    newLightEntities.Add(softLight);
                }
                light.Intensity = light.Intensity / totalLights;
            }
            return newLightEntities;
        }
    }
}
