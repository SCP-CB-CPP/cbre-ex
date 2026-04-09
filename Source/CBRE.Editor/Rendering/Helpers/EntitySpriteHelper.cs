using CBRE.DataStructures.GameData;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.Editor.Extensions;
using CBRE.Graphics.Helpers;
using CBRE.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Property = CBRE.DataStructures.MapObjects.Property;

namespace CBRE.Editor.Rendering.Helpers
{
    public class EntitySpriteHelper : IHelper
    {
        public Document Document { get; set; }
        public bool Is2DHelper { get { return false; } }
        public bool Is3DHelper { get { return true; } }
        public bool IsDocumentHelper { get { return false; } }
        public HelperType HelperType { get { return HelperType.Replace; } }

        public bool IsValidFor(MapObject o)
        {
            return !CBRE.Settings.View.DisableSpriteRendering && o is Entity && ((Entity)o).HasSprite();
        }

        public void BeforeRender2D(Viewport2D viewport)
        {
            throw new NotImplementedException();
        }

        public void Render2D(Viewport2D viewport, MapObject o)
        {
            throw new NotImplementedException();
        }

        public void AfterRender2D(Viewport2D viewport)
        {
            throw new NotImplementedException();
        }

        public void BeforeRender3D(Viewport3D viewport)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0);
        }

        public void Render3D(Viewport3D vp, MapObject o)
        {
            Vector3 right = vp.Camera.GetRight();
            Vector3 up = Vector3.Cross(right, (vp.Camera.LookAt - vp.Camera.Location).Normalized());
            Entity entity = (Entity)o;

            Vector3 orig = new Vector3((float)entity.Origin.X, (float)entity.Origin.Y, (float)entity.Origin.Z);
            if (entity.IsSelected)
            {
                orig = Vector3.TransformPosition(orig, Document.SelectListTransform);
            }
            Vector3 normal = Vector3.Subtract(vp.Camera.Location, orig);

            Property rKey = entity.EntityData.Properties.FirstOrDefault(p => p.Key == "radius");
            float.TryParse(rKey?.Value, out float r);

            Property colorKey = entity.EntityData.Properties.FirstOrDefault(p => p.Key == "color");
            string colorString = colorKey?.Value;

            Common.ITexture tex = Document.GetTexture(entity.GetSprite());
            if (tex == null) TextureHelper.Unbind();
            else tex.Bind();
            Console.WriteLine(entity.GetSprite());

            GL.Color3(Color.White);

            if (entity.GameData != null)
            {
                DataStructures.GameData.Property col = entity.GameData.Properties.FirstOrDefault(x => x.VariableType == VariableType.Color255);
                if (col != null)
                {
                    DataStructures.MapObjects.Property val = entity.EntityData.Properties.FirstOrDefault(x => x.Key == col.Name);
                    if (val != null)
                    {
                        GL.Color3(val.GetColour(Color.White));
                    }
                }
            }

            // todo rotation/orientation types

            Vector3 tup = Vector3.Multiply(up, (float)entity.BoundingBox.Height / 2f);
            Vector3 tright = Vector3.Multiply(right, (float)entity.BoundingBox.Width / 2f);

            GL.Begin(PrimitiveType.Quads);

            GL.Normal3(normal); GL.TexCoord2(0, 1); GL.Vertex3(Vector3.Subtract(orig, Vector3.Add(tup, tright)));
            GL.Normal3(normal); GL.TexCoord2(0, 0); GL.Vertex3(Vector3.Add(orig, Vector3.Subtract(tup, tright)));
            GL.Normal3(normal); GL.TexCoord2(1, 0); GL.Vertex3(Vector3.Add(orig, Vector3.Add(tup, tright)));
            GL.Normal3(normal); GL.TexCoord2(1, 1); GL.Vertex3(Vector3.Subtract(orig, Vector3.Subtract(tup, tright)));

            GL.End();

            if (r > 0f)
            {
                int numPoints = 32;
                int latitudeLines = 8;
                float increment = (float)(2 * Math.PI / numPoints);

                Color entityColor = Color.White;
                if (!string.IsNullOrEmpty(colorString))
                {
                    string[] parts = colorString.Split(' ');

                    entityColor = Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                }

                TextureHelper.Unbind();
                GL.Begin(PrimitiveType.Lines);
                // X/Y Plane
                for (int i = 0; i < numPoints; i++)
                {
                    float firstAngle = i * increment;
                    float secondAngle = (i + 1) * increment;
                    float firstX = (float)(orig.X + r * Math.Cos(firstAngle));
                    float firstY = (float)(orig.Y + r * Math.Sin(firstAngle));
                    float secondX = (float)(orig.X + r * Math.Cos(secondAngle));
                    float secondY = (float)(orig.Y + r * Math.Sin(secondAngle));
                    GL.Color3(Color.FromArgb(128, entityColor));
                    GL.Vertex3(firstX, firstY, orig.Z);
                    GL.Vertex3(secondX, secondY, orig.Z);
                }
                // Y/Z Plane
                for (int i = 0; i < numPoints; i++)
                {
                    float firstAngle = i * increment;
                    float secondAngle = (i + 1) * increment;
                    float firstY = (float)(orig.Y + r * Math.Cos(firstAngle));
                    float firstZ = (float)(orig.Z + r * Math.Sin(firstAngle));
                    float secondY = (float)(orig.Y + r * Math.Cos(secondAngle));
                    float secondZ = (float)(orig.Z + r * Math.Sin(secondAngle));
                    GL.Color3(Color.FromArgb(128, entityColor));
                    GL.Vertex3(orig.X, firstY, firstZ);
                    GL.Vertex3(orig.X, secondY, secondZ);
                }
                // Z/X Plane
                for (int i = 0; i < numPoints; i++)
                {
                    float firstAngle = i * increment;
                    float secondAngle = (i + 1) * increment;
                    float firstZ = (float)(orig.Z + r * Math.Cos(firstAngle));
                    float firstX = (float)(orig.X + r * Math.Sin(firstAngle));
                    float secondZ = (float)(orig.Z + r * Math.Cos(secondAngle));
                    float secondX = (float)(orig.X + r * Math.Sin(secondAngle));
                    GL.Color3(Color.FromArgb(128, entityColor));
                    GL.Vertex3(firstX, orig.Y, firstZ);
                    GL.Vertex3(secondX, orig.Y, secondZ);
                }
                GL.End();
            }
        }

        public void AfterRender3D(Viewport3D viewport)
        {
            GL.AlphaFunc(AlphaFunction.Always, 0);
            GL.Disable(EnableCap.AlphaTest);
            GL.Disable(EnableCap.Texture2D);
        }

        public void RenderDocument(ViewportBase viewport, Document document)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MapObject> Order(ViewportBase viewport, IEnumerable<MapObject> mapObjects)
        {
            Viewport3D vp3 = viewport as Viewport3D;
            if (vp3 == null) return mapObjects;
            DataStructures.Geometric.Coordinate cam = vp3.Camera.Location.ToCoordinate();
            return mapObjects.OrderByDescending(x => (x.BoundingBox.Center - cam).LengthSquared());
        }
    }
}
