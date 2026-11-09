namespace CBRE.Settings
{
    public class Exporting
    {
        public static bool ShowModelBakingWarning { get; set; }

        public static bool ForceLightmapOnExport { get; set; }

        static Exporting()
        {
            ShowModelBakingWarning = true;
            ForceLightmapOnExport = true;
        }
    }
}
