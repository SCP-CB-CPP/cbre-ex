using CBRE.Common;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Compiling.Lightmap;
using CBRE.Editor.Documents;
using CBRE.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CBRE.Common.Extensions;
using CBRE.DataStructures.Models;
using CBRE.DataStructures.Models.LightInfo;
using CBRE.Editor.Extensions;
using CBRE.Providers.Model;
using Path = System.IO.Path;

namespace CBRE.Editor.Compiling
{
    public class RMeshExport
    {
        enum RMeshLoadFlags
        {
            COLOR = 1,
            ALPHA = 2
        };

        enum RMeshBlendFlags
        {
            NORMAL = 0,
            DIFFUSE = 1,
            LM = 2
        };
        
        private record struct ModelTextureRenderInfo(string TextureName, int VertexCount, IEnumerable<VertexLightInfo> Vertices, IReadOnlyList<int> Indices);
        
        private const byte EXPORTED_VERSION = 2;
        
        public static void SaveToFile(string filename, Document document, ExportForm form, bool is1_3_12)
        {
            Map map = document.Map;
            string filepath = Path.GetDirectoryName(filename);
            filename = Path.GetFileName(filename);
            filename = Path.GetFileNameWithoutExtension(filename) + ".rmesh";
            string lmPath = Path.GetFileNameWithoutExtension(filename) + "_lm";
            
            if (LightmapConfig.LightmapOnExport) Lightmapper.Render(document, form, out _, out _);

            var entities = map.WorldSpawn.Find(x => x.ClassName != null)
                .OfType<Entity>()
                .GroupBy(x => x.ClassName.ToLowerInvariant())
                .ToDictionary(x => x.Key, x => x);
            
            var triggerBoxes = entities.TryGetValue("triggerbox", out var tb) ? tb.ToArray() : [];
            
            List<Face> faces = [];
            List<Face> transparentFaces = [];
            List<Face> facesNoColl = [];
            List<Face> transparentFacesNoColl = [];
            List<Face> invisibleCollisionFaces = [];
            var lmCount = 0;
            
            List<Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>> textures = [];

            var warnedNoColl1_3_11 = false;
            
            foreach (var (face, noColl) in map.WorldSpawn.Find(x => x is Solid).Cast<Solid>().SelectMany(x => x.Faces
                         .Select(y => (y, x.Parent?.ClassName?.ToLowerInvariant() == "nocoll")))) {
                var texName = face.Texture?.Name.ToLowerInvariant();

                switch (texName)
                {
                    case null:
                    case "tooltextures/invisible_collision":
                        invisibleCollisionFaces.Add(face);
                        continue;
                    case "tooltextures/triggerbox":
                        if (!triggerBoxes.Any())
                            throw new(
                                "The map contains trigger box faces, but no trigger box entities were found. Trigger boxes require an entity to function! Use \"Tie to Entity\" with your brush selected.");
                        continue;
                }
                
                if (face.Texture.IsToolTexture || face.Texture.Texture == null)
                    continue;

                lmCount = Math.Max(lmCount, face.LmIndex + 1);

                var isNoColl = noColl;
                if (noColl && !is1_3_12) {
                    if (!warnedNoColl1_3_11) {
                        warnedNoColl1_3_11 = true;
                        form.ProgressLog.Invoke((MethodInvoker)(() => {
                            form.ProgressLog.SelectionColor = Color.Red;
                            form.ProgressLog.AppendText(
                                "\nWarning: This map contains nocoll brush entities that are not supported in CB v1.3.11. " +
                                "The brushes will have collision.");
                            form.ProgressLog.SelectionColor = form.ProgressLog.ForeColor;
                        }));
                    }
                    isNoColl = false;
                }
                
                if (face.Texture.Texture.HasTransparency()) {
                    (isNoColl ? transparentFacesNoColl : transparentFaces).Add(face);
                    if (textures.All(x => x.Item1 != face.Texture.Name)) textures.Add(new(face.Texture.Name, RMeshLoadFlags.ALPHA, RMeshBlendFlags.NORMAL, 0));
                } else {
                    (isNoColl ? facesNoColl : faces).Add(face);
                    if (textures.All(x => x.Item1 != face.Texture.Name)) textures.Add(new(face.Texture.Name, RMeshLoadFlags.COLOR, RMeshBlendFlags.DIFFUSE, 0));
                }
            }

            List<Light> lights;
            Light.FindLights(map, out lights);
            lights.RemoveAll(l => !l.HasSprite && !l.AlwaysExport);

            Lightmapper.SaveLightmaps(document, lmCount, Path.Combine(filepath, lmPath), false);
            lmPath = Path.GetFileName(lmPath);

            var customEntities = document.GameData.Classes
                .Where(x => x.IsCustom)
                .Select(x => x.Name.ToLowerInvariant())
                .SelectMany(x => entities.TryGetValue(x, out var g) ? g : Enumerable.Empty<Entity>())
                .ToArray();
            
            var waypoints = entities.TryGetValue("waypoint", out var wp) ? wp.ToArray() : [];
            var soundEmitters = entities.TryGetValue("soundemitter", out var se) ? se.ToArray() : [];
            var modelEntities = entities.TryGetValue("model", out var p) ? p : Enumerable.Empty<Entity>();
            var modelEntitiesGrouped = modelEntities
                .GroupBy(x => x.EntityData.GetPropertyValue("bake")?.ToBool() ?? false)
                .ToDictionary(x => x.Key, x => x.ToArray());
            var props = modelEntitiesGrouped.TryGetValue(false, out var regular) ? regular : [];
            var bakedProps = modelEntitiesGrouped.TryGetValue(true, out var baked) ? baked : [];
            
            var screens = entities.TryGetValue("screen", out var s) ? s.ToArray() : [];

            FileStream stream = new FileStream(Path.Combine(filepath, filename), FileMode.Create);
            BinaryWriter br = new BinaryWriter(stream);

            //header
            if (is1_3_12) {
                br.WriteB3DString("RM");
                br.Write(EXPORTED_VERSION);
            } else {
                br.WriteB3DString(triggerBoxes.Any() ? "RoomMesh.HasTriggerBox" : "RoomMesh");
            }

            //mesh

            int vertCount;
            int vertOffset;
            int triCount;

            //TODO: find a clever way of splitting up meshes with the same texture
            //into several for collision optimization.
            //Making each face its own collision object is too slow, and merging all of
            //them together is not optimal either.

            var models = bakedProps
                .GroupBy(x => x.EntityData.GetPropertyValue("hascollision")?.ToBool() ?? true)
                .ToDictionary(x => x.Key, x => x.ToArray());

            WriteFaces(faces, transparentFaces, models.TryGetValue(true, out var collModels) ? collModels : []);
            
            if (is1_3_12) {
                WriteFaces(facesNoColl, transparentFacesNoColl, models.TryGetValue(false, out var noCollModels) ? noCollModels : []);
            }
            
            void WriteFaces(IReadOnlyCollection<Face> faces, IReadOnlyCollection<Face> transparentFaces, IReadOnlyCollection<Entity> models) {
                int texCount = 0;
                for (int i = 0; i < textures.Count; i++)
                {
                    texCount += faces.Where(x => x.Texture.Name == textures[i].Item1).Select(x => x.LmIndex).Distinct().Count();
                    texCount += transparentFaces.Any(x => x.Texture.Name == textures[i].Item1) ? 1 : 0;
                }

                br.Write(texCount + models.Sum(x => x.GetModel().Model.Textures.Count));

                for (int i = 0; i < textures.Count; i++)
                {
                    string texName = Directories.GetTextureExtension(textures[i].Item1);

                    for (int lmInd = 0; lmInd < lmCount; lmInd++)
                    {
                        var tLmFaces = faces.Where(x => x.Texture.Name == textures[i].Item1 && x.LmIndex == lmInd).ToArray();
                        Face[] tTrptFaces = transparentFaces.Where(x => x.Texture.Name == textures[i].Item1).ToArray();
                        vertCount = 0;
                        vertOffset = 0;
                        triCount = 0;

                        if (tLmFaces.Length > 0)
                        {
                            foreach (var face in tLmFaces)
                            {
                                vertCount += face.Vertices.Count;
                                triCount += face.GetTriangleIndices().Count() / 3;
                            }

                            byte flag = 1;
                            br.Write(flag);
                            string currLmPath = lmPath + (lmCount > 1 ? "_" + lmInd.ToString() : "");
                            currLmPath += ".png";
                            br.WriteB3DString(currLmPath);

                            flag = 1;
                            br.Write(flag);
                            br.WriteB3DString(texName);

                            if (vertCount > UInt16.MaxValue) throw new Exception("Vertex overflow: " + texName);
                            br.Write((Int32)vertCount);
                            foreach (var face in tLmFaces)
                            {
                                for (int j = 0; j < face.Vertices.Count; j++)
                                {
                                    br.Write((float)face.Vertices[j].Location.X);
                                    br.Write((float)face.Vertices[j].Location.Z);
                                    br.Write((float)face.Vertices[j].Location.Y);

                                    br.Write((float)face.Vertices[j].TextureU);
                                    br.Write((float)face.Vertices[j].TextureV);

                                    float lmMul = (lmCount > 1) ? 2.0f : 1.0f;
                                    float uSub = ((lmInd % 2) > 0) ? 0.5f : 0.0f;
                                    float vSub = ((lmInd / 2) > 0) ? 0.5f : 0.0f;

                                    br.Write((face.Vertices[j].LMU - uSub) * lmMul);
                                    br.Write((face.Vertices[j].LMV - vSub) * lmMul);

                                    br.Write((byte)255); //r
                                    br.Write((byte)255); //g
                                    br.Write((byte)255); //b
                                }
                            }
                            br.Write((Int32)triCount);
                            foreach (var face in tLmFaces)
                            {
                                foreach (uint ind in face.GetTriangleIndices())
                                {
                                    br.Write((Int32)(ind + vertOffset));
                                }

                                vertOffset += face.Vertices.Count;
                            }
                        }
                        else if (lmInd == 0 && tTrptFaces.Length > 0)
                        {
                            foreach (Face face in tTrptFaces)
                            {
                                vertCount += face.Vertices.Count;
                                triCount += face.GetTriangleIndices().Count() / 3;
                            }

                            byte flag = 0;
                            br.Write(flag);
                            flag = 3;
                            br.Write(flag);
                            br.WriteB3DString(texName);

                            if (vertCount > UInt16.MaxValue) throw new Exception("Vertex overflow!");
                            br.Write((Int32)vertCount);
                            foreach (Face face in tTrptFaces)
                            {
                                for (int j = 0; j < face.Vertices.Count; j++)
                                {
                                    br.Write((float)face.Vertices[j].Location.X);
                                    br.Write((float)face.Vertices[j].Location.Z);
                                    br.Write((float)face.Vertices[j].Location.Y);

                                    br.Write((float)face.Vertices[j].TextureU);
                                    br.Write((float)face.Vertices[j].TextureV);
                                    br.Write(0.0f);
                                    br.Write(0.0f);

                                    br.Write((byte)255); //r
                                    br.Write((byte)255); //g
                                    br.Write((byte)255); //b
                                }
                            }
                            br.Write((Int32)triCount);
                            foreach (Face face in tTrptFaces)
                            {
                                foreach (uint ind in face.GetTriangleIndices())
                                {
                                    br.Write((Int32)(ind + vertOffset));
                                }

                                vertOffset += face.Vertices.Count;
                            }
                        }
                    }
                }
                
                foreach (var entity in models) {
                    foreach (var info in GetModelLightData(entity)) {
                        byte flags = 0;
                        br.Write(flags);
            
                        flags = 1;
                        br.Write(flags);
                        br.WriteB3DString(Directories.GetTextureExtension(Path.Combine("Props",
                            Path.GetFileNameWithoutExtension(info.TextureName))));
            
                        br.Write(info.VertexCount);
                        
                        foreach (var vertex in info.Vertices) {
                            WriteVertex(vertex);
                        }
            
                        br.Write(info.Indices.Count / 3);
                        foreach (var t in info.Indices) {
                            br.Write(t);
                        }
                    }
                    
                    void WriteVertex(VertexLightInfo info) {
                        br.Write((float)info.Position.X);
                        br.Write((float)info.Position.Z);
                        br.Write((float)info.Position.Y);

                        br.Write(info.Vertex.TextureU);
                        br.Write(info.Vertex.TextureV);
                        br.Write(0.0f);
                        br.Write(0.0f);
                
                        br.Write((byte)(Math.Min(1f, info.Light.X) * 255f)); //r
                        br.Write((byte)(Math.Min(1f, info.Light.Y) * 255f)); //g
                        br.Write((byte)(Math.Min(1f, info.Light.Z) * 255f)); //b
                    }
                }
            }

            if (invisibleCollisionFaces.Any())
            {
                br.Write((Int32)1);

                vertCount = 0;
                vertOffset = 0;
                triCount = 0;

                foreach (Face face in invisibleCollisionFaces)
                {
                    vertCount += face.Vertices.Count;
                    triCount += face.GetTriangleIndices().Count() / 3;
                }

                if (vertCount > UInt16.MaxValue) throw new Exception("Vertex overflow!");
                br.Write((Int32)vertCount);
                foreach (Face face in invisibleCollisionFaces)
                {
                    for (int j = 0; j < face.Vertices.Count; j++)
                    {
                        br.Write((float)face.Vertices[j].Location.X);
                        br.Write((float)face.Vertices[j].Location.Z);
                        br.Write((float)face.Vertices[j].Location.Y);
                    }
                }
                br.Write((Int32)triCount);
                foreach (Face face in invisibleCollisionFaces)
                {
                    foreach (uint ind in face.GetTriangleIndices())
                    {
                        br.Write((Int32)(ind + vertOffset));
                    }

                    vertOffset += face.Vertices.Count;
                }
            }
            else
            {
                br.Write((Int32)0);
            }

            if (is1_3_12 || triggerBoxes.Any()) {
                br.Write(triggerBoxes.Count());
                foreach (var triggerBox in triggerBoxes) {
                    br.Write(1); // Surfaces
                    var trbFaces = triggerBox.GetChildren().OfType<Solid>().SelectMany(x => x.Faces).ToArray();

                    vertCount = 0;
                    vertOffset = 0;
                    triCount = 0;

                    foreach (Face face in trbFaces) {
                        vertCount += face.Vertices.Count;
                        triCount += face.GetTriangleIndices().Count() / 3;
                    }

                    br.Write((Int32)vertCount);
                    foreach (Face face in trbFaces) {
                        for (int j = 0; j < face.Vertices.Count; j++) {
                            br.Write((float)face.Vertices[j].Location.X);
                            br.Write((float)face.Vertices[j].Location.Z);
                            br.Write((float)face.Vertices[j].Location.Y);
                        }
                    }
                    br.Write((Int32)triCount);
                    foreach (Face face in trbFaces) {
                        foreach (uint ind in face.GetTriangleIndices()) {
                            br.Write((Int32)(ind + vertOffset));
                        }

                        vertOffset += face.Vertices.Count;
                    }

                    br.WriteB3DString(triggerBox.EntityData.GetPropertyValue("name"));
                }
            }

            var totalEntityCount = lights.Count + waypoints.Length + soundEmitters.Length + props.Length
                                   + screens.Length + customEntities.Length;
            br.Write(totalEntityCount);

            foreach (Light light in lights)
            {
                br.WriteB3DString("light");

                if (is1_3_12 && EXPORTED_VERSION >= 2) {
                    br.Write(light.HasSprite);
                }
                
                br.Write(light.Origin.X);
                br.Write(light.Origin.Z);
                br.Write(light.Origin.Y);

                br.Write(light.Range);

                string lcolor = light.Color.X + " " + light.Color.Y + " " + light.Color.Z;
                br.Write((Int32)lcolor.Length);
                for (int k = 0; k < lcolor.Length; k++)
                {
                    br.Write((byte)lcolor[k]);
                }

                br.Write(light.Intensity);
            }

            foreach (var waypoint in waypoints)
            {
                br.WriteB3DString("waypoint");

                br.Write((float)waypoint.Origin.X);
                br.Write((float)waypoint.Origin.Z);
                br.Write((float)waypoint.Origin.Y);
            }

            foreach (Entity soundEmitter in soundEmitters)
            {
                br.WriteB3DString("soundemitter");

                br.Write((float)soundEmitter.Origin.X);
                br.Write((float)soundEmitter.Origin.Z);
                br.Write((float)soundEmitter.Origin.Y);

                br.Write(int.Parse(soundEmitter.EntityData.GetPropertyValue("sound")));

                br.Write(float.Parse(soundEmitter.EntityData.GetPropertyValue("range")));
            }

            foreach (Entity prop in props)
            {
                br.WriteB3DString("model");

                string modelName = prop.EntityData.GetPropertyValue("file") ?? "";
                if (!modelName.Contains('.'))
                {
                    modelName = Path.GetFileName(Directories.GetModelPath(modelName)) ?? (modelName + ".x");
                }
                br.WriteB3DString(modelName);

                br.Write((float)prop.Origin.X);
                br.Write((float)prop.Origin.Z);
                br.Write((float)prop.Origin.Y);

                Coordinate rotation = prop.EntityData.GetPropertyCoordinate("angles");
                br.Write((float)rotation.X);
                br.Write((float)rotation.Y);
                br.Write((float)rotation.Z);

                Coordinate scale = prop.EntityData.GetPropertyCoordinate("scale");
                br.Write((float)scale.X);
                br.Write((float)scale.Y);
                br.Write((float)scale.Z);

                if (is1_3_12 && EXPORTED_VERSION >= 2) {
                    br.Write(prop.EntityData.GetPropertyValue("hascollision")?.ToBool() ?? true);
                    br.Write(int.Parse(prop.EntityData.GetPropertyValue("effects") ?? "0"));
                    br.WriteB3DString(prop.EntityData.GetPropertyValue("textureoverride") ?? "");
                }
            }

            foreach (Entity screen in screens)
            {
                br.WriteB3DString("screen");

                br.Write((float)screen.Origin.X);
                br.Write((float)screen.Origin.Z);
                br.Write((float)screen.Origin.Y);

                br.WriteB3DString(screen.EntityData.GetPropertyValue("imgpath"));
            }

            if (!is1_3_12 && customEntities.Any())
            {
                form.ProgressLog.Invoke((MethodInvoker)(() =>
                {
                    form.ProgressLog.SelectionColor = Color.Goldenrod;
                    form.ProgressLog.AppendText(
                        "\nWarning: This map contains entities that are not supported in CB v1.3.11. " +
                        "Unless you are using a modded version, this map will not work. " +
                        "It will also not be forward-compatible with v1.3.12.");
                    form.ProgressLog.SelectionColor = form.ProgressLog.ForeColor;
                }));
            }

            foreach (Entity customEntity in customEntities)
            {
                br.WriteB3DString(customEntity.ClassName);

                //Write position
                br.Write((float)customEntity.Origin.X);
                br.Write((float)customEntity.Origin.Z);
                br.Write((float)customEntity.Origin.Y);

                IEnumerable<DataStructures.GameData.Property> customEntityProperties = customEntity.GameData.Properties.Where(x => x.Name != "position");

                foreach (DataStructures.GameData.Property customEntityProperty in customEntityProperties)
                {
                    WriteCustomEntityProperty(customEntityProperty, ref br, customEntity);
                }
            }

            br.Dispose();
            stream.Dispose();

            form.ProgressLog.Invoke((MethodInvoker)(() => form.ProgressLog.AppendText(
                $"""
                
                {faces.Count + transparentFaces.Count + facesNoColl.Count + transparentFacesNoColl.Count + invisibleCollisionFaces.Count} total faces
                {transparentFaces.Count + transparentFacesNoColl.Count} of which transparent
                {facesNoColl.Count + transparentFacesNoColl.Count} of which without collision
                {invisibleCollisionFaces.Count} of which invisible collision
                
                {totalEntityCount} total entities
                {lights.Count} of which are lights
                {props.Length} of which are props
                {waypoints.Length} of which are waypoints
                """)));
            
            form.ProgressLog.Invoke((MethodInvoker)(() => form.ProgressLog.AppendText("\nDone!")));
            form.ProgressBar.Invoke((MethodInvoker)(() => form.ProgressBar.Value = 10000));
        }

