using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using GtbTools;
using GUI;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ViewModels
{
    public class DurchbruchMemoryViewModel : INotifyPropertyChanged
    {
        public UIApplication UIApplication { get; set; }
        public UIDocument UIDocument {get; set;}
        public Document Document { get; set; }
        public DurchbruchModel CurrentModel { get; set; }
        public ElementId CurrentSelection { get; set; }
        public MovedAndResizedDbViewModel CurrentItem { get; set; }
        public View DesiredView { get; set; }
        public ExternalEvent ChangeDiameterEvent { get; set; }
        public ExternalEvent ChangeOffsetEvent { get; set; }
        public ExternalEvent LoadContextEvent { get; set; }
        public ExternalEvent ShowElementEvent { get; set; }
        public ExternalEvent OpenViewEvent { get; set; }
        public ExternalEvent SaveDataToExStorageEvent { get; set; }
        public List<NewDurchbruchViewModel> _newDurchbruche;
        public List<NewDurchbruchViewModel> NewDurchbruche
        {
            get => _newDurchbruche;
            set
            {
                if (_newDurchbruche != value)
                {
                    _newDurchbruche = value;
                    OnPropertyChanged(nameof(NewDurchbruche));
                }
            }
        }
        public List<MovedAndResizedDbViewModel> _movedDurchbruche;
        public List<MovedAndResizedDbViewModel> MovedDurchbruche
        {
            get => _movedDurchbruche;
            set
            {
                if (_movedDurchbruche != value)
                {
                    _movedDurchbruche = value;
                    OnPropertyChanged(nameof(MovedDurchbruche));
                }
            }
        }
        public List<ResizedDurchbruchViewModel> _resizedDurchbruche;
        public List<ResizedDurchbruchViewModel> ResizedDurchbruche
        {
            get => _resizedDurchbruche;
            set
            {
                if (_resizedDurchbruche != value)
                {
                    _resizedDurchbruche = value;
                    OnPropertyChanged(nameof(ResizedDurchbruche));
                }
            }
        }
        public List<MovedAndResizedDbViewModel> _movedAndResizedDurchbruche;
        public List<MovedAndResizedDbViewModel> MovedAndResizedDurchbruche
        {
            get => _movedAndResizedDurchbruche;
            set
            {
                if (_movedAndResizedDurchbruche != value)
                {
                    _movedAndResizedDurchbruche = value;
                    OnPropertyChanged(nameof(MovedAndResizedDurchbruche));
                }
            }
        }
        public MemorySaveOption MemorySaveOption { get; set; }
        public DurchbruchMemoryAction DurchbruchMemoryAction { get; set; }
        public List<DurchbruchModel> SelectedToSave { get; set; }
        public List<ElementId> SelectedIds { get; set; }

        public ManualResetEvent SignalEvent = new ManualResetEvent(false);

        List<FamilyInstance> _familyInstancesAll;
        List<FamilyInstance> _familyInstancesView;
        List<DurchbruchModel> _modelDurchbrucheAll;
        public OptimisationChoice OptimisationChoice { get; set; }
        public List<int> OldPositionMarkers { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DurchbruchMemoryViewModel()
        {
            OldPositionMarkers = new List<int>();
        }

        public void InitializeDurchbruche()
        {
            SetAllDurchbruche();
            SetViewDurchbruche();
            List<ModelView> selectedViews =  EstimateProject();
            if(OptimisationChoice == OptimisationChoice.Fast)
            {
                SetAllModelDurchbruche(_familyInstancesView);
                SetActiveViewToAllModelDurchbruche();
            }
            if(OptimisationChoice == OptimisationChoice.Medium)
            {
                SetAllModelDurchbruche(_familyInstancesAll);
            }
            if (OptimisationChoice == OptimisationChoice.Slow)
            {
                SetAllModelDurchbruche(_familyInstancesAll);
                SetAllModelDurchbrucheViews(selectedViews);
            }
            if (OptimisationChoice == OptimisationChoice.None)
            {
                return;
            }
            SetChangedDurchbruche();
        }

        public List<ModelView> EstimateProject()
        {
            int fullNumber = _familyInstancesAll.Count;
            int viewNumber = _familyInstancesView.Count;
            List<ModelView> viewSelection = SetViewModels();
            string label1 = String.Format("There are {0} openings found in this project.", fullNumber);
            string label2 = String.Format("There are {0} openings found in this view.", viewNumber);
            MemoryFunctionOptimisation window = new MemoryFunctionOptimisation(label1, label2, viewSelection);
            window.ShowDialog();
            OptimisationChoice = window.OptimisationChoice;
            return window.SelectedViews;
        }

        private List<ModelView> SetViewModels()
        {
            List<ModelView> result = new List<ModelView>();
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            ficol.OfClass(typeof(View));
            foreach (View view in ficol)
            {
                if (view.IsTemplate) continue;
                if (view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan || view.ViewType == ViewType.EngineeringPlan || view.ViewType == ViewType.Section)
                {
                    ModelView mv = new ModelView() { Name = view.Name, View = view, IsSelected = false };
                    result.Add(mv);
                }
            }
            return result;
        }

        public void UpdateDurchbrucheDeep()
        {
            foreach (var item in NewDurchbruche)
            {
                item.UpdateDurchbruchDeep();
            }
            NewDurchbruche = NewDurchbruche.Where(e => e.DurchbruchModel.DurchbruchStatus == DurchbruchStatus.New).ToList();

            foreach (var item in MovedDurchbruche)
            {
                item.UpdateDurchbruchDeep();
            }
            MovedDurchbruche = MovedDurchbruche.Where(e => e.DurchbruchModel.DurchbruchStatus == DurchbruchStatus.Moved).ToList();

            foreach (var item in ResizedDurchbruche)
            {
                item.UpdateDurchbruchDeep();
            }
            ResizedDurchbruche = ResizedDurchbruche.Where(e => e.DurchbruchModel.DurchbruchStatus == DurchbruchStatus.Resized).ToList();

            foreach (var item in MovedAndResizedDurchbruche)
            {
                item.UpdateDurchbruchDeep();
            }
            MovedAndResizedDurchbruche = MovedAndResizedDurchbruche.Where(e => e.DurchbruchModel.DurchbruchStatus == DurchbruchStatus.MovedAndResized).ToList();
        }

        private void SetAllModelDurchbrucheViews(List<ModelView> views)
        {
            foreach (ModelView modelView in views)
            {
                FilteredElementCollector ficol = new FilteredElementCollector(Document, modelView.View.Id);
                List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                        .Select(x => x as FamilyInstance)
                            .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
                foreach (DurchbruchModel dbm in _modelDurchbrucheAll)
                {
                    FamilyInstance fi = dbm.FamilyInstance;
                    List<FamilyInstance> instances = genModelInstances.Where(y => y.Id.IntegerValue == fi.Id.IntegerValue).ToList();
                    if (instances.Count > 0) dbm.Views.Add(modelView.View);
                }
            }
        }

        private void SetActiveViewToAllModelDurchbruche()
        {
            foreach (DurchbruchModel dbm in _modelDurchbrucheAll)
            {
                dbm.Views.Add(Document.ActiveView);
            }
        }

        public void LoadContext(UIApplication uIApplication)
        {
            UIApplication = uIApplication;
            UIDocument = UIApplication.ActiveUIDocument;
            Document = UIApplication.ActiveUIDocument.Document;
        }

        public void SetExternalEvents(ExternalEvent loadContextEvent, ExternalEvent showElementEvent
                                        , ExternalEvent openViewEvent, ExternalEvent saveDataToExStorageEvent
                                            , ExternalEvent changeDiameterEvent, ExternalEvent changeOffsetEvent)
        {
            LoadContextEvent = loadContextEvent;
            ShowElementEvent = showElementEvent;
            OpenViewEvent = openViewEvent;
            SaveDataToExStorageEvent = saveDataToExStorageEvent;
            ChangeDiameterEvent = changeDiameterEvent;
            ChangeOffsetEvent = changeOffsetEvent;
        }

        public void ShowElement()
        {
            UIDocument.Selection.SetElementIds(new List<ElementId>());
            UIDocument.Selection.SetElementIds(new List<ElementId>() { CurrentSelection });
        }

        public void ShowElements()
        {
            UIDocument.Selection.SetElementIds(new List<ElementId>());
            UIDocument.Selection.SetElementIds(SelectedIds);
        }

        public void OpenView()
        {
            UIDocument.ActiveView = DesiredView;
            UIDocument.Selection.SetElementIds(new List<ElementId>() { CurrentSelection });
            UIDocument.ShowElements(CurrentSelection);
        }

        public void SaveOpeningsToExStorage()
        {
            UpdateDurchbrucheDeep();
            using (Transaction tx = new Transaction(Document, "GTB-Tool ExStorage Write"))
            {
                tx.Start();
                if(MemorySaveOption == MemorySaveOption.New)
                {
                    SaveNewOpenings();
                }
                if(MemorySaveOption == MemorySaveOption.All)
                {
                    SaveNewOpenings();
                    SaveMovedOpenings();
                    SaveResizedOpenings();
                    SaveMovedAndResizedOpenings();
                }
                if(MemorySaveOption == MemorySaveOption.Selected)
                {
                    SaveSelectedOpenings();
                }
                tx.Commit();
            }
            UpdateDurchbrucheDeep();
        }

        private FamilySymbol FindPositionMarker()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            List<FamilySymbol> genModelInstances = ficol.OfClass(typeof(FamilySymbol))
                        .Select(x => x as FamilySymbol)
                            .Where(y => y.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            FamilySymbol fs = genModelInstances.Where(e => e.Name == "LocationMarker").FirstOrDefault();
            return fs;
        }

        public void CreateOldPositionMarker()
        {
            FamilySymbol fs = FindPositionMarker();

            if (fs == null)
            {
                TaskDialog.Show("Info", "Can't find position marker family type!");
                return;
            }

            string oldPosition = CurrentItem.DurchbruchModel.OpeningMemory.OldPosition;
            string[] coords = oldPosition.Split(';');
            double x1 = Convert.ToDouble(coords[0], System.Globalization.CultureInfo.InvariantCulture);
            double y1 = Convert.ToDouble(coords[1], System.Globalization.CultureInfo.InvariantCulture);
            double z1 = Convert.ToDouble(coords[2], System.Globalization.CultureInfo.InvariantCulture);

            XYZ xyz1 = new XYZ(x1, y1, z1);

            string newPosition = CurrentItem.DurchbruchModel.OpeningMemory.NewPosition;
            string[] coordsNew = newPosition.Split(';');
            double x2 = Convert.ToDouble(coordsNew[0], System.Globalization.CultureInfo.InvariantCulture);
            double y2 = Convert.ToDouble(coordsNew[1], System.Globalization.CultureInfo.InvariantCulture);
            double z2 = Convert.ToDouble(coordsNew[2], System.Globalization.CultureInfo.InvariantCulture);

            XYZ xyz2 = new XYZ(x2, y2, z2);

            Transaction tx = new Transaction(Document, "Inserted position marker");
            tx.Start();
            FamilyInstance instance = Document.Create.NewFamilyInstance(xyz1, fs, StructuralType.NonStructural);

            //line creation postponed because of too many issues
            //try
            //{
            //    Line line = Line.CreateBound(xyz1, xyz2);
            //    Plane plane = Plane.CreateByThreePoints(xyz1, xyz2, new XYZ(0, 0, 0));
            //    SketchPlane sketchPlane = SketchPlane.Create(Document, plane);
            //    ModelCurve curve = Document.Create.NewModelCurve(line, sketchPlane);
            //    CurrentItem.OldPositionModelCurve = curve.Id.IntegerValue;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Can't create line marker" + Environment.NewLine + ex.ToString());
            //}
            tx.Commit();
            CurrentItem.OldPositionMarker = instance.Id.IntegerValue;
            OldPositionMarkers.Add(instance.Id.IntegerValue);
        }

        public void DeleteOldPositionMarker()
        {
            if (CurrentItem == null || CurrentItem.OldPositionMarker == 0) return;
            if (Document.GetElement(new ElementId(CurrentItem.OldPositionMarker)) == null) return;
            OldPositionMarkers.Remove(CurrentItem.OldPositionMarker);
            Transaction tx = new Transaction(Document, "Deleted position marker");
            tx.Start();
            Document.Delete(new ElementId(CurrentItem.OldPositionMarker));
            tx.Commit();
        }

        public void DeleteAllPositionMarkers()
        {
            Transaction tx = new Transaction(Document, "Deleted position markers");
            tx.Start();
            foreach (int idInteger in OldPositionMarkers)
            {
                ElementId id = new ElementId(idInteger);
                if (Document.GetElement(id) == null) continue;
                Document.Delete(id);
            }
            tx.Commit();
            OldPositionMarkers.Clear();
        }

        private void SaveSelectedOpenings()
        {
            foreach (DurchbruchModel item in SelectedToSave)
            {
                item.OpeningMemory.UpdateCurrentSettings();
                item.OpeningMemory.SaveDataToStorage();
                //item.OpeningMemory.SaveDateTostorage();
                //item.OpeningMemory.SaveDimensionsTostorage();
                //item.OpeningMemory.SavePositionTostorage();
            }
        }

        private void SaveNewOpenings()
        {
            foreach (NewDurchbruchViewModel item in NewDurchbruche)
            {
                item.DurchbruchModel.OpeningMemory.SaveDataToStorage();
                //item.DurchbruchModel.OpeningMemory.SaveDateTostorage();
                //item.DurchbruchModel.OpeningMemory.SaveDimensionsTostorage();
                //item.DurchbruchModel.OpeningMemory.SavePositionTostorage();
            }
        }

        private void SaveResizedOpenings()
        {
            foreach (ResizedDurchbruchViewModel item in ResizedDurchbruche)
            {
                item.DurchbruchModel.OpeningMemory.SaveDataToStorage();
                //item.DurchbruchModel.OpeningMemory.SaveDateTostorage();
                //item.DurchbruchModel.OpeningMemory.SaveDimensionsTostorage();
                //item.DurchbruchModel.OpeningMemory.SavePositionTostorage();
            }
        }

        private void SaveMovedOpenings()
        {
            foreach (MovedAndResizedDbViewModel item in MovedDurchbruche)
            {
                item.DurchbruchModel.OpeningMemory.SaveDataToStorage();
                //item.DurchbruchModel.OpeningMemory.SaveDateTostorage();
                //item.DurchbruchModel.OpeningMemory.SaveDimensionsTostorage();
                //item.DurchbruchModel.OpeningMemory.SavePositionTostorage();
            }
        }
        private void SaveMovedAndResizedOpenings()
        {
            foreach (MovedAndResizedDbViewModel item in MovedAndResizedDurchbruche)
            {
                item.DurchbruchModel.OpeningMemory.SaveDataToStorage();
                //item.DurchbruchModel.OpeningMemory.SaveDateTostorage();
                //item.DurchbruchModel.OpeningMemory.SaveDimensionsTostorage();
                //item.DurchbruchModel.OpeningMemory.SavePositionTostorage();
            }
        }

        private void SetAllDurchbruche()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            _familyInstancesAll = new List<FamilyInstance>();
            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker") _familyInstancesAll.Add(fi);
            }
        }

        private void SetViewDurchbruche()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(Document, Document.ActiveView.Id);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            _familyInstancesView = new List<FamilyInstance>();
            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker") _familyInstancesView.Add(fi);
            }
        }

        private void SetAllModelDurchbruche(List<FamilyInstance> familyInstances)
        {
            _modelDurchbrucheAll = new List<DurchbruchModel>();

            foreach (FamilyInstance fi in familyInstances)
            {
                DurchbruchModel durchbruchModel = DurchbruchModel.Initialize(fi, UIDocument);
                _modelDurchbrucheAll.Add(durchbruchModel);
            }
        }

        private void SetChangedDurchbruche()
        {
            NewDurchbruche = new List<NewDurchbruchViewModel>();
            ResizedDurchbruche = new List<ResizedDurchbruchViewModel>();
            MovedDurchbruche = new List<MovedAndResizedDbViewModel>();
            MovedAndResizedDurchbruche = new List<MovedAndResizedDbViewModel>();

            foreach (DurchbruchModel dbm in _modelDurchbrucheAll)
            {
                if(dbm.DurchbruchStatus == DurchbruchStatus.New)
                {
                    NewDurchbruchViewModel newDurchbruchViewModel = NewDurchbruchViewModel.Initialize(dbm);
                    NewDurchbruche.Add(newDurchbruchViewModel);
                }
                if (dbm.DurchbruchStatus == DurchbruchStatus.Resized)
                {
                    ResizedDurchbruchViewModel resizedDurchbruchViewModel = ResizedDurchbruchViewModel.Initialize(dbm);
                    ResizedDurchbruche.Add(resizedDurchbruchViewModel);
                }
                if (dbm.DurchbruchStatus == DurchbruchStatus.Moved)
                {
                    MovedAndResizedDbViewModel movedDurchbruchViewModel = MovedAndResizedDbViewModel.Initialize(dbm);
                    MovedDurchbruche.Add(movedDurchbruchViewModel);
                }
                if (dbm.DurchbruchStatus == DurchbruchStatus.MovedAndResized)
                {
                    MovedAndResizedDbViewModel movedAndResizedDbViewModel = MovedAndResizedDbViewModel.Initialize(dbm);
                    MovedAndResizedDurchbruche.Add(movedAndResizedDbViewModel);
                }
            }
        }

        public void UpdateDurchbrucheLight()
        {
            foreach (var item in NewDurchbruche)
            {
                item.UpdateDurchbruch();
            }
            foreach (var item in MovedDurchbruche)
            {
                item.UpdateDurchbruch();
            }
            foreach (var item in ResizedDurchbruche)
            {
                item.UpdateDurchbruch();
            }
            foreach (var item in MovedAndResizedDurchbruche)
            {
                item.UpdateDurchbruch();
            }
        }
    }
}
