using CBRE.Common;
using CBRE.Common.Mediator;
using CBRE.DataStructures.GameData;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Transformations;
using CBRE.Editor.Actions;
using CBRE.Editor.Actions.MapObjects.Groups;
using CBRE.Editor.Actions.MapObjects.Operations;
using CBRE.Editor.Actions.MapObjects.Operations.EditOperations;
using CBRE.Editor.Actions.MapObjects.Selection;
using CBRE.Editor.Actions.Visgroups;
using CBRE.Editor.Clipboard;
using CBRE.Editor.Compiling;
using CBRE.Editor.Enums;
using CBRE.Editor.Rendering;
using CBRE.Editor.Rendering.Helpers;
using CBRE.Editor.Tools;
using CBRE.Editor.Tools.SelectTool;
using CBRE.Editor.UI;
using CBRE.Editor.UI.ObjectProperties;
using CBRE.Editor.Visgroups;
using CBRE.Extensions;
using CBRE.Providers.Texture;
using CBRE.QuickForms;
using CBRE.QuickForms.Items;
using CBRE.Settings;
using CBRE.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DarkModeForms;
using Quaternion = CBRE.DataStructures.Geometric.Quaternion;

namespace CBRE.Editor.Documents
{
    /// <summary>
    /// A simple container to separate out the document mediator listeners from the document itself.
    /// </summary>
    public class DocumentSubscriptions : IMediatorListener
    {
        private readonly Document _document;

        public DocumentSubscriptions(Document document)
        {
            _document = document;
        }

        public void Subscribe()
        {
            Mediator.Subscribe(EditorMediator.DocumentTreeStructureChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeObjectsChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeSelectedObjectsChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeFacesChanged, this);
            Mediator.Subscribe(EditorMediator.DocumentTreeSelectedFacesChanged, this);

            Mediator.Subscribe(EditorMediator.SettingsChanged, this);

            Mediator.Subscribe(HotkeysMediator.FileClose, this);
            Mediator.Subscribe(HotkeysMediator.FileSave, this);
            Mediator.Subscribe(HotkeysMediator.FileSaveAs, this);
            //Mediator.Subscribe(HotkeysMediator.FileExport, this);
            Mediator.Subscribe(HotkeysMediator.FileCompile, this);

            Mediator.Subscribe(HotkeysMediator.HistoryUndo, this);
            Mediator.Subscribe(HotkeysMediator.HistoryRedo, this);

            Mediator.Subscribe(HotkeysMediator.OperationsCopy, this);
            Mediator.Subscribe(HotkeysMediator.OperationsCut, this);
            Mediator.Subscribe(HotkeysMediator.OperationsPaste, this);
            Mediator.Subscribe(HotkeysMediator.OperationsPasteSpecial, this);
            Mediator.Subscribe(HotkeysMediator.OperationsDelete, this);
            Mediator.Subscribe(HotkeysMediator.SelectionClear, this);
            Mediator.Subscribe(HotkeysMediator.SelectAll, this);
            Mediator.Subscribe(HotkeysMediator.ObjectProperties, this);

            Mediator.Subscribe(HotkeysMediator.QuickHideSelected, this);
            Mediator.Subscribe(HotkeysMediator.QuickHideUnselected, this);
            Mediator.Subscribe(HotkeysMediator.QuickHideShowAll, this);

            Mediator.Subscribe(HotkeysMediator.SwitchTool, this);
            Mediator.Subscribe(HotkeysMediator.ApplyCurrentTextureToSelection, this);

            Mediator.Subscribe(HotkeysMediator.RotateClockwise, this);
            Mediator.Subscribe(HotkeysMediator.RotateCounterClockwise, this);

            Mediator.Subscribe(HotkeysMediator.Carve, this);
            Mediator.Subscribe(HotkeysMediator.MakeHollow, this);
            Mediator.Subscribe(HotkeysMediator.GroupingGroup, this);
            Mediator.Subscribe(HotkeysMediator.GroupingUngroup, this);
            Mediator.Subscribe(HotkeysMediator.TieToEntity, this);
            Mediator.Subscribe(HotkeysMediator.TieToWorld, this);
            Mediator.Subscribe(HotkeysMediator.Transform, this);
            Mediator.Subscribe(HotkeysMediator.ReplaceTextures, this);
            Mediator.Subscribe(HotkeysMediator.SnapSelectionToGrid, this);
            Mediator.Subscribe(HotkeysMediator.SnapSelectionToGridIndividually, this);
            Mediator.Subscribe(HotkeysMediator.AlignXMax, this);
            Mediator.Subscribe(HotkeysMediator.AlignXMin, this);
            Mediator.Subscribe(HotkeysMediator.AlignYMax, this);
            Mediator.Subscribe(HotkeysMediator.AlignYMin, this);
            Mediator.Subscribe(HotkeysMediator.AlignZMax, this);
            Mediator.Subscribe(HotkeysMediator.AlignZMin, this);
            Mediator.Subscribe(HotkeysMediator.FlipX, this);
            Mediator.Subscribe(HotkeysMediator.FlipY, this);
            Mediator.Subscribe(HotkeysMediator.FlipZ, this);

            Mediator.Subscribe(HotkeysMediator.GridIncrease, this);
            Mediator.Subscribe(HotkeysMediator.GridDecrease, this);
            Mediator.Subscribe(HotkeysMediator.CenterAllViewsOnSelection, this);
            Mediator.Subscribe(HotkeysMediator.Center2DViewsOnSelection, this);
            Mediator.Subscribe(HotkeysMediator.Center3DViewsOnSelection, this);
            Mediator.Subscribe(HotkeysMediator.GoToBrushID, this);
            Mediator.Subscribe(HotkeysMediator.GoToCoordinates, this);

            Mediator.Subscribe(HotkeysMediator.ToggleSnapToGrid, this);
            Mediator.Subscribe(HotkeysMediator.ToggleShow2DGrid, this);
            Mediator.Subscribe(HotkeysMediator.ToggleShow3DGrid, this);
            Mediator.Subscribe(HotkeysMediator.ToggleIgnoreGrouping, this);
            Mediator.Subscribe(HotkeysMediator.ToggleTextureLock, this);
            Mediator.Subscribe(HotkeysMediator.ToggleTextureScalingLock, this);
            Mediator.Subscribe(HotkeysMediator.ToggleHideFaceMask, this);
            Mediator.Subscribe(HotkeysMediator.ToggleHideDisplacementSolids, this);
            Mediator.Subscribe(HotkeysMediator.ToggleHideToolTextures, this);
            Mediator.Subscribe(HotkeysMediator.ToggleHideEntitySprites, this);
            Mediator.Subscribe(HotkeysMediator.ToggleHideMapOrigin, this);

            Mediator.Subscribe(HotkeysMediator.ShowSelectedBrushID, this);
            Mediator.Subscribe(HotkeysMediator.ShowMapInformation, this);
            Mediator.Subscribe(HotkeysMediator.ShowLogicalTree, this);
            Mediator.Subscribe(HotkeysMediator.ShowEntityReport, this);
            Mediator.Subscribe(HotkeysMediator.CheckForProblems, this);

            Mediator.Subscribe(EditorMediator.ViewportRightClick, this);

            Mediator.Subscribe(EditorMediator.WorldspawnProperties, this);

            Mediator.Subscribe(EditorMediator.VisgroupSelect, this);
            Mediator.Subscribe(EditorMediator.VisgroupShowAll, this);
            Mediator.Subscribe(EditorMediator.VisgroupShowEditor, this);
            Mediator.Subscribe(EditorMediator.VisgroupToggled, this);
            Mediator.Subscribe(HotkeysMediator.VisgroupCreateNew, this);
            Mediator.Subscribe(EditorMediator.SetZoomValue, this);
            Mediator.Subscribe(EditorMediator.TextureSelected, this);
            Mediator.Subscribe(EditorMediator.SelectMatchingTextures, this);

            Mediator.Subscribe(EditorMediator.ViewportCreated, this);
        }

