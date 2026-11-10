using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CBRE.Common.Mediator;
using CBRE.UI.Native;
using DarkModeForms;

namespace CBRE.Editor.Logging
{
    public partial class ExceptionWindow : DarkModeForm
    {
        public ExceptionInfo ExceptionInfo { get; set; }
        private string LogText { get; set; }

        public ExceptionWindow(ExceptionInfo info)
        {
            ExceptionInfo = info;
            
            LogText = "CBRE-EX has encountered an error it couldn't recover from. Details are found below.\n" +
                      "-----------------------------------------------------------------------------------\n" +
                      $"System Processor: {info.ProcessorName}\n" +
                      $"Available Memory: {info.AvailableMemory}\n" +
                      $"Operating System: {info.OperatingSystem}\n" +
                      $".NET Version: {info.RuntimeVersion}\n" +
                      $"CBRE-EX Version: {info.ApplicationVersion}\n" +
                      "-----------------------------------ERROR MESSAGE-----------------------------------\n" +
                      info.FullStackTrace;
            
            InitializeComponent();
            
            ProcessorName.Text = info.ProcessorName;
            AvailableMemory.Text = info.AvailableMemory;
            RuntimeVersion.Text = info.RuntimeVersion;
            OperatingSystem.Text = info.OperatingSystem;
            CBREVersion.Text = info.ApplicationVersion;
            FullError.Text = info.FullStackTrace;
            
            NativeIcons.SHSTOCKICONINFO StockIconInfo = new NativeIcons.SHSTOCKICONINFO();
            StockIconInfo.cbSize = (UInt32)Marshal.SizeOf(typeof(NativeIcons.SHSTOCKICONINFO));
            NativeIcons.SHGetStockIconInfo(NativeIcons.SHSTOCKICONID.SIID_ERROR, NativeIcons.SHGSI.SHGSI_ICON | NativeIcons.SHGSI.SHGSI_SHELLICONSIZE, ref StockIconInfo);

            systemBitmap.Image = Icon.FromHandle(StockIconInfo.hIcon).ToBitmap();
            
            try
            {
                Directory.CreateDirectory("Logs\\Exceptions");
                string fn = DateTime.Now.ToString("dd-MM-yy-HH-mm-ss");
                using (StreamWriter sw = new StreamWriter($"Logs\\Exceptions\\{fn}.txt"))
                {
                    sw.Write(LogText);
                }
                HeaderLabel.Text += $"Information has been written to \"Logs\\Exceptions\\{fn}.txt\".";
            }
            catch (Exception e)
            {
                HeaderLabel.Text += $"Couldn't write error log: {e.Message}.";
            }

            FullError.SelectionLength = 0;
        }

        private void CancelButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(LogText);
        }

        private void reportButton_Click(object sender, EventArgs e)
        {
            Mediator.Publish(EditorMediator.OpenWebsite, Editor.GITHUB_REPORT_BUG_URL);
        }
    }
}
