using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DurchbruchRotationFix;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Functions
{
    public class DurchbruchRotationFix
    {
        public ExternalEvent FixRotationEvent { get; set; }
        public ExternalEvent ProcessButton { get; set; }
        public RotationFixViewModel RotationFixViewModel { get; set; }
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);
        public DurchbruchRotationFixWindow DurchbruchRotationFixWindow { get; set; }
        List<DurchbruchRotatedModel> _allDurchbruche;
        List<FamilyInstance> _allOpenings;
        Document _document;
        UIDocument _uiDocument;
        //List<DurchbruchRotatedModel> _rotatedDurchbruche;
        //List<DurchbruchRotatedModel> _symbolNotVisible;

        public DurchbruchRotationFix()
        {

        }

        public static DurchbruchRotationFix Initialize(UIDocument uIDocument)
        {
            DurchbruchRotationFix result = new DurchbruchRotationFix();
            result._uiDocument = uIDocument;
            result._document = uIDocument.Document; ;
            result.SetAllOpeningsList();
            result.SetModelDurchbruchList();
            result.SetViewModel();
            //result.SetRotatedModelDurchbruche();
            //result.setUnvisible();
            return result;
        }

        public void FixRotation(List<DurchbruchRotatedModel> durchBrucheToFix)
        {
            using(Transaction tx = new Transaction(_document, "Durchbruche rotation fix"))
            {
                tx.Start();
                foreach (DurchbruchRotatedModel model in durchBrucheToFix)
                {
                    model.FixDurchbruchRotation(_document);
                }
                tx.Commit();
            }
        }

        public void SelectOpenings(List<ElementId> selection)
        {
            _uiDocument.Selection.SetElementIds(selection);
        }

        public void DisplayWindow()
        {
            DurchbruchRotationFixWindow = new DurchbruchRotationFixWindow(RotationFixViewModel);
            DurchbruchRotationFixWindow.ShowDialog();
        }

        private void SetViewModel()
        {
            RotationFixViewModel = RotationFixViewModel.Initialize(_allDurchbruche);           
        }

        //private void setUnvisible()
        //{
        //    _symbolNotVisible = new List<DurchbruchRotatedModel>();
        //    foreach (var item in _rotatedDurchbruche)
        //    {
        //        if (item.IsSymbolVisible == false) _symbolNotVisible.Add(item);
        //    }
        //    MessageBox.Show("Wall durchbruche symbol not visible" + _symbolNotVisible.Count.ToString());
        //}

        //private void SetRotatedModelDurchbruche()
        //{
        //    _rotatedDurchbruche = new List<DurchbruchRotatedModel>();
        //    foreach (DurchbruchRotatedModel model in _allDurchbruche)
        //    {
        //        if(model.IsRotated)
        //        {
        //            _rotatedDurchbruche.Add(model);
        //        }
        //    }
        //    MessageBox.Show("Number of rotated wall hosted durchbruche: " + _rotatedDurchbruche.Count.ToString());
        //}

        private void SetModelDurchbruchList()
        {
            _allDurchbruche = new List<DurchbruchRotatedModel>();
            foreach (FamilyInstance fi in _allOpenings)
            {
                DurchbruchRotatedModel model = DurchbruchRotatedModel.Initialize(fi);
                if(model.IsWallHosted) _allDurchbruche.Add(model);
            }
            //MessageBox.Show("Total number of wall hosted durchbruche: " + _allDurchbruche.Count.ToString());
        }

        private void SetAllOpeningsList()
        {
            _allOpenings = new List<FamilyInstance>();
            FilteredElementCollector ficol = new FilteredElementCollector(_document);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();

            foreach (FamilyInstance fi in genModelInstances)
            {
                Parameter gtbParameter = fi.get_Parameter(new Guid("4a581041-cc9c-4be4-8ab3-156d7b8e17a6"));
                if (gtbParameter != null && gtbParameter.AsString() != "GTB_Tools_location_marker") _allOpenings.Add(fi);
            }
        }

        public void SetExternalEvents(ExternalEvent fixRotationEvent)
        {
            FixRotationEvent = fixRotationEvent;
        }
    }
}
