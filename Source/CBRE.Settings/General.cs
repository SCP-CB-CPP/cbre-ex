using System.ComponentModel;

namespace CBRE.Settings
{
    public class General
    {
        public static bool EnableDiscordPresence { get; set; }
        public static bool EnableDarkMode { get; set; }

        static General()
        {
            EnableDiscordPresence = true;
            EnableDarkMode = LicenseManager.UsageMode == LicenseUsageMode.Runtime;
        }
    }
}