        public void Unsubscribe()
        {
            Mediator.UnsubscribeAll(this);
        }

        public void Notify(string message, object data)
        {
            HotkeysMediator val;
            if (ToolManager.ActiveTool != null && Enum.TryParse(message, true, out val))
            {
                HotkeyInterceptResult result = ToolManager.ActiveTool.InterceptHotkey(val, data);
                if (result == HotkeyInterceptResult.Abort) return;
                if (result == HotkeyInterceptResult.SwitchToSelectTool)
                {
                    ToolManager.Activate(typeof(SelectTool));
                }
            }
            if (!Mediator.ExecuteDefault(this, message, data))
            {
                throw new Exception("Invalid document message: " + message + ", with data: " + data);
            }
        }

        // ReSharper disable UnusedMember.Global
        // ReSharper disable MemberCanBePrivate.Global

        private void DocumentTreeStructureChanged()
        {
            _document.RenderAll();
        }

        private void DocumentTreeObjectsChanged(IEnumerable<MapObject> objects)
        {
            _document.RenderObjects(objects);
        }

        private void DocumentTreeSelectedObjectsChanged(IEnumerable<MapObject> objects)
        {
            _document.RenderSelection(objects);
        }

        private void DocumentTreeFacesChanged(IEnumerable<Face> faces)
        {
            _document.RenderFaces(faces);
        }

        private void DocumentTreeSelectedFacesChanged(IEnumerable<Face> faces)
        {
            _document.RenderSelection(faces.Select(x => x.Parent).Distinct());
        }

        public void SettingsChanged()
        {
            _document.HelperManager.UpdateCache();
            RebuildGrid();
            _document.RenderAll();
        }

        public void HistoryUndo()
        {
            _document.History.Undo();
        }

        public void HistoryRedo()
        {
            _document.History.Redo();
        }

        public void FileClose()
        {
            if (_document.History.TotalActionsSinceLastSave > 0)
            {
                DialogResult result = DarkMessageBox.Show("Would you like to save your changes to this map?", "Changes Detected", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel) return;
                if (result == DialogResult.Yes) FileSave();
            }
            DocumentManager.Remove(_document);
        }

