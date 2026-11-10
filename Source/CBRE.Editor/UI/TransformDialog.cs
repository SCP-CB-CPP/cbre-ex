using CBRE.DataStructures.Geometric;
using CBRE.Editor.Enums;
using System;
using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.UI
{
    public partial class TransformDialog : DarkModeForm
    {
        private readonly Box _source;
        private decimal zeroValue = 0;

        public Coordinate TransformValue
        {
            get { return new Coordinate(ValueX.Value, ValueZ.Value, ValueY.Value); }
            set
            {
                ValueX.Value = value.X;
                ValueZ.Value = value.Y;
                ValueY.Value = value.Z;
            }
        }

        public TransformType TransformType
        {
            get
            {
                if (Rotate.Checked) return TransformType.Rotate;
                if (scale.Checked) return TransformType.Scale;
                return TransformType.Translate;
            }
            set
            {
                switch (value)
                {
                    case TransformType.Rotate:
                        Rotate.Checked = true;
                        break;
                    case TransformType.Scale:
                        scale.Checked = true;
                        break;
                    default:
                        Translate.Checked = true;
                        break;
                }
            }
        }

        public TransformDialog(Box source)
        {
            _source = source;
            InitializeComponent();

            ZeroValueXButton.Click += (sender, e) => ValueX.Value = zeroValue;
            ZeroValueYButton.Click += (sender, e) => ValueY.Value = zeroValue;
            ZeroValueZButton.Click += (sender, e) => ValueZ.Value = zeroValue;

            SourceValueXButton.Click += (sender, e) => ValueX.Value = _source.Width;
            SourceValueYButton.Click += (sender, e) => ValueY.Value = _source.Length;
            SourceValueZButton.Click += (sender, e) => ValueZ.Value = _source.Height;

            TypeChanged(null, null);
        }

        private void TypeChanged(object sender, EventArgs e)
        {
            SourceValueXButton.Visible
                = SourceValueYButton.Visible
                  = SourceValueZButton.Visible
                    = Translate.Checked;
            ZeroValueXButton.Text
                = ZeroValueYButton.Text
                  = ZeroValueZButton.Text
                    = scale.Checked ? "1" : "0";
            if (scale.Checked)
            {
                if (ValueX.Value == 0) ValueX.Value = 1;
                if (ValueY.Value == 0) ValueY.Value = 1;
                if (ValueZ.Value == 0) ValueZ.Value = 1;
                zeroValue = 1;
            }
            else
            {
                if (ValueX.Value == 1) ValueX.Value = 0;
                if (ValueY.Value == 1) ValueY.Value = 0;
                if (ValueZ.Value == 1) ValueZ.Value = 0;
                zeroValue = 0;
            }
        }

        private void OkButtonClicked(object sender, EventArgs e)
        {
            if (scale.Checked && (ValueX.Value == 0 || ValueY.Value == 0 || ValueZ.Value == 0))
            {
                DarkMessageBox.Show("Please enter a non-zero value for all axes when scaling.");
                DialogResult = DialogResult.None;
            }
        }
    }
}
