using System.ComponentModel;

namespace CBRE.Settings
{
    public class General
    {
        public static string? Theme { get; set; }
        public static bool EnableDiscordPresence { get; set; }

        static General()
        {
            EnableDiscordPresence = true;
            Theme = LicenseManager.UsageMode == LicenseUsageMode.Runtime ? "Dark" : null;
        }
    }
}