        //If juan sees this he would probably crush my neck
        private static void WriteCustomEntityProperty(DataStructures.GameData.Property property, ref BinaryWriter binaryWriter, Entity entity)
        {
            Property mapProperty = entity.EntityData.Properties.FirstOrDefault(x => x.Key == property.Name);

            mapProperty ??= new() { Key = property.Name, Value = property.DefaultValue };
            
            switch (property.VariableType)
            {
                case DataStructures.GameData.VariableType.Bool:
                    binaryWriter.Write(mapProperty.Value?.ToBool() ?? false);
                    
                    break;
                case DataStructures.GameData.VariableType.Integer:
                    int.TryParse(mapProperty.Value, out int parsedInt);

                    binaryWriter.Write(parsedInt);
                    
                    break;
                case DataStructures.GameData.VariableType.Color255:
                    Coordinate colorCoord = entity.EntityData.GetPropertyCoordinate(property.Name, Coordinate.Zero);
                    string color = colorCoord.X + " " + colorCoord.Y + " " + colorCoord.Z;
                    
                    binaryWriter.WriteB3DString(color);
                    
                    break;
                case DataStructures.GameData.VariableType.Float:
                    float.TryParse(mapProperty.Value, out float parsedFloat);

                    binaryWriter.Write(parsedFloat);
                    
                    break;
                case DataStructures.GameData.VariableType.String:
                    binaryWriter.WriteB3DString(mapProperty.Value ?? string.Empty);
                    
                    break;
                case DataStructures.GameData.VariableType.Vector:
                    Coordinate coord = entity.EntityData.GetPropertyCoordinate(property.Name, Coordinate.Zero);
                    
                    binaryWriter.Write((float)coord.X);
                    binaryWriter.Write((float)coord.Y);
                    binaryWriter.Write((float)coord.Z);
                    
                    break;
            }
        }
        
