using CBRE.Common.Mediator;
using CBRE.Editor.Extensions;
using CBRE.QuickForms;
using CBRE.Settings;
using CBRE.Settings.Models;
using CBRE.UI;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;
using DarkModeForms;

namespace CBRE.Editor.Settings
{
    /// <summary>
    /// Description of SettingsForm.
    /// </summary>
    public partial class SettingsForm : DarkModeForm
    {
        private List<Hotkey> _hotkeys;

        public SettingsForm()
        {
            InitializeComponent();
            BindColourPicker(GridBackgroundColour);
            BindColourPicker(GridColour);
            BindColourPicker(GridZeroAxisColour);
            BindColourPicker(GridBoundaryColour);
            BindColourPicker(GridHighlight1Colour);
            BindColourPicker(GridHighlight2Colour);
            BindColourPicker(ViewportBackgroundColour);
            AddColourPresetButtons();
            AddFileTypeBoxes();

            UpdateData();

            SetupDirectoryDataGrids();

            BindConfigControls();
        }

        #region Colour Presets

        private struct ColourPreset
        {
            public string Name { get; set; }
            public Color Background { get; set; }
            public Color Grid { get; set; }
            public Color ZeroAxes { get; set; }
            public Color Boundaries { get; set; }
            public Color Highlight1 { get; set; }
            public Color Highlight2 { get; set; }
            public bool Highlight1Enabled { get; set; }
            public bool Highlight2Enabled { get; set; }
            public int Highlight1Count { get; set; }
            public int Highlight2Units { get; set; }
        }

        private static readonly ColourPreset[] Presets = new[]
        {
            new ColourPreset
            {
                Name = "CBRE-EX Default",
                Background = Color.Black,
                Grid = Color.FromArgb(75, 75, 75),
                ZeroAxes = Color.FromArgb(0, 100, 100),
                Highlight1 = Color.FromArgb(115, 115, 115),
                Highlight1Count = 8,
                Highlight1Enabled = true,
                Highlight2 = Color.FromArgb(100, 46, 0),
                Highlight2Units = 1024,
                Highlight2Enabled = true,
                Boundaries = Color.Red
            },
            new ColourPreset
            {
                Name = "Hammer 3",
                Background = Color.Black,
                Grid = Color.FromArgb(81, 81, 81),
                ZeroAxes = Color.FromArgb(0, 100, 100),
                Highlight1 = Color.FromArgb(120, 120, 120),
                Highlight1Count = 8,
                Highlight1Enabled = true,
                Highlight2 = Color.FromArgb(120, 120, 120),
                Highlight2Units = 1024,
                Highlight2Enabled = true,
                Boundaries = Color.FromArgb(120, 120, 120)
            },
            new ColourPreset
            {
                Name = "Hammer 4",
                Background = Color.Black,
                Grid = Color.FromArgb(81, 81, 81),
                ZeroAxes = Color.FromArgb(0, 100, 100),
                Highlight1 = Color.FromArgb(109, 109, 109),
                Highlight1Count = 8,
                Highlight1Enabled = true,
                Highlight2 = Color.FromArgb(100, 46, 1),
                Highlight2Units = 1024,
                Highlight2Enabled = true,
                Boundaries = Color.FromArgb(100, 46, 1)
            },
            new ColourPreset
            {
                Name = "GtkRadiant",
                Background = Color.White,
                Grid = Color.FromArgb(191, 191, 191),
                ZeroAxes = Color.FromArgb(127, 127, 127),
                Highlight1 = Color.FromArgb(127, 127, 127),
                Highlight1Count = 8,
                Highlight1Enabled = false,
                Highlight2 = Color.FromArgb(127, 127, 127),
                Highlight2Units = 64,
                Highlight2Enabled = true,
                Boundaries = Color.FromArgb(127, 127, 127)
            },
            new ColourPreset
            {
                Name = "QuArK",
                Background = Color.FromArgb(232, 220, 184),
                Grid = Color.FromArgb(208, 208, 255),
                ZeroAxes = Color.FromArgb(166, 202, 240),
                Highlight1 = Color.FromArgb(184, 196, 255),
                Highlight1Count = 8,
                Highlight1Enabled = true,
                Highlight2 = Color.FromArgb(184, 196, 255),
                Highlight2Units = 1024,
                Highlight2Enabled = false,
                Boundaries = Color.FromArgb(166, 202, 240)
            },
            new ColourPreset
            {
                Name = "Unreal Editor",
                Background = Color.FromArgb(163, 163, 163),
                Grid = Color.FromArgb(145, 145, 145),
                ZeroAxes = Color.FromArgb(119, 119, 119),
                Highlight1 = Color.FromArgb(127, 127, 127),
                Highlight1Count = 8,
                Highlight1Enabled = true,
                Highlight2 = Color.FromArgb(127, 127, 127),
                Highlight2Units = 1024,
                Highlight2Enabled = false,
                Boundaries = Color.FromArgb(0, 0, 107)
            }
        };

        private void AddColourPresetButtons()
        {
            ColourPresetPanel.Controls.Clear();
            foreach (ColourPreset cp in Presets)
            {
                Button btn = new Button
                {
                    AutoSize = false,
                    Height = 23,
                    Width = 95,
                    Padding = new Padding(0),
                    Margin = new Padding(2),
                    Text = cp.Name
                };
                ColourPreset preset = cp;
                btn.Click += (s, e) => SetColourPreset(preset);
                ColourPresetPanel.Controls.Add(btn);
            }
        }

