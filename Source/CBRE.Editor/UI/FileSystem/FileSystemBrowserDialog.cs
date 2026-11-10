using CBRE.Editor.Logging;
using CBRE.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.UI.FileSystem
{
    public partial class FileSystemBrowserDialog : DarkModeForm
    {
        #region Events
        public delegate void ConfirmedButtonEventHandler(object sender, IEnumerable<IFile> selection);
        public delegate void CancelButtonEventHandler(object sender);

        public event ConfirmedButtonEventHandler Confirmed;
        public event CancelButtonEventHandler Cancelled;

        protected virtual void OnConfirmed(IEnumerable<IFile> files)
        {
            if (Confirmed != null)
            {
                Confirmed(this, files);
            }
        }

        protected virtual void OnCancelled()
        {
            if (Cancelled != null)
            {
                Cancelled(this);
            }
        }

        #endregion Events

        public List<IFile> SelectedFiles { get; private set; }

        public string Filter
        {
            get
            {
                return Browser.Filter;
            }
            set
            {
                Browser.Filter = value;
            }
        }

        public string FilterText
        {
            get
            {
                return Browser.FilterText;
            }
            set
            {
                Browser.FilterText = value;
            }
        }

        public FileSystemBrowserDialog(IFile root)
        {
            InitializeComponent();
            Browser.Cancelled += Cancel;
            Browser.Confirmed += Confirm;

            try
            {
                // DO NOT REMOVE THIS TRY/CATCH BLOCK!
                // Without it, the CLR crashes, even if no exception is thrown! I have no idea why this is the case.
                Browser.File = root;
            }
            catch (Exception ex)
            {
                Logger.ShowException(ex);
            }
        }

        private void Confirm(object sender, IEnumerable<IFile> selection)
        {
            SelectedFiles = selection.ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel(object sender)
        {
            SelectedFiles = new List<IFile>();
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
