using CBRE.Common.Mediator;
using CBRE.Editor.Documents;
using CBRE.Editor.Properties;
using CBRE.Settings;
using CBRE.Settings.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CBRE.Editor.Menu
{
    public static class MenuManager
    {
        private static MenuStrip _menu;
        private static ToolStripContainer _container;
        private static readonly Dictionary<string, List<IMenuBuilder>> MenuItems;

        public static List<RecentFile> RecentFiles { get; private set; }

        static MenuManager()
        {
            RecentFiles = new List<RecentFile>();
            MenuItems = new Dictionary<string, List<IMenuBuilder>>();
        }

        public static void Init(MenuStrip menu, ToolStripContainer toolStripContainer)
        {
            _menu = menu;
            _container = toolStripContainer;
            AddDefault();
        }

        public static void Add(string menuName, IMenuBuilder builder)
        {
            if (!MenuItems.ContainsKey(menuName)) MenuItems.Add(menuName, new List<IMenuBuilder>());
            MenuItems[menuName].Add(builder);
        }

        public static void Insert(string menuName, int index, IMenuBuilder builder)
        {
            if (!MenuItems.ContainsKey(menuName)) MenuItems.Add(menuName, new List<IMenuBuilder>());
            if (index < 0) index = 0;
            if (index > MenuItems[menuName].Count) index = MenuItems[menuName].Count;
            MenuItems[menuName].Insert(index, builder);
        }

        private static void AddDefault()
        {
            Func<bool> mapOpen = () => DocumentManager.CurrentDocument != null;
            Add("File", new SimpleMenuBuilder("New", HotkeysMediator.FileNew) { Image = Resources.Menu_New, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Open", HotkeysMediator.FileOpen) { Image = Resources.Menu_Open, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Close", HotkeysMediator.FileClose) { IsVisible = mapOpen, Image = Resources.Menu_Close, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Save", HotkeysMediator.FileSave) { IsVisible = mapOpen, Image = Resources.Menu_Save, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Save As...", HotkeysMediator.FileSaveAs) { Image = Resources.Menu_SaveAs, IsVisible = mapOpen });
            Add("File", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Export / Lightmap", HotkeysMediator.FileCompile) { Image = Resources.Menu_ExportRmesh, IsVisible = mapOpen, ShowInToolStrip = true });
            Add("File", new SimpleMenuBuilder("Bulk Export", HotkeysMediator.FileBulkExport) { Image = Resources.Menu_ExportRmesh, IsVisible = () => true, ShowInToolStrip = true });
            Add("File", new RecentFilesMenu());
            Add("File", new MenuSplitter());
            Add("File", new SimpleMenuBuilder("Exit", EditorMediator.Exit));

            Func<bool> canUndo = () => mapOpen() && DocumentManager.CurrentDocument.History.CanUndo();
            Func<bool> canRedo = () => mapOpen() && DocumentManager.CurrentDocument.History.CanRedo();
            Func<string> undoText = () => mapOpen() ? DocumentManager.CurrentDocument.History.GetUndoString() : "Undo";
            Func<string> redoText = () => mapOpen() ? DocumentManager.CurrentDocument.History.GetRedoString() : "Redo";
            Func<bool> itemsSelected = () => mapOpen() && DocumentManager.CurrentDocument.Selection.GetSelectedObjects().Any();
            Func<bool> canPaste = Clipboard.ClipboardManager.CanPaste;
            Add("Edit", new SimpleMenuBuilder("Undo", HotkeysMediator.HistoryUndo) { Image = Resources.Menu_Undo, IsVisible = mapOpen, IsActive = canUndo, Text = undoText, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Redo", HotkeysMediator.HistoryRedo) { Image = Resources.Menu_Redo, IsVisible = mapOpen, IsActive = canRedo, Text = redoText, ShowInToolStrip = true });
            Add("Edit", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Cut", HotkeysMediator.OperationsCut) { Image = Resources.Menu_Cut, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Copy", HotkeysMediator.OperationsCopy) { Image = Resources.Menu_Copy, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Paste", HotkeysMediator.OperationsPaste) { Image = Resources.Menu_Paste, IsVisible = mapOpen, IsActive = canPaste, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Paste Special...", HotkeysMediator.OperationsPasteSpecial) { Image = Resources.Menu_PasteSpecial, IsVisible = mapOpen, IsActive = canPaste, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Delete", HotkeysMediator.OperationsDelete) { Image = Resources.Menu_Delete, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Edit", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("Edit", new SimpleMenuBuilder("Clear Selection", HotkeysMediator.SelectionClear) { Image = Resources.Menu_ClearSelection, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Edit", new SimpleMenuBuilder("Select All", HotkeysMediator.SelectAll) { Image = Resources.Menu_SelectAll, IsVisible = mapOpen });
            Add("Edit", new MenuSplitter { IsVisible = mapOpen });
            Add("Edit", new SimpleMenuBuilder("Object Properties", HotkeysMediator.ObjectProperties) { Image = Resources.Menu_ObjectProperties, IsVisible = mapOpen, ShowInToolStrip = true });

            Add("Map", new SimpleMenuBuilder("Snap to Grid", HotkeysMediator.ToggleSnapToGrid) { Image = Resources.Menu_SnapToGrid, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.SnapToGrid, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Show 2D Grid", HotkeysMediator.ToggleShow2DGrid) { Image = Resources.Menu_Show2DGrid, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.Show2DGrid, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Show 3D Grid", HotkeysMediator.ToggleShow3DGrid) { Image = Resources.Menu_Show3DGrid, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.Show3DGrid, ShowInToolStrip = true });
            Add("Map", new GroupedMenuBuilder("Grid Settings",
                                              new SimpleMenuBuilder("Smaller Grid", HotkeysMediator.GridDecrease) { Image = Resources.Menu_SmallerGrid, IsVisible = mapOpen, ShowInToolStrip = true },
                                              new SimpleMenuBuilder("Bigger Grid", HotkeysMediator.GridIncrease) { Image = Resources.Menu_LargerGrid, IsVisible = mapOpen, ShowInToolStrip = true }
                           )
            { IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Smaller Grid", HotkeysMediator.GridDecrease) { Image = Resources.Menu_SmallerGrid, IsVisible = mapOpen, ShowInToolStrip = true, ShowInMenu = false });
            Add("Map", new SimpleMenuBuilder("Bigger Grid", HotkeysMediator.GridIncrease) { Image = Resources.Menu_LargerGrid, IsVisible = mapOpen, ShowInToolStrip = true, ShowInMenu = false });
            Add("Map", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Ignore Grouping", HotkeysMediator.ToggleIgnoreGrouping) { Image = Resources.Menu_IgnoreGrouping, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.IgnoreGrouping, ShowInToolStrip = true });
            Add("Map", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Texture Lock", HotkeysMediator.ToggleTextureLock) { Image = Resources.Menu_TextureLock, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.TextureLock, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Texture Scaling Lock", HotkeysMediator.ToggleTextureScalingLock) { Image = Resources.Menu_TextureScalingLock, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.TextureScalingLock, ShowInToolStrip = true });
            Add("Map", new MenuSplitter { IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Hide Tool Textures", HotkeysMediator.ToggleHideToolTextures) { Image = Resources.Menu_HideToolTextures, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.HideToolTextures, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Hide Entity Sprites", HotkeysMediator.ToggleHideEntitySprites) { Image = Resources.Menu_HideEntitySprites, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.HideEntitySprites, ShowInToolStrip = true });
            Add("Map", new SimpleMenuBuilder("Hide Map Origin", HotkeysMediator.ToggleHideMapOrigin) { Image = Resources.Menu_HideMapOrigin, IsVisible = mapOpen, IsChecked = () => mapOpen() && DocumentManager.CurrentDocument.Map.HideMapOrigin, ShowInToolStrip = true });
            Add("Map", new MenuSplitter { IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Show Information", HotkeysMediator.ShowMapInformation) { Image = Resources.Menu_ShowInformation, IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Show Selected Brush ID", HotkeysMediator.ShowSelectedBrushID) { Image = Resources.Menu_ShowBrushID, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Map", new SimpleMenuBuilder("Entity Report...", HotkeysMediator.ShowEntityReport) { Image = Resources.Menu_EntityReport, IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Show Logical Tree", HotkeysMediator.ShowLogicalTree) { Image = Resources.Menu_ShowLogicalTree, IsVisible = mapOpen });
            Add("Map", new MenuSplitter { IsVisible = mapOpen });
            Add("Map", new SimpleMenuBuilder("Map Properties...", EditorMediator.WorldspawnProperties) { Image = Resources.Menu_MapProperties, IsVisible = mapOpen });

            Add("View", new SimpleMenuBuilder("Autosize All Views", HotkeysMediator.ViewportAutosize) { Image = Resources.Menu_AutosizeViews, IsVisible = mapOpen });
            Add("View", new SimpleMenuBuilder("Center All Views on Selection", HotkeysMediator.CenterAllViewsOnSelection) { Image = Resources.Menu_CenterSelectionAll, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("View", new SimpleMenuBuilder("Center 2D Views on Selection", HotkeysMediator.Center2DViewsOnSelection) { Image = Resources.Menu_CenterSelection2D, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("View", new SimpleMenuBuilder("Center 3D View on Selection", HotkeysMediator.Center3DViewsOnSelection) { Image = Resources.Menu_CenterSelection3D, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("View", new MenuSplitter { IsVisible = mapOpen });
            Add("View", new SimpleMenuBuilder("Go to Brush ID...", HotkeysMediator.GoToBrushID) { Image = Resources.Menu_GoToBrushID, IsVisible = mapOpen });
            Add("View", new SimpleMenuBuilder("Go to Coordinates...", HotkeysMediator.GoToCoordinates) { Image = Resources.Menu_GoToCoordinates, IsVisible = mapOpen });
            Add("View", new MenuSplitter { IsVisible = mapOpen });
            Add("View", new SimpleMenuBuilder("Hide Selected Objects", HotkeysMediator.QuickHideSelected) { Image = Resources.Menu_HideSelected, IsVisible = mapOpen, ShowInToolStrip = true });
            Add("View", new SimpleMenuBuilder("Hide Unselected Objects", HotkeysMediator.QuickHideUnselected) { Image = Resources.Menu_HideUnselected, IsVisible = mapOpen, ShowInToolStrip = true });
            Add("View", new SimpleMenuBuilder("Show Hidden Objects", HotkeysMediator.QuickHideShowAll) { Image = Resources.Menu_ShowHidden, IsVisible = mapOpen, ShowInToolStrip = true });

            Add("Tools", new SimpleMenuBuilder("Carve", HotkeysMediator.Carve) { Image = Resources.Menu_Carve, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Tools", new SimpleMenuBuilder("Make Hollow", HotkeysMediator.MakeHollow) { Image = Resources.Menu_Hollow, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Tools", new MenuSplitter { IsVisible = mapOpen });
            Add("Tools", new SimpleMenuBuilder("Group", HotkeysMediator.GroupingGroup) { Image = Resources.Menu_Group, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Tools", new SimpleMenuBuilder("Ungroup", HotkeysMediator.GroupingUngroup) { Image = Resources.Menu_Ungroup, IsVisible = mapOpen, IsActive = itemsSelected, ShowInToolStrip = true });
            Add("Tools", new MenuSplitter { IsVisible = mapOpen, ShowInToolStrip = true });
            Add("Tools", new SimpleMenuBuilder("Tie to Entity", HotkeysMediator.TieToEntity) { Image = Resources.Menu_TieToEntity, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Tools", new SimpleMenuBuilder("Move to World", HotkeysMediator.TieToWorld) { Image = Resources.Menu_TieToWorld, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Tools", new MenuSplitter { IsVisible = mapOpen });
            Add("Tools", new SimpleMenuBuilder("Replace Textures", HotkeysMediator.ReplaceTextures) { Image = Resources.Menu_ReplaceTextures, IsVisible = mapOpen });
            Add("Tools", new MenuSplitter { IsVisible = mapOpen });
            Add("Tools", new SimpleMenuBuilder("Transform...", HotkeysMediator.Transform) { Image = Resources.Menu_Transform, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Tools", new SimpleMenuBuilder("Snap Selected to Grid", HotkeysMediator.SnapSelectionToGrid) { Image = Resources.Menu_SnapSelection, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Tools", new SimpleMenuBuilder("Snap Selected to Grid Individually", HotkeysMediator.SnapSelectionToGridIndividually) { Image = Resources.Menu_SnapSelectionIndividual, IsVisible = mapOpen, IsActive = itemsSelected });
            Add("Tools", new GroupedMenuBuilder("Align Objects",
                                                new SimpleMenuBuilder("To X Axis Min", HotkeysMediator.AlignXMin) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("To X Axis Max", HotkeysMediator.AlignXMax) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("To Y Axis Min", HotkeysMediator.AlignZMin) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("To Y Axis Max", HotkeysMediator.AlignZMax) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("To Z Axis Min", HotkeysMediator.AlignYMin) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("To Z Axis Max", HotkeysMediator.AlignYMax) { IsVisible = mapOpen, IsActive = itemsSelected }
                             )
            { Image = Resources.Menu_Align, IsVisible = mapOpen });
            Add("Tools", new GroupedMenuBuilder("Flip Objects",
                                                new SimpleMenuBuilder("X Axis", HotkeysMediator.FlipX) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("Y Axis", HotkeysMediator.FlipZ) { IsVisible = mapOpen, IsActive = itemsSelected },
                                                new SimpleMenuBuilder("Z Axis", HotkeysMediator.FlipY) { IsVisible = mapOpen, IsActive = itemsSelected }
                             )
            { Image = Resources.Menu_Flip, IsVisible = mapOpen });
            Add("Tools", new MenuSplitter { IsVisible = mapOpen });
            Add("Tools", new SimpleMenuBuilder("Options...", EditorMediator.OpenSettings) { Image = Resources.Menu_Options, ShowInToolStrip = true });

            Add("Help", new SimpleMenuBuilder("Report A Bug...", EditorMediator.OpenWebsite, Editor.GITHUB_REPORT_BUG_URL) { Image = Resources.Menu_GitHub });
            Add("Help", new MenuSplitter { IsVisible = mapOpen });
            Add("Help", new SimpleMenuBuilder("About CBRE-EX", EditorMediator.About) { Image = Resources.Menu_ShowInformation });
        }

        public static void UpdateRecentFilesMenu()
        {
            RebuildPartial("File");
        }

        public static void RebuildPartial(string name)
        {
            if (!MenuItems.ContainsKey(name)) return;
            List<IMenuBuilder> mi = MenuItems[name];

            foreach (ToolStripMenuItem menu in _menu.Items)
            {
                if (menu.Text != name) continue;
                menu.DropDownItems.Clear();
                menu.DropDownItems.AddRange(mi.Where(x => x.ShowInMenu).SelectMany(x => x.Build()).ToArray());
            }
        }

        public static void Rebuild()
        {
            if (_menu == null || _container == null) return;
            foreach (ToolStripMenuItem mi in _menu.Items)
            {
                mi.DropDownOpening -= DropDownOpening;
            }
            _menu.Items.Clear();
            List<ToolStripMenuItem> removeMenu = _menu.Items.OfType<ToolStripMenuItem>().ToList();
            foreach (KeyValuePair<string, List<IMenuBuilder>> kv in MenuItems)
            {
                ToolStripMenuItem mi = removeMenu.FirstOrDefault(x => x.Text == kv.Key) ?? new ToolStripMenuItem(kv.Key);
                mi.DropDownItems.Clear();
                mi.DropDownItems.AddRange(kv.Value.Where(x => x.ShowInMenu).SelectMany(x => x.Build()).ToArray());
                if (mi.DropDownItems.Count <= 0) continue;
                removeMenu.Remove(mi);
                mi.DropDownOpening += DropDownOpening;
                if (!_menu.Items.Contains(mi)) _menu.Items.Add(mi);
            }
            foreach (ToolStripMenuItem rem in removeMenu)
            {
                _menu.Items.Remove(rem);
            }
            // Need to remove and re-add tool strips because the ordering is incorrect otherwise
            List<ToolStrip> removeToolbar = _container.Controls.OfType<ToolStripPanel>()
                .SelectMany(x => x.Controls.OfType<ToolStrip>())
                .Where(control => MenuItems.Any(x => x.Key == control.Name))
                .ToList();
            foreach (KeyValuePair<string, List<IMenuBuilder>> kv in MenuItems.Reverse())
            {
                ToolStrip ts = removeToolbar.FirstOrDefault(x => x.Name == kv.Key) ?? new ToolStrip { Name = kv.Key };
                // TODO Match by name, only remove items that don't match
                ts.Items.Clear();
                ts.Items.AddRange(kv.Value.Where(x => x.ShowInToolStrip).SelectMany(x => x.BuildToolStrip()).ToArray());
                if (ts.Items.Count > 0)
                {
                    if (!removeToolbar.Contains(ts)) _container.TopToolStripPanel.Join(ts);
                    removeToolbar.Remove(ts);
                }
            }
            foreach (ToolStrip control in removeToolbar)
            {
                control.Parent.Controls.Remove(control);
                control.Dispose();
            }
        }

        private static void DropDownOpening(object sender, EventArgs e)
        {
            Mediator.Publish(EditorMediator.UpdateMenu);
        }
    }
}