        private void SetColourPreset(ColourPreset cp)
        {
            GridBackgroundColour.BackColor = cp.Background;
            GridColour.BackColor = cp.Grid;
            GridZeroAxisColour.BackColor = cp.ZeroAxes;
            GridBoundaryColour.BackColor = cp.Boundaries;

            GridHighlight1Colour.BackColor = cp.Highlight1;
            GridHighlight1Distance.Value = cp.Highlight1Count;
            GridHighlight1On.Checked = cp.Highlight1Enabled;

            GridHighlight2Colour.BackColor = cp.Highlight2;
            GridHighlight2UnitNum.SelectedItem = cp.Highlight2Units.ToString();
            GridHighlight2On.Checked = cp.Highlight2Enabled;
        }

        #endregion

        #region File Types

        private void AddFileTypeBoxes()
        {
            //TODO: remove?
        }

        #endregion

        #region Initialisation
        private void BindConfigControls()
        {
            //TODO: remove?
        }

        private void BindColourPicker(Control panel)
        {
            panel.Click += (sender, e) =>
            {
                Control p = (Control)sender;
                using (ColorDialog cpd = new ColorDialog { Color = p.BackColor })
                {
                    if (cpd.ShowDialog() == DialogResult.OK) p.BackColor = cpd.Color;
                }
            };
        }

        private void CheckNull<T>(T obj, Action<T> act) where T : class
        {
            if (obj != null) act(obj);
        }

        #endregion

        #region Data Loading

        private void UpdateData()
        {
            _hotkeys = Hotkeys.GetHotkeys().Select(x => new Hotkey { ID = x.ID, HotkeyString = x.HotkeyString }).ToList();
        }

        private void SetupDirectoryDataGrids()
        {
            textureDirsDataGrid.CellEndEdit += TextureDirsDataGrid_CellEndEdit;
            textureDirsDataGrid.EditingControlShowing += TextureDirsDataGrid_EditingControlShowing;
            textureDirsDataGrid.CellClick += TextureDirsDataGrid_CellClick;

            modelDirsDataGrid.CellEndEdit += ModelDirsDataGrid_CellEndEdit;
            modelDirsDataGrid.EditingControlShowing += ModelDirsDataGrid_EditingControlShowing;
            modelDirsDataGrid.CellClick += ModelDirsDataGrid_CellClick;

            foreach (string dir in Directories.TextureDirs)
            {
                if (string.IsNullOrEmpty(dir)) { continue; }
                AddDirsRow(textureDirsDataGrid, dir);
            }
            AddDirsRow(textureDirsDataGrid, "");


            foreach (string dir in Directories.ModelDirs)
            {
                if (string.IsNullOrEmpty(dir)) { continue; }
                AddDirsRow(modelDirsDataGrid, dir);
            }
            AddDirsRow(modelDirsDataGrid, "");
        }

