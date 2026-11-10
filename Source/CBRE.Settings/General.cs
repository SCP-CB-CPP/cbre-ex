using System.ComponentModel;

namespace CBRE.Settings
{
    public class General
    {
        public static bool EnableDiscordPresence { get; set; }
        public static string? Theme { get; set; }
        public static bool EnableDarkMode => !string.IsNullOrEmpty(Theme);

        static General()
        {
            EnableDiscordPresence = true;
            Theme = LicenseManager.UsageMode == LicenseUsageMode.Runtime ? "Dark" : null;
        }
    }
}
