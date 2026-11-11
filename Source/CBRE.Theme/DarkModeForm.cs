using System.Windows.Forms;
using CBRE.Settings;

namespace DarkModeForms;

public class DarkModeForm : Form {
    private readonly DarkModeCS _dark;
    
    protected DarkModeForm() {
        if (ThemeManager.IsDarkMode) {
            _dark = new(this) {
                ColorizeIcons = false,
                ColorMode = DarkModeCS.DisplayMode.DarkMode,
                OScolors = ThemeManager.Colors,
            };
        }
    }
}
