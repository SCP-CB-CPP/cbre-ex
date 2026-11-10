using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CBRE.Settings;
using DarkModeForms;

namespace DarkModeForms;

public static class ThemeManager {
    public static OSThemeColors? Colors { get; private set; }
    public static bool IsDarkMode => Colors != null;

    public static IReadOnlyList<string> AvailableThemes { get; private set; }
    
    public static void Initialize() {
        AvailableThemes = Directory.EnumerateFiles("Themes")
            .Where(x => x.EndsWith(".ini"))
            .Select(Path.GetFileNameWithoutExtension)
            .Prepend("Default")
            .ToArray();

        if (!string.IsNullOrEmpty(General.Theme)) {
            var path = Path.Combine("Themes", General.Theme + ".ini");

            if (!File.Exists(path)) {
                return;
            }
            
            var data = File.ReadLines(path)
                .Select(x => x.Split('='))
                .Where(x => x.Length == 2)
                .Select(x => (x[0].Trim(), x[1].Split(',')))
                .Where(x => x.Item2.Length == 3);

            Colors = new();
            foreach (var (name, rgb) in data) {
                if (int.TryParse(rgb[0], out var r) && int.TryParse(rgb[1], out var g) && int.TryParse(rgb[2], out var b)) {
                    typeof(OSThemeColors).GetProperty(name)!.SetValue(Colors, Color.FromArgb(r, g, b));
                }
            }
        }
    }
}