using System.Windows.Forms;
using CBRE.Settings;

namespace DarkModeForms;

public static class DarkMessageBox {
    public static DialogResult Show(string text)
        => General.EnableDarkMode
            ? Messenger.MessageBox(text)
            : MessageBox.Show(text);

    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        => General.EnableDarkMode
            ? Messenger.MessageBox(text, caption, buttons, icon)
            : MessageBox.Show(text, caption, buttons, icon);
}
