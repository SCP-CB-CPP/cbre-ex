using System;
using System.Collections.Generic;
using System.Linq;
using CBRE.Common.Extensions;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Models;
using CBRE.DataStructures.Models.LightInfo;
using CBRE.Editor.Extensions;
using CBRE.Providers.Model;
using OpenTK;

namespace CBRE.Editor.Compiling.Lightmap.Model;

public class ModelLightmapper {
    public static void LightmapModels(Map map) {
        Light.FindLights(map, out var lights);
        
        var modelsToBake = map.WorldSpawn.Find(x => x.ClassName == "model")
            .Cast<Entity>()
            .Where(x => x.EntityData.GetPropertyValue("bake").ToBool());
        
        foreach (var model in modelsToBake) {
            model.SetModelLightData(BakeModel(model, lights));
        }
    }

    private static ModelLightInfo BakeModel(Entity entity, IReadOnlyList<Light> lights) {
        if (entity.GetModel() is not ModelReference model) throw new("Entity has no model?");
        
        var transform = entity.GetTransform();
        
        var modelTransforms = model.Model.GetTransforms();
        for (var i = 0; i < modelTransforms.Count; i++) {
            //modelTransforms = modelTransforms[i] * transform;
        }
        
        var meshGroups = model.Model.GetActiveMeshes().GroupBy(x => x.SkinRef);
        var modelLightData = new MeshLightInfo[model.Model.Textures.Count];
        
        foreach (var meshGroup in meshGroups) {
            var vertexCount = meshGroup.Sum(x => x.Vertices.Count);
            var vertices = new List<VertexLightInfo>();
            vertices.Capacity = vertexCount;
            foreach (var tri in Group3(meshGroup.SelectMany(x => x.Vertices))) {
                var a = GetGlobal(tri.Item1);
                var b = GetGlobal(tri.Item2);
                var c = GetGlobal(tri.Item3);

                var normal = (b - a).Cross(c - a).Normalise();
                
                vertices.Add(new(tri.Item1, a, CalculateLighting(a, normal)));
                vertices.Add(new(tri.Item2, b, CalculateLighting(b, normal)));
                vertices.Add(new(tri.Item3, c, CalculateLighting(c, normal)));
            }
            
            var indices = Enumerable.Range(0, vertexCount).ToArray();
            for (var i = 0; i < vertexCount; i++) {
                if (indices[i] != i) {
                    continue;
                }
                var count = 1;
                for (var j = i + 1; j < vertexCount; j++) {
                    if (vertices[i].Position.EquivalentTo(vertices[j].Position)
                        && vertices[i].Vertex.TextureU == vertices[j].Vertex.TextureU
                        && vertices[i].Vertex.TextureV == vertices[j].Vertex.TextureV) {
                        indices[j] = indices[i];
                        vertices[i] = vertices[i] with { Light = vertices[i].Light + vertices[j].Light };
                        count++;
                    }
                }
                vertices[i] = vertices[i] with { Light = vertices[i].Light / count };
            }
            
            modelLightData[meshGroup.Key] = new(vertices, indices);
        }

        return new(transform, modelLightData);
        
        Vector3 CalculateLighting(Coordinate pos, Coordinate normal) {
            var color = new Vector3(0f, 0f, 0f);
            foreach (var light in lights) {
                var toMe = pos - new Coordinate(light.Origin);
                var distSqr = toMe.LengthSquared();
                if (distSqr > (decimal)(light.Range * light.Range)) {
                    continue;
                }
                    
                var lightColor = new Vector3(light.Color.X, light.Color.Y, light.Color.Z) * light.Intensity / 255f;

                var distFactor = (float)(1f - Math.Sqrt((float)distSqr) / light.Range);
                distFactor *= distFactor;
                var dirFactor = (float)Math.Sqrt(Math.Max(0f, (float)normal.Dot(toMe.Normalise())));
                color += distFactor * dirFactor * lightColor;
            }
            return color;
        }
            
        Coordinate GetGlobal(MeshVertex vertex)
            => new Coordinate(
                vertex.Location *
                modelTransforms[vertex.BoneWeightings.First().Bone.BoneIndex]) * transform;

        static IEnumerable<(T, T, T)> Group3<T>(IEnumerable<T> source) {
            using var e = source.GetEnumerator();
            while (e.MoveNext()) {
                var a = e.Current;
                if (!e.MoveNext()) yield break;
                var b = e.Current;
                if (!e.MoveNext()) yield break;
                var c = e.Current;
                yield return (a, b, c);
            }
        }
    }
}