using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CBRE.Editor.Documents;
using CBRE.Providers.Map;
using CBRE.Settings;

namespace CBRE.Editor.Compiling;

public partial class BulkExportForm : Form {
    private bool _cancelled = false;
    
    public BulkExportForm() {
        InitializeComponent();
    }

    private void BulkExportFiles(string dir) {
        var files = Directory.EnumerateFiles(dir)
            .Where(x => Path.GetExtension(x).ToLowerInvariant() == ".vmf")
            .ToArray();
        progressBar1.Invoke((MethodInvoker)(() => {
            progressBar1.Value = 0;
            progressBar1.Maximum = files.Length;
        }));
        var i = 0;
        progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = 0));
        foreach (var file in files) {
            if (_cancelled) break;
            var ready = false;
            ExportForm form = null;
            Document doc = null;
            Invoke((MethodInvoker)(() => {
                var map = MapProvider.GetMapFromFile(file, Directories.ModelDirs, out _);
                doc = new Document(file, map, SettingsManager.Game);
                form = new ExportForm();
                form.Show();
                ready = true;
            }));
            while (!ready) {
                Thread.Sleep(10);
            }
            form.Document = doc;
            var output = Path.Combine("out", Path.ChangeExtension(Path.GetFileName(file), "rmesh"));
            Directory.CreateDirectory("out");
            RMeshExport.SaveToFile(output, doc, form, true);
            progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = ++i));
            form.Invoke((MethodInvoker)(form.Close));
        }
        button1.Invoke((MethodInvoker)(() => button1.Enabled = true));
    }
    
    private void button1_Click(object sender, EventArgs e) {
        button1.Enabled = false;
        button2.Enabled = true;
        _cancelled = false;
        new Thread(() => BulkExportFiles(textBox1.Text)).Start();
    }

    private void button2_Click(object sender, EventArgs e) {
        _cancelled = true;
        button2.Enabled = false;
    }
}
