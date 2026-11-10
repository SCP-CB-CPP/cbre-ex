using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.UI
{
    public partial class MapInformationDialog : DarkModeForm
    {
        public MapInformationDialog(Document document)
        {
            InitializeComponent();
            CalculateStats(document);
        }

        private void CalculateStats(Document document)
        {
            System.Collections.Generic.List<MapObject> all = document.Map.WorldSpawn.FindAll();
            System.Collections.Generic.List<Solid> solids = all.OfType<Solid>().ToList();
            System.Collections.Generic.List<Face> faces = solids.SelectMany(x => x.Faces).ToList();
            System.Collections.Generic.List<Entity> entities = all.OfType<Entity>().ToList();
            int numSolids = solids.Count;
            int numFaces = faces.Count;
            int numPointEnts = entities.Count(x => !x.HasChildren);
            int numSolidEnts = entities.Count(x => x.HasChildren);
            System.Collections.Generic.List<string> uniqueTextures = faces.Select(x => x.Texture.Name).Distinct().ToList();
            int numUniqueTextures = uniqueTextures.Count;
            int textureMemory = faces.Select(x => x.Texture.Texture)
                .Where(x => x != null)
                .Distinct()
                .Sum(x => x.Width * x.Height * 3); // 3 bytes per pixel
            decimal textureMemoryMb = textureMemory / (1024m * 1024m);
            // todo texture memory, texture packages

            NumSolids.Text = numSolids.ToString(CultureInfo.CurrentCulture);
            NumFaces.Text = numFaces.ToString(CultureInfo.CurrentCulture);
            NumPointEntities.Text = numPointEnts.ToString(CultureInfo.CurrentCulture);
            NumSolidEntities.Text = numSolidEnts.ToString(CultureInfo.CurrentCulture);
            NumUniqueTextures.Text = numUniqueTextures.ToString(CultureInfo.CurrentCulture);
            // TextureMemory.Text = textureMemory.ToString(CultureInfo.CurrentCulture);
            TextureMemory.Text = textureMemory.ToString("#,##0", CultureInfo.CurrentCulture)
                + " bytes (" + textureMemoryMb.ToString("0.00", CultureInfo.CurrentCulture) + " MB)";
            foreach (Providers.Texture.TexturePackage tp in document.GetUsedTexturePackages())
            {
                TexturePackages.Items.Add(tp);
            }
        }
    }
}