        private void TextureDirsDataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                OpenBrowseDirDialog((dir) =>
                {
                    textureDirsDataGrid.Rows[e.RowIndex].Cells[1].Value = dir;
                    DirsChanged(textureDirsDataGrid);
                });
            }
            else if (e.ColumnIndex == 0)
            {
                textureDirsDataGrid.Rows[e.RowIndex].Cells[1].Value = "";
                DirsChanged(textureDirsDataGrid);
            }
        }

        private void ModelDirsDataGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                OpenBrowseDirDialog((dir) =>
                {
                    modelDirsDataGrid.Rows[e.RowIndex].Cells[1].Value = dir;
                    DirsChanged(modelDirsDataGrid);
                });
            }
            else if (e.ColumnIndex == 0)
            {
                modelDirsDataGrid.Rows[e.RowIndex].Cells[1].Value = "";
                DirsChanged(modelDirsDataGrid);
            }
        }

        private void TextureDirsDataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl tb)
            {
                tb.PreviewKeyDown -= TextureDirsDataGrid_KeyDown;
                tb.PreviewKeyDown += TextureDirsDataGrid_KeyDown;
            }
        }

        private void ModelDirsDataGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewTextBoxEditingControl tb)
            {
                tb.PreviewKeyDown -= ModelDirsDataGrid_KeyDown;
                tb.PreviewKeyDown += ModelDirsDataGrid_KeyDown;
            }
        }

        private void ReIndex()
        {

        }

        private void UpdateBuildTree()
        {

        }

        private void UpdateGameTree()
        {

        }

        #endregion

        #region Controls

        public void SelectTab(int index)
        {
            tbcSettings.SelectedIndex = index;
        }

        private void AddHeading(string text)
        {
            Label label = new Label
            {
                Font = new Font(tbcSettings.Font, FontStyle.Bold),
                Text = text,
                AutoSize = true,
                Padding = new Padding(0, 5, 0, 5)
            };
            flowLayoutPanel1.Controls.Add(label);
        }

        private CheckBox AddSetting(Expression<Func<bool>> prop, string text)
        {
            MemberExpression expression = (MemberExpression)prop.Body;
            PropertyInfo property = (PropertyInfo)expression.Member;
            CheckBox checkbox = new CheckBox
            {
                Text = text,
                AutoSize = true,
                Checked = (bool)property.GetValue(null, null),
                Tag = prop,
                Padding = new Padding(10, 0, 0, 0)
            };
            checkbox.CheckedChanged += (s, e) => property.SetValue(null, checkbox.Checked, null);
            flowLayoutPanel1.Controls.Add(checkbox);

            return checkbox;
        }

        private ComboBox AddSetting(Expression<Func<Enum>> prop, string text)
        {
            MemberExpression expression = (MemberExpression)((UnaryExpression)prop.Body).Operand;
            PropertyInfo property = (PropertyInfo)expression.Member;
            ComboBox combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300
            };
            List<Enum> vals = Enum.GetValues(property.PropertyType).OfType<Enum>().ToList();
            foreach (Enum val in vals)
            {
                combo.Items.Add(val.GetDescription());
            }
            combo.SelectedIndex = vals.IndexOf((Enum)property.GetValue(null, null));
            combo.SelectedIndexChanged += (s, e) => property.SetValue(null, vals[combo.SelectedIndex], null);
            Label label = new Label
            {
                AutoSize = true,
                Text = text,
                Padding = new Padding(0, 5, 0, 5)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            panel.Controls.Add(label);
            panel.Controls.Add(combo);
            flowLayoutPanel1.Controls.Add(panel);

            return combo;
        }

        private NumericUpDown AddSetting(Expression<Func<decimal>> prop, decimal min, decimal max, int decimals, decimal increment, string text)
        {
            MemberExpression expression = (MemberExpression)prop.Body;
            PropertyInfo property = (PropertyInfo)expression.Member;
            NumericUpDown updown = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Increment = increment,
                Value = Convert.ToDecimal(property.GetValue(null, null)),
                Width = 50
            };
            updown.ValueChanged += (s, e) => property.SetValue(null, updown.Value, null);
            Label label = new Label
            {
                AutoSize = true,
                Text = text,
                Padding = new Padding(0, 5, 0, 5)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            panel.Controls.Add(label);
            panel.Controls.Add(updown);
            flowLayoutPanel1.Controls.Add(panel);

            return updown;
        }

        private NumericUpDown AddSetting(Expression<Func<int>> prop, int min, int max, string text)
        {
            MemberExpression expression = (MemberExpression)prop.Body;
            PropertyInfo property = (PropertyInfo)expression.Member;
            NumericUpDown updown = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                DecimalPlaces = 0,
                Increment = 1,
                Value = Convert.ToDecimal(property.GetValue(null, null)),
                Width = 50
            };
            updown.ValueChanged += (s, e) => property.SetValue(null, (int)updown.Value, null);
            Label label = new Label
            {
                AutoSize = true,
                Text = text,
                Padding = new Padding(0, 5, 0, 5)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            panel.Controls.Add(label);
            panel.Controls.Add(updown);
            flowLayoutPanel1.Controls.Add(panel);

            return updown;
        }

        private Panel AddSetting(Expression<Func<Color>> prop, string text)
        {
            MemberExpression expression = (MemberExpression)prop.Body;
            PropertyInfo property = (PropertyInfo)expression.Member;
            Panel colour = new Panel
            {
                BackColor = (Color)property.GetValue(null, null),
                Height = 20,
                Width = 50,
                BorderStyle = BorderStyle.FixedSingle
            };
            colour.Click += (s, e) =>
            {
                using (ColorDialog cpd = new ColorDialog { Color = colour.BackColor })
                {
                    if (cpd.ShowDialog() == DialogResult.OK)
                    {
                        colour.BackColor = cpd.Color;
                        property.SetValue(null, cpd.Color, null);
                    }
                }
            };
            Label label = new Label
            {
                AutoSize = true,
                Text = text,
                Padding = new Padding(0, 5, 0, 5)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            panel.Controls.Add(label);
            panel.Controls.Add(colour);
            flowLayoutPanel1.Controls.Add(panel);

            return colour;
        }

        #endregion

        #region Load/Apply

        private void SettingsFormLoad(object sender, EventArgs e)
        {
            AddHeading("General Settings");
            AddSetting(() => CBRE.Settings.General.EnableDiscordPresence, "Enable Discord rich presence");
            AddThemeDropdown();

            AddHeading("Object Creation");
            AddSetting(() => CBRE.Settings.Select.SwitchToSelectAfterCreation, "Switch to selection tool after brush creation");
            AddSetting(() => CBRE.Settings.Select.SwitchToSelectAfterEntity, "Switch to selection tool after entity creation");
            AddSetting(() => CBRE.Settings.Select.SelectCreatedBrush, "Automatically select created brush");
            AddSetting(() => CBRE.Settings.Select.SelectCreatedEntity, "Automatically select created entity");
            AddSetting(() => CBRE.Settings.Select.DeselectOthersWhenSelectingCreation, "Deselect other objects when automatically selecting created items");
            AddSetting(() => CBRE.Settings.Select.ResetBrushTypeOnCreation, "Reset to block brush type after creating brush");
            AddSetting(() => CBRE.Settings.Select.KeepVisgroupsWhenCloning, "Keep visgroups when cloning");

            AddHeading("Multiple Files");
            AddSetting(() => CBRE.Settings.View.LoadSession, "Load previously opened files on startup");
            AddSetting(() => CBRE.Settings.View.KeepCameraPositions, "Keep current camera positions when switching between maps");
            AddSetting(() => CBRE.Settings.View.KeepSelectedTool, "Keep current selected tool when switching between maps");

            AddHeading("Textures");
            AddSetting(() => CBRE.Settings.Select.ApplyTextureImmediately, "Apply texture immediately after browsing in the texture application tool");

            AddHeading("Rendering");
            AddSetting(() => CBRE.Settings.View.ViewportAntiAliasing, "Viewport anti-alising");
            AddSetting(() => CBRE.Settings.View.DisableToolTextureTransparency, "Disable tool texture transparency");
            AddSetting(() => CBRE.Settings.View.GloballyDisableTransparency, "Disable transparent textures globally");
            AddSetting(() => CBRE.Settings.View.DisableModelRendering, "Disable model rendering");
            AddSetting(() => CBRE.Settings.View.DisableSpriteRendering, "Disable sprite rendering");

            AddHeading("Center Handles");
            AddSetting(() => CBRE.Settings.Select.DrawCenterHandles, "Render brush center handles");
            AddSetting(() => CBRE.Settings.Select.CenterHandlesActiveViewportOnly, "Render center handles only in active viewport");
            AddSetting(() => CBRE.Settings.Select.CenterHandlesFollowCursor, "Render center handles only near cursor position");
            AddSetting(() => CBRE.Settings.Select.BoxSelectByCenterHandlesOnly, "Selection box selects by center handles only");
            AddSetting(() => CBRE.Settings.Select.ClickSelectByCenterHandlesOnly, "Clicking in 2D view selects by center handles only");

            AddHeading("Interaction");
            AddSetting(() => CBRE.Settings.Select.DoubleClick3DAction, "Action to perform when double-clicking an object in the 3D view");
            AddSetting(() => CBRE.Settings.Select.OpenObjectPropertiesWhenCreatingEntity, "Open object properties when creating an entity");

            AddHeading("2D Vertices");
            AddSetting(() => CBRE.Settings.View.Draw2DVertices, "Render vertices in 2D views");
            AddSetting(() => CBRE.Settings.View.VertexPointSize, 1, 10, "Vertex point size");
            AddSetting(() => CBRE.Settings.View.OverrideVertexColour, "Override vertex colour (defaults to the brush colour)");
            AddSetting(() => CBRE.Settings.View.VertexOverrideColour, "Vertex override colour");

            AddHeading("Selection Box");
            AddSetting(() => CBRE.Settings.Select.AutoSelectBox, "Automatically select when box is drawn");
            AddSetting(() => CBRE.Settings.View.DrawBoxText, "Draw selection box size in the viewport");
            AddSetting(() => CBRE.Settings.View.DrawBoxDashedLines, "Draw selection box with dashed lines");
            AddSetting(() => CBRE.Settings.View.ScrollWheelZoomMultiplier, 1.01m, 10, 2, 0.1m, "Scroll wheel zoom multiplier (default 1.20)");
            AddSetting(() => CBRE.Settings.View.SelectionBoxBackgroundOpacity, 0, 128, "Selection box background opacity");

            AddHeading("Camera");
            AddSetting(() => CBRE.Settings.View.Camera2DPanRequiresMouseClick, "Require mouse click to enable panning in 2D viewports when holding spacebar");
            AddSetting(() => CBRE.Settings.View.Camera3DPanRequiresMouseClick, "Require mouse click to enable free-look in 3D viewports when holding spacebar");

            AddHeading("Undo Stack");
            AddSetting(() => CBRE.Settings.Select.UndoStackSize, 1, 1000, "Undo stack size (High value may cause out of memory situations)");
            AddSetting(() => CBRE.Settings.Select.SkipSelectionInUndoStack, "Fast-forward selection operations when performing undo/redo (selection, deselection)");
            AddSetting(() => CBRE.Settings.Select.SkipVisibilityInUndoStack, "Fast-forward visibility operations when performing undo/redo (visgroup visibility, quick show/hide)");

            // 2D Views
            CrosshairCursorIn2DViews.Checked = CBRE.Settings.View.CrosshairCursorIn2DViews;
            DrawEntityNames.Checked = CBRE.Settings.View.DrawEntityNames;
            DrawEntityAngles.Checked = CBRE.Settings.View.DrawEntityAngles; //mxd

            RotationStyle_SnapOnShift.Checked = CBRE.Settings.Select.RotationStyle == RotationStyle.SnapOnShift;
            RotationStyle_SnapOffShift.Checked = CBRE.Settings.Select.RotationStyle == RotationStyle.SnapOffShift;
            RotationStyle_SnapNever.Checked = CBRE.Settings.Select.RotationStyle == RotationStyle.SnapNever;

            SnapStyle_SnapOffAlt.Checked = CBRE.Settings.Select.SnapStyle == SnapStyle.SnapOffAlt;
            SnapStyle_SnapOnAlt.Checked = CBRE.Settings.Select.SnapStyle == SnapStyle.SnapOnAlt;

            ArrowKeysNudgeSelection.Checked = CBRE.Settings.Select.ArrowKeysNudgeSelection;
            NudgeUnits.Value = CBRE.Settings.Select.NudgeUnits;
            NudgeStyle_GridOffCtrl.Checked = CBRE.Settings.Select.NudgeStyle == NudgeStyle.GridOffCtrl;
            NudgeStyle_GridOnCtrl.Checked = CBRE.Settings.Select.NudgeStyle == NudgeStyle.GridOnCtrl;

            DefaultGridSize.SelectedItem = Grid.DefaultSize;
            HideGridLimit.Value = Grid.HideSmallerThan;
            HideGridOn.Checked = Grid.HideSmallerOn;
            HideGridFactor.SelectedItem = Grid.HideFactor;

            GridBackgroundColour.BackColor = Grid.Background;
            GridColour.BackColor = Grid.GridLines;
            GridZeroAxisColour.BackColor = Grid.ZeroLines;
            GridBoundaryColour.BackColor = Grid.BoundaryLines;
            GridHighlight1Colour.BackColor = Grid.Highlight1;
            GridHighlight2Colour.BackColor = Grid.Highlight2;
            GridHighlight1Distance.Value = Grid.Highlight1LineNum;
            GridHighlight2UnitNum.SelectedItem = Grid.Highlight2UnitNum;
            GridHighlight1On.Checked = Grid.Highlight1On;
            GridHighlight2On.Checked = Grid.Highlight2On;

            // 3D Views
            ViewportBackgroundColour.BackColor = CBRE.Settings.View.ViewportBackground;
            BackClippingPlaneUpDown.Value = BackClippingPane.Value = CBRE.Settings.View.BackClippingPane;
            ModelRenderDistanceUpDown.Value = ModelRenderDistance.Value = CBRE.Settings.View.ModelRenderDistance;
            DetailRenderDistanceUpDown.Value = DetailRenderDistance.Value = CBRE.Settings.View.DetailRenderDistance;
            BackClippingPaneChanged(null, null);
            ModelRenderDistanceChanged(null, null);
            DetailRenderDistanceChanged(null, null);

            ForwardSpeedUpDown.Value = ForwardSpeed.Value = CBRE.Settings.View.ForwardSpeed;
            ForwardSpeedChanged(null, null);

            TimeToTopSpeed.Value = (int)(CBRE.Settings.View.TimeToTopSpeed / 100);
            TimeToTopSpeedUpDown.Value = CBRE.Settings.View.TimeToTopSpeed / 1000;
            TimeToTopSpeedChanged(null, null);

            InvertMouseX.Checked = CBRE.Settings.View.InvertX;
            InvertMouseY.Checked = CBRE.Settings.View.InvertY;
            MouseWheelMoveDistance.Value = CBRE.Settings.View.MouseWheelMoveDistance;

            CameraFOV.Value = CBRE.Settings.View.CameraFOV;

            // Hotkeys
            UpdateHotkeyList();

            //Directories
        }

        private void Apply()
        {
            // 2D Views
            CBRE.Settings.View.CrosshairCursorIn2DViews = CrosshairCursorIn2DViews.Checked;
            CBRE.Settings.View.DrawEntityNames = DrawEntityNames.Checked;
            CBRE.Settings.View.DrawEntityAngles = DrawEntityAngles.Checked; //mxd

            if (RotationStyle_SnapOnShift.Checked) CBRE.Settings.Select.RotationStyle = RotationStyle.SnapOnShift;
            if (RotationStyle_SnapOffShift.Checked) CBRE.Settings.Select.RotationStyle = RotationStyle.SnapOffShift;
            if (RotationStyle_SnapNever.Checked) CBRE.Settings.Select.RotationStyle = RotationStyle.SnapNever;

            if (SnapStyle_SnapOffAlt.Checked) CBRE.Settings.Select.SnapStyle = SnapStyle.SnapOffAlt;
            if (SnapStyle_SnapOnAlt.Checked) CBRE.Settings.Select.SnapStyle = SnapStyle.SnapOnAlt;

            CBRE.Settings.Select.ArrowKeysNudgeSelection = ArrowKeysNudgeSelection.Checked;
            CBRE.Settings.Select.NudgeUnits = NudgeUnits.Value;
            if (NudgeStyle_GridOffCtrl.Checked) CBRE.Settings.Select.NudgeStyle = NudgeStyle.GridOffCtrl;
            if (NudgeStyle_GridOnCtrl.Checked) CBRE.Settings.Select.NudgeStyle = NudgeStyle.GridOnCtrl;

            Grid.DefaultSize = int.Parse(Convert.ToString(DefaultGridSize.Text));
            Grid.HideSmallerThan = int.Parse(Convert.ToString(HideGridLimit.Value));
            Grid.HideSmallerOn = HideGridOn.Checked;
            Grid.HideFactor = int.Parse(Convert.ToString(HideGridFactor.Text));

            Grid.Background = GridBackgroundColour.BackColor;
            Grid.GridLines = GridColour.BackColor;
            Grid.ZeroLines = GridZeroAxisColour.BackColor;
            Grid.BoundaryLines = GridBoundaryColour.BackColor;
            Grid.Highlight1 = GridHighlight1Colour.BackColor;
            Grid.Highlight2 = GridHighlight2Colour.BackColor;
            Grid.Highlight1LineNum = (int)GridHighlight1Distance.Value;
            Grid.Highlight2UnitNum = int.Parse(Convert.ToString(GridHighlight2UnitNum.Text));
            Grid.Highlight1On = GridHighlight1On.Checked;
            Grid.Highlight2On = GridHighlight2On.Checked;

            // 3D Views
            CBRE.Settings.View.ViewportBackground = ViewportBackgroundColour.BackColor;
            CBRE.Settings.View.BackClippingPane = BackClippingPane.Value;
            CBRE.Settings.View.ModelRenderDistance = ModelRenderDistance.Value;
            CBRE.Settings.View.DetailRenderDistance = DetailRenderDistance.Value;

            CBRE.Settings.View.ForwardSpeed = ForwardSpeed.Value;
            CBRE.Settings.View.TimeToTopSpeed = TimeToTopSpeed.Value * 100m;
            CBRE.Settings.View.InvertX = InvertMouseX.Checked;
            CBRE.Settings.View.InvertY = InvertMouseY.Checked;
            CBRE.Settings.View.MouseWheelMoveDistance = MouseWheelMoveDistance.Value;

            CBRE.Settings.View.CameraFOV = (int)CameraFOV.Value;

            // Hotkeys
            SettingsManager.Hotkeys.Clear();
            SettingsManager.Hotkeys.AddRange(_hotkeys);
            Hotkeys.SetupHotkeys(SettingsManager.Hotkeys);

            // Directories
            Directories.TextureDirs.Clear();
            for (int i = 0; i < textureDirsDataGrid.Rows.Count; i++)
            {
                string dir = textureDirsDataGrid.Rows[i].Cells[1].Value as string;
                if (!string.IsNullOrEmpty(dir))
                {
                    Directories.TextureDirs.Add(dir);
                }
            }
            Directories.ModelDirs.Clear();
            for (int i = 0; i < modelDirsDataGrid.Rows.Count; i++)
            {
                string dir = modelDirsDataGrid.Rows[i].Cells[1].Value as string;
                if (!string.IsNullOrEmpty(dir))
                {
                    Directories.ModelDirs.Add(dir);
                }
            }

            SettingsManager.Write();

            Mediator.Publish(EditorMediator.SettingsChanged);
        }

        private void Apply(object sender, EventArgs e)
        {
            btnApplySettings.Enabled = false;
            btnApplyAndCloseSettings.Enabled = false;
            btnCancelSettings.Enabled = false;
            Application.DoEvents();
            Apply();
            UpdateData();
            btnApplySettings.Enabled = true;
            btnApplyAndCloseSettings.Enabled = true;
            btnCancelSettings.Enabled = true;
        }

        private void ApplyAndClose(object sender, MouseEventArgs e)
        {
            btnApplySettings.Enabled = false;
            btnApplyAndCloseSettings.Enabled = false;
            btnCancelSettings.Enabled = false;
            Apply();
            Close();
        }

        private void Close(object sender, MouseEventArgs e)
        {
            Close();
        }

        private void SettingsFormClosed(object sender, FormClosedEventArgs e)
        {
            SettingsManager.Read();
        }

        #endregion

        #region Specific Events

        private void TabChanged(object sender, EventArgs e)
        {

        }

        private void BackClippingPaneChanged(object sender, EventArgs e)
        {
            if (BackClippingPane.Value != BackClippingPlaneUpDown.Value) BackClippingPlaneUpDown.Value = BackClippingPane.Value;
        }

        private void BackClippingPlaneUpDownValueChanged(object sender, EventArgs e)
        {
            if (BackClippingPlaneUpDown.Value != BackClippingPane.Value) BackClippingPane.Value = (int)BackClippingPlaneUpDown.Value;
        }

        private void ModelRenderDistanceChanged(object sender, EventArgs e)
        {
            if (ModelRenderDistance.Value != ModelRenderDistanceUpDown.Value) ModelRenderDistanceUpDown.Value = ModelRenderDistance.Value;
        }

        private void ModelRenderDistanceUpDownValueChanged(object sender, EventArgs e)
        {
            if (ModelRenderDistanceUpDown.Value != ModelRenderDistance.Value) ModelRenderDistance.Value = (int)ModelRenderDistanceUpDown.Value;
        }

        private void DetailRenderDistanceChanged(object sender, EventArgs e)
        {
            if (DetailRenderDistance.Value != DetailRenderDistanceUpDown.Value) DetailRenderDistanceUpDown.Value = DetailRenderDistance.Value;
        }

        private void DetailRenderDistanceUpDownValueChanged(object sender, EventArgs e)
        {
            if (DetailRenderDistanceUpDown.Value != DetailRenderDistance.Value) DetailRenderDistance.Value = (int)DetailRenderDistanceUpDown.Value;
        }

        private void ForwardSpeedChanged(object sender, EventArgs e)
        {
            if (ForwardSpeed.Value != ForwardSpeedUpDown.Value) ForwardSpeedUpDown.Value = ForwardSpeed.Value;
        }

        private void ForwardSpeedUpDownValueChanged(object sender, EventArgs e)
        {
            if (ForwardSpeedUpDown.Value != ForwardSpeed.Value) ForwardSpeed.Value = (int)ForwardSpeedUpDown.Value;
        }

        private void TimeToTopSpeedChanged(object sender, EventArgs e)
        {
            if (TimeToTopSpeed.Value != TimeToTopSpeedUpDown.Value) TimeToTopSpeedUpDown.Value = TimeToTopSpeed.Value / 10m;
        }

        private void TimeToTopSpeedUpDownValueChanged(object sender, EventArgs e)
        {
            if (TimeToTopSpeedUpDown.Value != TimeToTopSpeed.Value) TimeToTopSpeed.Value = (int)(TimeToTopSpeedUpDown.Value * 10);
        }

        private void SteamInstallDirBrowseClicked(object sender, EventArgs e)
        {

        }

        private void SteamUsernameChanged(object sender, EventArgs e)
        {
            SelectedGameUpdateSteamGames();
        }

        private void SteamDirectoryChanged(object sender, EventArgs e)
        {
            UpdateSteamUsernames();
            SelectedGameUpdateSteamGames();
        }

        private void UpdateSteamUsernames()
        {

        }

        private void RemoveGameClicked(object sender, EventArgs e)
        {

        }

        private void AddGameClicked(object sender, EventArgs e)
        {

        }

        private void AddBuildClicked(object sender, EventArgs e)
        {

        }

        private void RemoveBuildClicked(object sender, EventArgs e)
        {

        }

        #endregion

        #region Selected Game

        private void GameSelected(object sender, TreeViewEventArgs e)
        {

        }

        private void UpdateSelectedGame()
        {

        }

        private void SelectedGameUpdateAdditionalPackages()
        {

        }

        private void SelectedGameUpdateFgds()
        {

        }

        private void SelectedGameEngineChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameUpdateSteamGames()
        {

        }

        private void SelectedGameWonDirChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameSteamDirChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameNameChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameUseDiffAutosaveDirChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameDirBrowseClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameMapDirBrowseClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameDiffAutosaveDirBrowseClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameAddFgdClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameRemoveFgdClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameOverrideMapSizeChanged(object sender, EventArgs e)
        {

        }

        private void SelectedGameAddAdditionalPackageFileClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameAddAdditionalPackageFolderClicked(object sender, EventArgs e)
        {

        }

        private void SelectedGameRemoveAdditionalPackageClicked(object sender, EventArgs e)
        {

        }

        #endregion

        #region Hotkeys

        private class HotkeyQuickFormItem : QuickForms.Items.QuickFormTextBox
        {
            public HotkeyQuickFormItem(string tbname, string value) : base(tbname, value)
            {
            }
            public override List<Control> GetControls(QuickForm qf)
            {
                List<Control> ctrls = base.GetControls(qf);
                ctrls.OfType<TextBox>().First().KeyDown += HotkeyDown;
                return ctrls;
            }

            private void HotkeyDown(object sender, KeyEventArgs e)
            {
                e.SuppressKeyPress = e.Handled = true;
                ((TextBox)sender).Text = KeyboardState.KeysToString(e.KeyData);
            }
        }
        private void UpdateHotkeyList()
        {
            HotkeyList.BeginUpdate();
            int idx = HotkeyList.SelectedIndices.Count == 0 ? 0 : HotkeyList.SelectedIndices[0];
            HotkeyList.Items.Clear();
            foreach (Hotkey hotkey in _hotkeys.OrderBy(x => x.ID))
            {
                HotkeyDefinition def = Hotkeys.GetHotkeyDefinition(hotkey.ID);
                HotkeyList.Items.Add(new ListViewItem(new[]
                                                          {
                                                              def.Name,
                                                              def.Description,
                                                              String.IsNullOrWhiteSpace(hotkey.HotkeyString)
                                                                  ? "<unassigned>"
                                                                  : hotkey.HotkeyString
                                                          })
                { Tag = hotkey });
            }
            HotkeyList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            if (idx >= 0 && idx < HotkeyList.Items.Count) HotkeyList.Items[idx].Selected = true;
            HotkeyList.EndUpdate();

            HotkeyActionList.BeginUpdate();
            idx = HotkeyActionList.SelectedIndex;
            HotkeyActionList.Items.Clear();
            foreach (HotkeyDefinition def in Hotkeys.GetHotkeyDefinitions().OrderBy(x => x.ID))
            {
                HotkeyActionList.Items.Add(def);
            }
            if (idx < 0 || idx >= HotkeyActionList.Items.Count) idx = 0;
            HotkeyActionList.SelectedIndex = idx;
            HotkeyActionList.EndUpdate();
        }

        private void DeleteHotkey(Hotkey hk)
        {
            List<Hotkey> others = _hotkeys.Where(x => x.ID == hk.ID && x != hk).ToList();
            if (others.Any()) _hotkeys.Remove(hk);
            else hk.HotkeyString = "";
            UpdateHotkeyList();
        }

        private void EditHotkey(Hotkey hk)
        {
            using (QuickForm qf = new QuickForm("Enter New Hotkey")
                .Item(new HotkeyQuickFormItem("Hotkey", hk.HotkeyString))
                .OkCancel())
            {
                if (qf.ShowDialog() != DialogResult.OK) return;
                string key = qf.String("Hotkey");
                if (String.IsNullOrWhiteSpace(key)) return;

                Hotkey conflict = _hotkeys.FirstOrDefault(x => x.HotkeyString == key && x != hk);
                if (conflict != null)
                {
                    if (DarkMessageBox.Show(key + " is already assigned to \"" + Hotkeys.GetHotkeyDefinition(conflict.ID) + "\".\n" +
                                        "Continue anyway?", "Conflict Detected", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return;
                    }
                }

                hk.HotkeyString = key;
                UpdateHotkeyList();
            }
        }

        private void HotkeyListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && HotkeyList.SelectedItems.Count == 1)
            {
                DeleteHotkey((Hotkey)HotkeyList.SelectedItems[0].Tag);
            }
            else if (e.KeyCode == Keys.Enter && HotkeyList.SelectedItems.Count == 1)
            {
                EditHotkey((Hotkey)HotkeyList.SelectedItems[0].Tag);
            }
        }

        private void HotkeyListDoubleClicked(object sender, MouseEventArgs e)
        {
            if (HotkeyList.SelectedItems.Count == 1)
            {
                EditHotkey((Hotkey)HotkeyList.SelectedItems[0].Tag);
            }
        }

        private void HotkeyReassignButtonClicked(object sender, EventArgs e)
        {
            if (HotkeyList.SelectedItems.Count == 1)
            {
                EditHotkey((Hotkey)HotkeyList.SelectedItems[0].Tag);
            }
        }

        private void HotkeyRemoveButtonClicked(object sender, EventArgs e)
        {
            if (HotkeyList.SelectedItems.Count == 1)
            {
                DeleteHotkey((Hotkey)HotkeyList.SelectedItems[0].Tag);
            }
        }

        private void HotkeyAddButtonClicked(object sender, EventArgs e)
        {
            string key = HotkeyCombination.Text;
            if (HotkeyActionList.SelectedIndex <= 0 || String.IsNullOrWhiteSpace(key)) return;
            Hotkey conflict = _hotkeys.FirstOrDefault(x => x.HotkeyString == key);
            if (conflict != null)
            {
                if (DarkMessageBox.Show(key + " is already assigned to \"" + Hotkeys.GetHotkeyDefinition(conflict.ID) + "\".\n" +
                                    "Continue anyway?", "Conflict Detected", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
            HotkeyDefinition def = (HotkeyDefinition)HotkeyActionList.SelectedItem;
            Hotkey blank = _hotkeys.FirstOrDefault(x => x.ID == def.ID && String.IsNullOrWhiteSpace(x.HotkeyString));
            if (blank == null) _hotkeys.Add(new Hotkey { ID = def.ID, HotkeyString = key });
            else blank.HotkeyString = key;
            HotkeyCombination.Text = "";
            UpdateHotkeyList();
        }

        private void HotkeyCombinationKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            e.Handled = true;
            HotkeyCombination.Text = KeyboardState.KeysToString(e.KeyData);
        }

        protected override bool ProcessTabKey(bool forward)
        {
            if (HotkeyCombination.Focused) return false;
            return base.ProcessTabKey(forward);
        }

        private void HotkeyResetButtonClicked(object sender, EventArgs e)
        {
            _hotkeys.Clear();
            foreach (HotkeyDefinition def in Hotkeys.GetHotkeyDefinitions())
            {
                foreach (string hk in def.DefaultHotkeys)
                {
                    _hotkeys.Add(new Hotkey { ID = def.ID, HotkeyString = hk });
                }
            }
            UpdateHotkeyList();
        }
        #endregion

        private void OpenBrowseDirDialog(Action<string> callback)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                BeginInvoke(new MethodInvoker(() => callback?.Invoke(dialog.FileName)));
            }
        }

        private void TextureDirsDataGrid_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            BeginInvoke(new MethodInvoker(() => DirsChanged(textureDirsDataGrid)));
        }

        private void TextureDirsDataGrid_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BeginInvoke(new MethodInvoker(() => DirsChanged(textureDirsDataGrid)));
            }
        }

        private void ModelDirsDataGrid_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            BeginInvoke(new MethodInvoker(() => DirsChanged(modelDirsDataGrid)));
        }

        private void ModelDirsDataGrid_KeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BeginInvoke(new MethodInvoker(() => DirsChanged(modelDirsDataGrid)));
            }
        }

        private void DirsChanged(DataGridView dataGridView)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView.Rows[i];
                string dir = row.Cells[1].Value as string;
                if (!string.IsNullOrEmpty(dir))
                {
                    dir = dir.Replace('\\', '/');
                    if (dir.Last() != '/')
                    {
                        dir += "/";
                    }
                }
                if (Directory.Exists(dir))
                {
                    row.Cells[1].Value = dir;
                    if (i >= dataGridView.Rows.Count - 1)
                    {
                        int newRowInd = AddDirsRow(dataGridView, "");
                        if (dataGridView.CurrentRow.Index == i)
                        {
                            dataGridView.CurrentCell =
                                dataGridView.Rows[newRowInd].Cells[1];
                        }
                    }
                }
                else if (i < dataGridView.Rows.Count - 1)
                {
                    dataGridView.Rows.RemoveAt(i);
                    i--;
                }
                else
                {
                    row.Cells[1].Value = "";
                }
            }
        }

        private int AddDirsRow(DataGridView dataGridView, string dir)
        {
            int r = dataGridView.Rows.Add("X", dir, "...");
            return r;
        }

        private ComboBox AddThemeDropdown() {
            PropertyInfo property = typeof(General).GetProperty("Theme");
            ComboBox combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300
            };
            foreach (var val in ThemeManager.AvailableThemes)
            {
                combo.Items.Add(val);
            }
            combo.SelectedIndex = GetIndex(General.Theme);
            
            int GetIndex(string theme) {
                if (string.IsNullOrEmpty(theme)) return 0;
                var count = ThemeManager.AvailableThemes.TakeWhile(x => x != theme).Count();
                if (count == ThemeManager.AvailableThemes.Count) return 0;
                return count;
            }

            combo.SelectedIndexChanged += (s, e)
                => property.SetValue(null, combo.SelectedIndex == 0
                    ? null
                    : ThemeManager.AvailableThemes[combo.SelectedIndex]);
            Label label = new Label
            {
                AutoSize = true,
                Text = "Theme (requires restart)",
                Padding = new Padding(0, 5, 0, 5)
            };
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true
            };
            panel.Controls.Add(label);
            panel.Controls.Add(combo);
            flowLayoutPanel1.Controls.Add(panel);

            return combo;
        }
    }
}
