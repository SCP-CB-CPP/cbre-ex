using CBRE.Common;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Extensions;
using CBRE.Graphics.Arrays;
using CBRE.Graphics.Helpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using CBRE.Editor.Documents;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace CBRE.Editor.Rendering.Arrays
{
    public class MapObjectArray : VBO<MapObject, MapObjectVertex>
    {
        private const int Textured = 0;
        private const int Transparent = 1;
        private const int BrushWireframe = 2;
        private const int EntityWireframe = 3;

        public MapObjectArray(IEnumerable<MapObject> data)
            : base(data)
        {
        }

        public void RenderTextured(IGraphicsContext context, ITexture lightmapTexture)
        {
            foreach (Subset subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance != null))
            {
                ITexture tex = (ITexture)subset.Instance;
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);

                tex.Bind();
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture1);
                lightmapTexture.Bind();
                Render(context, PrimitiveType.Triangles, subset);
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            }
        }
        public void RenderUntextured(IGraphicsContext context, Coordinate location)
        {
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            TextureHelper.Unbind();
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture1);
            TextureHelper.Unbind();
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            foreach (Subset subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance == null))
            {
                Render(context, PrimitiveType.Triangles, subset);
            }
            foreach (Subset subset in GetSubsets<Entity>(Textured))
            {
                Entity e = (Entity)subset.Instance;
                if (!CBRE.Settings.View.DisableModelRendering && e.HasModel() && e.HideDistance() > (location - e.Origin).VectorMagnitude()) continue;
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        private decimal LookAtOrder(Face face, Coordinate cameraLocation, Coordinate lookAt)
        {
            return -(face.BoundingBox.Center - cameraLocation).LengthSquared();
        }

        public void RenderTransparent(IGraphicsContext context, Action<TextureReference> textureCallback, Coordinate cameraLocation, Coordinate lookAt)
        {
            IEnumerable<Subset> sorted =
                from subset in GetSubsets<Face>(Transparent)
                let face = subset.Instance as Face
                where face != null
                orderby LookAtOrder(face, cameraLocation, lookAt) ascending
                select subset;

            foreach (Subset subset in sorted)
            {
                TextureReference tex = ((Face)subset.Instance).Texture;
                if (Documents.DocumentManager.CurrentDocument.Map.HideToolTextures)
                {
                    if (tex.IsToolTexture) continue;
                }
                if (tex.Texture != null) tex.Texture.Bind();
                else TextureHelper.Unbind();
                textureCallback(tex);
                //program.Set("isTextured", tex.Texture != null);
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        public void RenderWireframe(IGraphicsContext context)
        {
            foreach (Subset subset in GetSubsets(BrushWireframe))
            {
                Render(context, PrimitiveType.Lines, subset);
            }

            foreach (Subset subset in GetSubsets(EntityWireframe))
            {
                Render(context, PrimitiveType.Lines, subset);
            }
        }

        public void RenderVertices(IGraphicsContext context, int pointSize)
        {
            GL.PointSize(pointSize);
            foreach (Subset subset in GetSubsets(BrushWireframe))
            {
                Render(context, PrimitiveType.Points, subset);
            }

        }

        public void UpdatePartial(IEnumerable<MapObject> objects)
        {
            UpdatePartial(objects.OfType<Solid>().SelectMany(x => x.Faces));
            UpdatePartial(objects.OfType<Entity>().Where(x => !x.HasChildren));
        }

        public void UpdatePartial(IEnumerable<Face> faces)
        {
            foreach (Face face in faces)
            {
                int offset = GetOffset(face);
                if (offset < 0) continue;
                IEnumerable<MapObjectVertex> conversion = Convert(face);
                Update(offset, conversion);
            }
        }

        public void UpdatePartial(IEnumerable<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                int offset = GetOffset(entity);
                if (offset < 0) continue;
                IEnumerable<MapObjectVertex> conversion = entity.GetBoxFaces().SelectMany(Convert);
                Update(offset, conversion);
            }
        }

        protected override void CreateArray(IEnumerable<MapObject> objects)
        {
            List<MapObject> obj = objects.Where(x => !x.IsVisgroupHidden && !x.IsCodeHidden).ToList();
            List<Face> faces = obj.OfType<Solid>().SelectMany(x => x.Faces).ToList();
            List<Entity> entities = obj.OfType<Entity>().Where(x => !x.HasChildren).ToList();

            StartSubset(BrushWireframe);

            // Render solids
            foreach (var group in faces.GroupBy(x => new { x.Texture.Texture, Transparent = HasTransparency(x) }))
            {
                int subset = group.Key.Transparent ? Transparent : Textured;
                if (!group.Key.Transparent) StartSubset(subset);

                foreach (Face face in group)
                {
                    if (group.Key.Transparent) StartSubset(subset);

                    PushOffset(face);
                    uint index = PushData(Convert(face));
                    if (!face.Parent.IsRenderHidden3D && face.Opacity > 0) PushIndex(subset, index, face.GetTriangleIndices());
                    if (!face.Parent.IsRenderHidden2D) PushIndex(BrushWireframe, index, face.GetLineIndices());

                    if (group.Key.Transparent) PushSubset(subset, face);
                }

                if (!group.Key.Transparent) PushSubset(subset, group.Key.Texture);
            }

            PushSubset(BrushWireframe, (object)null);
            StartSubset(EntityWireframe);

            // Render entities
            foreach (IGrouping<bool, Entity> g in entities.GroupBy(x => x.HasModel()))
            {
                // key = false -> no model, put in the untextured group
                // key = true  -> model, put in the entity group
                if (!g.Key) StartSubset(Textured);
                foreach (Entity entity in g)
                {
                    if (g.Key) StartSubset(Textured);
                    PushOffset(entity);
                    foreach (Face face in entity.GetBoxFaces())
                    {
                        uint index = PushData(Convert(face));
                        if (!face.Parent.IsRenderHidden3D) PushIndex(Textured, index, face.GetTriangleIndices());
                        if (!face.Parent.IsRenderHidden2D) PushIndex(EntityWireframe, index, face.GetLineIndices());
                    }
                    if (g.Key) PushSubset(Textured, entity);
                }
                if (!g.Key) PushSubset(Textured, (ITexture)null);
            }

            PushSubset(EntityWireframe, (object)null);
        }

        private bool HasTransparency(Face face)
        {
            return face.Opacity < 0.95
                   || (face.Texture.Texture != null && face.Texture.Texture.HasTransparency())
                   || IsNoColl(face);
        }

        protected IEnumerable<MapObjectVertex> Convert(Face face)
        {
            float nx = (float)face.Plane.Normal.DX,
              ny = (float)face.Plane.Normal.DY,
              nz = (float)face.Plane.Normal.DZ;
            float r = face.Colour.R / 255f,
                  g = face.Colour.G / 255f,
                  b = face.Colour.B / 255f,
                  a = face.Opacity;
            // Hacky as shit but who gaf.
            if (IsNoColl(face)) {
                a /= 2;
            }
            return face.GetIndexedVertices().Select(vert => new MapObjectVertex
            {
                Position = new Vector3((float)vert.Location.DX, (float)vert.Location.DY, (float)vert.Location.DZ),
                Normal = new Vector3(nx, ny, nz),
                Texture = new Vector2((float)vert.TextureU, (float)vert.TextureV),
                LightmapUv = new Vector2(vert.LMU, vert.LMV),
                Colour = new Color4(r, g, b, a),
                IsSelected = face.IsSelected || (face.Parent != null && face.Parent.IsSelected) ? 1 : 0
            });
        }
        
        private static bool IsNoColl(Face face) => face.Parent.Parent?.ClassName == "nocoll"
                                                   && !(DocumentManager.CurrentDocument?.Map.HideToolTextures ?? false);
    }
}
