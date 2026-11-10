using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.UI.Layout
{
    public partial class ViewportWindow : DarkModeForm
    {
        public ViewportWindow(TableSplitConfiguration configuration)
        {
            InitializeComponent();

            SplitControl.Configuration = configuration;
        }

        public TableSplitControl TableSplitControl
        {
            get { return SplitControl; }
        }
    }
}