        public void FileSave()
        {
            _document.SaveFile();
        }

        public void FileSaveAs()
        {
            _document.SaveFile(null, true);
        }

        public void FileCompile()
        {
            ExportForm form = new ExportForm();
            form.Document = _document;
            form.ShowDialog();
        }

        public void OperationsCopy()
        {
            if (!_document.Selection.IsEmpty() && !_document.Selection.InFaceSelection)
            {
                ClipboardManager.Push(_document.Selection.GetSelectedObjects());
            }
        }

        public void OperationsCut()
        {
            OperationsCopy();
            OperationsDelete();
        }

        public void OperationsPaste()
        {
            if (!ClipboardManager.CanPaste()) return;

            IEnumerable<MapObject> content = ClipboardManager.GetPastedContent(_document);
            if (content == null) return;

            List<MapObject> list = content.ToList();
            if (!list.Any()) return;

            list.SelectMany(x => x.FindAll()).ToList().ForEach(x => x.IsSelected = true);
            _document.Selection.SwitchToObjectSelection();

            string name = "Pasted " + list.Count + " item" + (list.Count == 1 ? "" : "s");
            List<MapObject> selected = _document.Selection.GetSelectedObjects().ToList();
            _document.PerformAction(name, new ActionCollection(
                                              new Deselect(selected), // Deselect the current objects
                                              new Create(_document.Map.WorldSpawn.ID, list))); // Add and select the new objects
        }

        public void OperationsPasteSpecial()
        {
            if (!ClipboardManager.CanPaste()) return;

            IEnumerable<MapObject> content = ClipboardManager.GetPastedContent(_document);
            if (content == null) return;

            List<MapObject> list = content.ToList();
            if (!list.Any()) return;

            foreach (Face face in list.SelectMany(x => x.FindAll().OfType<Solid>().SelectMany(y => y.Faces)))
            {
                face.Texture.Texture = _document.GetTexture(face.Texture.Name);
            }

            Box box = new Box(list.Select(x => x.BoundingBox));

            using (PasteSpecialDialog psd = new PasteSpecialDialog(box))
            {
                if (psd.ShowDialog() == DialogResult.OK)
                {
                    string name = "Paste special (" + psd.NumberOfCopies + (psd.NumberOfCopies == 1 ? " copy)" : " copies)");
                    PasteSpecial action = new PasteSpecial(list, psd.NumberOfCopies, psd.StartPoint, psd.Grouping,
                                                  psd.AccumulativeOffset, psd.AccumulativeRotation,
                                                  psd.MakeEntitiesUnique, psd.PrefixEntityNames, psd.EntityNamePrefix);
                    _document.PerformAction(name, action);
                }
            }
        }

        public void OperationsDelete()
        {
            if (!_document.Selection.IsEmpty() && !_document.Selection.InFaceSelection)
            {
                List<long> sel = _document.Selection.GetSelectedParents().Select(x => x.ID).ToList();
                string name = "Removed " + sel.Count + " item" + (sel.Count == 1 ? "" : "s");
                _document.PerformAction(name, new Delete(sel));
            }
        }

        public void SelectionClear()
        {
            List<MapObject> selected = _document.Selection.GetSelectedObjects().ToList();
            _document.PerformAction("Clear selection", new Deselect(selected));
        }

        public void SelectAll()
        {
            List<MapObject> all = _document.Map.WorldSpawn.Find(x => !(x is World));
            _document.PerformAction("Select all", new Actions.MapObjects.Selection.Select(all));
        }

        public void ObjectProperties()
        {
            ObjectPropertiesDialog pd = new ObjectPropertiesDialog(_document);
            pd.Show(Editor.Instance);
        }

        public void SwitchTool(HotkeyTool tool)
        {
            if (ToolManager.ActiveTool != null && ToolManager.ActiveTool.GetHotkeyToolType() == tool) tool = HotkeyTool.Selection;
            ToolManager.Activate(tool);
        }