        private static IEnumerable<ModelTextureRenderInfo> GetModelLightData(Entity entity) {
            if (entity.GetModel() is not ModelReference model) throw new("Entity has no model?");
            var modelLightData = entity.GetModelLightData();
            if (modelLightData != null) {
                if (LightmapConfig.LightmapOnExport) {
                    return modelLightData.MeshLightInfos
                        .Select((x, i) => new ModelTextureRenderInfo(model.Model.Textures[i].Name,
                            x.Vertices.Count, x.Vertices.AsEnumerable(), x.Indices));
                }

                var nowTransform = entity.GetTransform();
                var revertPrevTransform = modelLightData.UsedWorldTransform.Inverse();
                var fullTransform = revertPrevTransform * nowTransform;
                return modelLightData.MeshLightInfos
                    .Select((x, i) => new ModelTextureRenderInfo(model.Model.Textures[i].Name,
                        x.Vertices.Count, x.Vertices.Select(y => y with { Position = y.Position * fullTransform }),
                        x.Indices));
            }

            var transform = entity.GetTransform();
            return model.Model.StubLightInfo.Select((x, i) => new ModelTextureRenderInfo(model.Model.Textures[i].Name,
                x.Vertices.Count, x.Vertices
                    .Select(y => y with { Position = y.Position * transform }),
                x.Indices));
        }
    }
}