        public void ApplyCurrentTextureToSelection()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection || Editor.Instance == null) return;
            TextureItem texture = _document.TextureCollection.SelectedTexture;
            if (texture == null) return;
            ITexture ti = texture.GetTexture();
            if (ti == null) return;
            Action<Document, Face> action = (document, face) =>
            {
                face.Texture.Name = texture.Name;
                face.Texture.Texture = ti;
                face.CalculateTextureCoordinates(true);
            };
            IEnumerable<Face> faces = _document.Selection.GetSelectedObjects().OfType<Solid>().SelectMany(x => x.Faces);
            _document.PerformAction("Apply current texture", new EditFace(faces, action, true));
        }

        public void QuickHideSelected()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            Visgroup autohide = _document.Map.GetAllVisgroups().FirstOrDefault(x => x.Name == "Autohide");
            if (autohide == null) return;

            IEnumerable<MapObject> objects = _document.Selection.GetSelectedObjects();
            _document.PerformAction("Hide objects", new QuickHideObjects(objects));
        }

        public void QuickHideUnselected()
        {
            if (_document.Selection.InFaceSelection) return;

            Visgroup autohide = _document.Map.GetAllVisgroups().FirstOrDefault(x => x.Name == "Autohide");
            if (autohide == null) return;

            IEnumerable<MapObject> objects = _document.Map.WorldSpawn.FindAll().Except(_document.Selection.GetSelectedObjects()).Where(x => !(x is World) && !(x is Group));
            _document.PerformAction("Hide objects", new QuickHideObjects(objects));
        }

        public void QuickHideShowAll()
        {
            Visgroup autohide = _document.Map.GetAllVisgroups().FirstOrDefault(x => x.Name == "Autohide");
            if (autohide == null) return;

            List<MapObject> objects = _document.Map.WorldSpawn.Find(x => x.IsInVisgroup(autohide.ID, true));
            _document.PerformAction("Show hidden objects", new QuickShowObjects(objects));
        }

        public void WorldspawnProperties()
        {
            ObjectPropertiesDialog pd = new ObjectPropertiesDialog(_document) { FollowSelection = false, AllowClassChange = false };
            pd.SetObjects(new[] { _document.Map.WorldSpawn });
            pd.Show(Editor.Instance);
        }

        public void Carve()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            Solid carver = _document.Selection.GetSelectedObjects().OfType<Solid>().FirstOrDefault();
            if (carver == null) return;

            IEnumerable<Solid> carvees = _document.Map.WorldSpawn.Find(x => x is Solid && x.BoundingBox.IntersectsWith(carver.BoundingBox)).OfType<Solid>();

            _document.PerformAction("Carve objects", new Carve(carvees, carver));
        }

        public void MakeHollow()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            List<Solid> solids = _document.Selection.GetSelectedObjects().OfType<Solid>().ToList();
            if (!solids.Any()) return;

            if (solids.Count > 1)
            {
                if (DarkMessageBox.Show("This will hollow out every selected solid, are you sure?", "Multiple solids selected", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            QuickForm qf = new QuickForm("Select Wall Width") { UseShortcutKeys = true }.NumericUpDown("Wall Width", -1024, 1024, 0, 32).OkCancel();

            decimal width;
            do
            {
                if (qf.ShowDialog() == DialogResult.Cancel) return;
                width = qf.Decimal("Wall Width");
                if (width == 0) DarkMessageBox.Show("Please select a non-zero value.");
            } while (width == 0);

            _document.PerformAction("Make objects hollow", new MakeHollow(solids, width));
        }

        public void GroupingGroup()
        {
            if (!_document.Selection.IsEmpty() && !_document.Selection.InFaceSelection)
            {
                _document.PerformAction("Grouped objects", new GroupAction(_document.Selection.GetSelectedParents()));
            }
        }

        public void GroupingUngroup()
        {
            if (!_document.Selection.IsEmpty() && !_document.Selection.InFaceSelection)
            {
                _document.PerformAction("Ungrouped objects", new UngroupAction(_document.Selection.GetSelectedParents()));
            }
        }

        private class EntityContainer
        {
            public Entity Entity { get; set; }
            public override string ToString()
            {
                DataStructures.MapObjects.Property name = Entity.EntityData.Properties.FirstOrDefault(x => x.Key.ToLower() == "targetname");
                if (name != null) return name.Value + " (" + Entity.EntityData.Name + ")";
                return Entity.EntityData.Name;
            }
        }

        public void TieToEntity()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            List<Entity> entities = _document.Selection.GetSelectedObjects().OfType<Entity>().ToList();

            Entity existing = null;

            if (entities.Count == 1)
            {
                DialogResult result = new QuickForms.QuickForm("Existing Entity in Selection") { Width = 400 }
                    .Label(String.Format("You have selected an existing entity (a '{0}'), how would you like to proceed?", entities[0].ClassName))
                    .Label(" - Keep the existing entity and add the selected items to the entity")
                    .Label(" - Create a new entity and add the selected items to the new entity")
                    .Item(new QuickFormDialogButtons()
                              .Button("Keep Existing", DialogResult.Yes)
                              .Button("Create New", DialogResult.No)
                              .Button("Cancel", DialogResult.Cancel))
                    .ShowDialog();
                if (result == DialogResult.Yes)
                {
                    existing = entities[0];
                }
            }
            else if (entities.Count > 1)
            {
                QuickForm qf = new QuickForms.QuickForm("Multiple Entities Selected") { Width = 400 }
                    .Label("You have selected multiple entities, which one would you like to keep?")
                    .ComboBox("Entity", entities.Select(x => new EntityContainer { Entity = x }))
                    .OkCancel();
                DialogResult result = qf.ShowDialog();
                if (result == DialogResult.OK)
                {
                    EntityContainer cont = qf.Object("Entity") as EntityContainer;
                    if (cont != null) existing = cont.Entity;
                }
            }

            ActionCollection ac = new ActionCollection();

            if (existing == null)
            {
                string def = _document.Game.DefaultBrushEntity;
                GameDataObject entity = _document.Selection.GetSelectedObjects().OfType<Solid>().SelectMany(x => x.Faces).Any(x => x.Texture.Name == "tooltextures/triggerbox")
                    ? _document.GameData.Classes.FirstOrDefault(x => x.Name.ToLower() == "triggerbox")
                    : _document.GameData.Classes.FirstOrDefault(x => x.Name.ToLower() == def.ToLower())
                      ?? _document.GameData.Classes.FirstOrDefault(x => x.ClassType == ClassType.Solid);
                if (entity == null)
                {
                    DarkMessageBox.Show("No solid entities found. Please make sure your FGDs are configured correctly.", "No entities found!");
                    return;
                }
                existing = new Entity(_document.Map.IDGenerator.GetNextObjectID())
                {
                    EntityData = new EntityData(entity),
                    ClassName = entity.Name,
                    Colour = Colour.GetDefaultEntityColour()
                };
                ac.Add(new Create(_document.Map.WorldSpawn.ID, existing));
            }
            else
            {
                // Move the new parent to the root, in case it is a descendant of a selected parent...
                ac.Add(new Reparent(_document.Map.WorldSpawn.ID, new[] { existing }));

                // todo: get rid of all the other entities...
            }

            List<MapObject> reparent = _document.Selection.GetSelectedParents().Where(x => x != existing).ToList();
            ac.Add(new Reparent(existing.ID, reparent));
            ac.Add(new Actions.MapObjects.Selection.Select(existing));

            _document.PerformAction("Tie to Entity", ac);

            if (CBRE.Settings.Select.OpenObjectPropertiesWhenCreatingEntity && !ObjectPropertiesDialog.IsShowing)
            {
                Mediator.Publish(HotkeysMediator.ObjectProperties);
            }
        }

        public void TieToWorld()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            List<Entity> entities = _document.Selection.GetSelectedObjects().OfType<Entity>().ToList();
            List<MapObject> children = entities.SelectMany(x => x.GetChildren()).ToList();

            ActionCollection ac = new ActionCollection();
            ac.Add(new Reparent(_document.Map.WorldSpawn.ID, children));
            ac.Add(new Delete(entities.Select(x => x.ID)));

            _document.PerformAction("Tie to World", ac);
        }

        private IUnitTransformation GetSnapTransform(Box box)
        {
            Coordinate offset = box.Start.Snap(_document.Map.GridSpacing) - box.Start;
            return new UnitTranslate(offset);
        }

        public void Transform()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;
            Box box = _document.Selection.GetSelectionBoundingBox();
            using (TransformDialog td = new TransformDialog(box))
            {
                if (td.ShowDialog() != DialogResult.OK) return;

                Coordinate value = td.TransformValue;
                IUnitTransformation transform = null;
                switch (td.TransformType)
                {
                    case TransformType.Rotate:
                        Matrix mov = Matrix.Translation(-box.Center); // Move to zero
                        Matrix rot = Matrix.Rotation(Quaternion.EulerAngles(value * DMath.PI / 180)); // Do rotation
                        Matrix fin = Matrix.Translation(box.Center); // Move to final origin
                        transform = new UnitMatrixMult(fin * rot * mov);
                        break;
                    case TransformType.Translate:
                        transform = new UnitTranslate(value);
                        break;
                    case TransformType.Scale:
                        transform = new UnitScale(value, box.Center);
                        break;
                }

                if (transform == null) return;

                IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();
                _document.PerformAction("Transform selection", new Edit(selected, new TransformEditOperation(transform, _document.Map.GetTransformFlags())));
            }
        }

        public void RotateClockwise()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;
            Viewport2D focused = ViewportManager.Viewports.FirstOrDefault(x => x.IsFocused && x is Viewport2D) as Viewport2D;
            if (focused == null) return;
            Coordinate center = new Box(_document.Selection.GetSelectedObjects().Select(x => x.BoundingBox).Where(x => x != null)).Center;
            Coordinate axis = focused.GetUnusedCoordinate(Coordinate.One);
            UnitRotate transform = new UnitRotate(DMath.DegreesToRadians(90), new Line(center, center + axis));
            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();
            _document.PerformAction("Transform selection", new Edit(selected, new TransformEditOperation(transform, _document.Map.GetTransformFlags())));
        }

        public void RotateCounterClockwise()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;
            Viewport2D focused = ViewportManager.Viewports.FirstOrDefault(x => x.IsFocused && x is Viewport2D) as Viewport2D;
            if (focused == null) return;
            Coordinate center = new Box(_document.Selection.GetSelectedObjects().Select(x => x.BoundingBox).Where(x => x != null)).Center;
            Coordinate axis = focused.GetUnusedCoordinate(Coordinate.One);
            UnitRotate transform = new UnitRotate(DMath.DegreesToRadians(-90), new Line(center, center + axis));
            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();
            _document.PerformAction("Transform selection", new Edit(selected, new TransformEditOperation(transform, _document.Map.GetTransformFlags())));
        }

        public void ReplaceTextures()
        {
            using (TextureReplaceDialog trd = new TextureReplaceDialog(_document))
            {
                if (trd.ShowDialog() == DialogResult.OK)
                {
                    IAction action = trd.GetAction();
                    _document.PerformAction("Replace textures", action);
                }
            }
        }

        public void SnapSelectionToGrid()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();

            Box box = _document.Selection.GetSelectionBoundingBox();
            IUnitTransformation transform = GetSnapTransform(box);

            _document.PerformAction("Snap to grid", new Edit(selected, new TransformEditOperation(transform, _document.Map.GetTransformFlags())));
        }

        public void SnapSelectionToGridIndividually()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();

            _document.PerformAction("Snap to grid individually", new Edit(selected, new SnapToGridEditOperation(_document.Map.GridSpacing, _document.Map.GetTransformFlags())));
        }

        private void AlignObjects(AlignObjectsEditOperation.AlignAxis axis, AlignObjectsEditOperation.AlignDirection direction)
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();
            Box box = _document.Selection.GetSelectionBoundingBox();

            _document.PerformAction("Align Objects", new Edit(selected, new AlignObjectsEditOperation(box, axis, direction, _document.Map.GetTransformFlags())));
        }

        public void AlignXMax()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.X, AlignObjectsEditOperation.AlignDirection.Max);
        }

        public void AlignXMin()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.X, AlignObjectsEditOperation.AlignDirection.Min);
        }

        public void AlignYMax()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.Y, AlignObjectsEditOperation.AlignDirection.Max);
        }

        public void AlignYMin()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.Y, AlignObjectsEditOperation.AlignDirection.Min);
        }

        public void AlignZMax()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.Z, AlignObjectsEditOperation.AlignDirection.Max);
        }

        public void AlignZMin()
        {
            AlignObjects(AlignObjectsEditOperation.AlignAxis.Z, AlignObjectsEditOperation.AlignDirection.Min);
        }

        private void FlipObjects(Coordinate scale)
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            IEnumerable<MapObject> selected = _document.Selection.GetSelectedParents();
            Box box = _document.Selection.GetSelectionBoundingBox();

            UnitScale transform = new UnitScale(scale, box.Center);
            _document.PerformAction("Flip Objects", new Edit(selected, new TransformEditOperation(transform, _document.Map.GetTransformFlags())));
        }

        public void FlipX()
        {
            FlipObjects(new Coordinate(-1, 1, 1));
        }

        public void FlipY()
        {
            FlipObjects(new Coordinate(1, -1, 1));
        }

        public void FlipZ()
        {
            FlipObjects(new Coordinate(1, 1, -1));
        }

        public void GridIncrease()
        {
            decimal curr = _document.Map.GridSpacing;
            if (curr >= 1024) return;
            _document.Map.GridSpacing *= 2;
            RebuildGrid();
        }

        public void GridDecrease()
        {
            decimal curr = _document.Map.GridSpacing;
            if (curr <= 1) return;
            _document.Map.GridSpacing /= 2;
            RebuildGrid();
        }

        public void RebuildGrid()
        {
            _document.Renderer.UpdateGrid(_document.Map.GridSpacing, _document.Map.Show2DGrid, _document.Map.Show3DGrid, true);
            Mediator.Publish(EditorMediator.DocumentGridSpacingChanged, _document.Map.GridSpacing);
        }

        public void CenterAllViewsOnSelection()
        {
            Box box = _document.Selection.GetSelectionBoundingBox()
                      ?? new Box(Coordinate.Zero, Coordinate.Zero);
            foreach (ViewportBase vp in ViewportManager.Viewports)
            {
                vp.FocusOn(box);
            }
        }

        public void Center2DViewsOnSelection()
        {
            Box box = _document.Selection.GetSelectionBoundingBox()
                      ?? new Box(Coordinate.Zero, Coordinate.Zero);
            foreach (Viewport2D vp in ViewportManager.Viewports.OfType<Viewport2D>())
            {
                vp.FocusOn(box);
            }
        }

        public void Center3DViewsOnSelection()
        {
            Box box = _document.Selection.GetSelectionBoundingBox()
                      ?? new Box(Coordinate.Zero, Coordinate.Zero);
            foreach (Viewport3D vp in ViewportManager.Viewports.OfType<Viewport3D>())
            {
                vp.FocusOn(box);
            }
        }

        public void GoToCoordinates()
        {
            using (QuickForm qf = new QuickForm("Enter Coordinates") { LabelWidth = 50, UseShortcutKeys = true }
                .TextBox("X", "0")
                .TextBox("Y", "0")
                .TextBox("Z", "0")
                .OkCancel())
            {
                qf.ClientSize = new Size(180, qf.ClientSize.Height);
                if (qf.ShowDialog() != DialogResult.OK) return;

                decimal x, y, z;
                if (!Decimal.TryParse(qf.String("X"), out x)) return;
                if (!Decimal.TryParse(qf.String("Y"), out y)) return;
                if (!Decimal.TryParse(qf.String("Z"), out z)) return;

                Coordinate coordinate = new Coordinate(x, y, z);

                ViewportManager.Viewports.ForEach(vp => vp.FocusOn(coordinate));
            }
        }

        public void GoToBrushID()
        {
            using (QuickForm qf = new QuickForm("Enter Brush ID") { LabelWidth = 100, UseShortcutKeys = true }
                .TextBox("Brush ID")
                .OkCancel())
            {
                qf.ClientSize = new Size(230, qf.ClientSize.Height);

                if (qf.ShowDialog() != DialogResult.OK) return;

                long id;
                if (!long.TryParse(qf.String("Brush ID"), out id)) return;

                MapObject obj = _document.Map.WorldSpawn.FindByID(id);
                if (obj == null) return;

                // Select and go to the brush
                _document.PerformAction("Select brush ID " + id, new ChangeSelection(new[] { obj }, _document.Selection.GetSelectedObjects()));
                ViewportManager.Viewports.ForEach(x => x.FocusOn(obj.BoundingBox));
            }
        }

        public void ToggleSnapToGrid()
        {
            _document.Map.SnapToGrid = !_document.Map.SnapToGrid;
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleShow2DGrid()
        {
            _document.Map.Show2DGrid = !_document.Map.Show2DGrid;
            RebuildGrid();
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleShow3DGrid()
        {
            _document.Map.Show3DGrid = !_document.Map.Show3DGrid;
            _document.Renderer.UpdateGrid(_document.Map.GridSpacing, _document.Map.Show2DGrid, _document.Map.Show3DGrid, false);
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleIgnoreGrouping()
        {
            _document.Map.IgnoreGrouping = !_document.Map.IgnoreGrouping;
            Mediator.Publish(EditorMediator.IgnoreGroupingChanged);
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleTextureLock()
        {
            _document.Map.TextureLock = !_document.Map.TextureLock;
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleTextureScalingLock()
        {
            _document.Map.TextureScalingLock = !_document.Map.TextureScalingLock;
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleCordon()
        {
            _document.Map.Cordon = !_document.Map.Cordon;
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleHideFaceMask()
        {
            _document.Map.HideFaceMask = !_document.Map.HideFaceMask;
            _document.Renderer.UpdateDocumentToggles();
        }

        public void ToggleHideDisplacementSolids()
        {
            _document.Map.HideDisplacementSolids = !_document.Map.HideDisplacementSolids;
            // todo hide displacement solids
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleHideToolTextures()
        {
            _document.Map.HideToolTextures = !_document.Map.HideToolTextures;
            _document.RenderAll();
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleHideEntitySprites()
        {
            _document.Map.HideEntitySprites = !_document.Map.HideEntitySprites;
            _document.RenderAll();
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ToggleHideMapOrigin()
        {
            _document.Map.HideMapOrigin = !_document.Map.HideMapOrigin;
            _document.RenderAll();
            Mediator.Publish(EditorMediator.UpdateToolstrip);
        }

        public void ShowSelectedBrushID()
        {
            if (_document.Selection.IsEmpty() || _document.Selection.InFaceSelection) return;

            IEnumerable<long> selectedIds = _document.Selection.GetSelectedObjects().Select(x => x.ID);
            string idString = String.Join(", ", selectedIds);

            DarkMessageBox.Show("Selected Object IDs: " + idString);
        }

        public void ShowMapInformation()
        {
            using (MapInformationDialog mid = new MapInformationDialog(_document))
            {
                mid.ShowDialog();
            }
        }

        public void ShowLogicalTree()
        {
            MapTreeWindow mtw = new MapTreeWindow(_document);
            mtw.Show(Editor.Instance);
        }

        public void ShowEntityReport()
        {
            EntityReportDialog erd = new EntityReportDialog();
            erd.Show(Editor.Instance);
        }

        public void ViewportRightClick(Viewport2D vp, ViewportEvent e)
        {
            ViewportContextMenu.Instance.AddNonSelectionItems(_document, vp);
            if (!_document.Selection.IsEmpty() && !_document.Selection.InFaceSelection && ToolManager.ActiveTool is SelectTool)
            {
                Box selectionBoundingBox = _document.Selection.GetSelectionBoundingBox();
                Coordinate point = vp.ScreenToWorld(e.X, vp.Height - e.Y);
                Coordinate start = vp.Flatten(selectionBoundingBox.Start);
                Coordinate end = vp.Flatten(selectionBoundingBox.End);
                if (point.X >= start.X && point.X <= end.X && point.Y >= start.Y && point.Y <= end.Y)
                {
                    // Clicked inside the selection bounds
                    ViewportContextMenu.Instance.AddSelectionItems(_document, vp);
                }
            }
            if (ToolManager.ActiveTool != null) ToolManager.ActiveTool.OverrideViewportContextMenu(ViewportContextMenu.Instance, vp, e);
            if (ViewportContextMenu.Instance.Items.Count > 0) ViewportContextMenu.Instance.Show(vp, e.X, e.Y);
        }

        public void VisgroupSelect(int visgroupId)
        {
            if (_document.Selection.InFaceSelection) return;
            IEnumerable<MapObject> objects = _document.Map.WorldSpawn.Find(x => x.IsInVisgroup(visgroupId, true), true).Where(x => !x.IsVisgroupHidden);
            _document.PerformAction("Select visgroup", new ChangeSelection(objects, _document.Selection.GetSelectedObjects()));
        }

        public void VisgroupShowEditor()
        {
            using (VisgroupEditForm vef = new VisgroupEditForm(_document))
            {
                if (vef.ShowDialog() == DialogResult.OK)
                {
                    List<Visgroup> nv = new List<Visgroup>();
                    List<Visgroup> cv = new List<Visgroup>();
                    List<Visgroup> dv = new List<Visgroup>();
                    vef.PopulateChangeLists(_document, nv, cv, dv);
                    if (nv.Any() || cv.Any() || dv.Any())
                    {
                        _document.PerformAction("Edit visgroups", new CreateEditDeleteVisgroups(nv, cv, dv));
                    }
                }
            }
        }

        public void VisgroupShowAll()
        {
            _document.PerformAction("Show all visgroups", new ShowAllVisgroups());
        }

        public void VisgroupToggled(int visgroupId, CheckState state)
        {
            if (state == CheckState.Indeterminate) return;
            bool visible = state == CheckState.Checked;
            _document.PerformAction((visible ? "Show" : "Hide") + " visgroup", new ToggleVisgroup(visgroupId, visible));
        }

        public void VisgroupCreateNew()
        {
            using (QuickForm qf = new QuickForm("Create New Visgroup") { UseShortcutKeys = true }.TextBox("Name").CheckBox("Add selection to visgroup", true).OkCancel())
            {
                if (qf.ShowDialog() != DialogResult.OK) return;

                List<int> ids = _document.Map.Visgroups.Where(x => !x.IsAutomatic).Select(x => x.ID).ToList();
                int id = Math.Max(1, ids.Any() ? ids.Max() + 1 : 1);

                string name = qf.String("Name");
                if (String.IsNullOrWhiteSpace(name)) name = "Visgroup " + id.ToString();

                Visgroup vg = new Visgroup
                {
                    ID = id,
                    Colour = Colour.GetRandomLightColour(),
                    Name = name,
                    Visible = true
                };
                IAction action = new CreateEditDeleteVisgroups(new[] { vg }, new Visgroup[0], new Visgroup[0]);
                if (qf.Bool("Add selection to visgroup") && !_document.Selection.IsEmpty())
                {
                    action = new ActionCollection(action, new EditObjectVisgroups(_document.Selection.GetSelectedObjects(), new[] { id }, new int[0]));
                }
                _document.PerformAction("Create visgroup", action);
            }
        }

        public void SetZoomValue(decimal value)
        {
            foreach (Viewport2D vp in ViewportManager.Viewports.OfType<Viewport2D>())
            {
                vp.Zoom = value;
            }
            Mediator.Publish(EditorMediator.ViewZoomChanged, value);
        }

        public void TextureSelected(TextureItem selection)
        {
            _document.TextureCollection.SelectedTexture = selection;
        }

        public void ViewportCreated(ViewportBase viewport)
        {
            if (viewport is Viewport3D) viewport.RenderContext.Add(new WidgetLinesRenderable());
            _document.Renderer.Register(new[] { viewport });
            viewport.RenderContext.Add(new ToolRenderable());
            viewport.RenderContext.Add(new HelperRenderable(_document));
            _document.Renderer.UpdateGrid(_document.Map.GridSpacing, _document.Map.Show2DGrid, _document.Map.Show3DGrid, false);
        }

        public void SelectMatchingTextures(IEnumerable<string> textureList)
        {
            List<string> list = textureList.ToList();
            List<Face> allFaces = _document.Map.WorldSpawn.Find(x => x is Solid && !x.IsCodeHidden && !x.IsVisgroupHidden).OfType<Solid>().SelectMany(x => x.Faces).ToList();
            List<Face> matchingFaces = allFaces.Where(x => list.Contains(x.Texture.Name, StringComparer.CurrentCultureIgnoreCase)).ToList();
            int fc = matchingFaces.Count;
            if (_document.Selection.InFaceSelection)
            {
                _document.PerformAction("Select Faces by Texture", new ChangeFaceSelection(matchingFaces, _document.Selection.GetSelectedFaces()));
                DarkMessageBox.Show(fc + " face" + (fc == 1 ? "" : "s") + " selected.");
            }
            else
            {
                List<Solid> objects = matchingFaces.Select(x => x.Parent).Distinct().ToList();
                _document.PerformAction("Select Objects by Texture", new ChangeSelection(objects, _document.Selection.GetSelectedObjects()));
                int oc = objects.Count;
                DarkMessageBox.Show(fc + " face" + (fc == 1 ? "" : "s") + " found, " + oc + " object" + (oc == 1 ? "" : "s") + " selected.");
            }
        }
    }
}
