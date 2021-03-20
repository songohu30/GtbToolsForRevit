using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OpeningSymbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Functions
{
    public class OpeningSymbolSelector
    {
        List<RectangularOpening> _rectangularOpenings;
        List<RoundOpening> _roundOpenings;
        List<FamilyInstance> _rectFamilyInstances;
        List<FamilyInstance> _roundFamilyInstances;
        UIDocument _uidoc;
        int roundCounter = 0;
        int rectCounter = 0;

        private OpeningSymbolSelector()
        {

        }

        public static OpeningSymbolSelector Initialize(UIDocument uidoc)
        {
            OpeningSymbolSelector result = new OpeningSymbolSelector();
            result._uidoc = uidoc;
            result.SearchFamilyInstances();
            result.CreateOpeningLists();
            return result;
        }

        private void SearchFamilyInstances()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_uidoc.Document, _uidoc.Document.ActiveView.Id);
            List<FamilyInstance> genModelInstances = ficol.OfClass(typeof(FamilyInstance))
                                    .Select(x => x as FamilyInstance)
                                        .Where(y => y.Symbol.Family.FamilyCategory.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel).ToList();
            _roundFamilyInstances = new List<FamilyInstance>();
            _rectFamilyInstances = new List<FamilyInstance>();

            foreach (FamilyInstance fi in genModelInstances)
            {
                if (fi.Symbol.Family.Name == "XXX_Rectangular Face Opening_MT") _rectFamilyInstances.Add(fi);
                if (fi.Symbol.Family.Name == "XXX_Round Face Opening_MT") _roundFamilyInstances.Add(fi);
            }
            int roundIns = _roundFamilyInstances.Count;
            int rectIns = _rectFamilyInstances.Count;
        }
        
        private void CreateOpeningLists()
        {
            _rectangularOpenings = new List<RectangularOpening>();
            _roundOpenings = new List<RoundOpening>();

            foreach (FamilyInstance fi in _rectFamilyInstances)
            {
                RectangularOpening rectangularOpening = RectangularOpening.Initialize(fi);
                _rectangularOpenings.Add(rectangularOpening);
            }

            foreach (FamilyInstance fi in _roundFamilyInstances)
            {
                RoundOpening roundOpening = RoundOpening.Initialize(fi);
                _roundOpenings.Add(roundOpening);
            }
        }

        public void SelectWallOpenings()
        {
            List<ElementId> wallOpenings = new List<ElementId>();
            foreach (RectangularOpening ro in _rectangularOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Wall)
                {
                    wallOpenings.Add(ro.FamilyInstance.Id);
                    rectCounter++;
                }
            }
            foreach (RoundOpening ro in _roundOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Wall)
                {
                    wallOpenings.Add(ro.FamilyInstance.Id);
                    roundCounter++;
                }
            }
            _uidoc.Selection.SetElementIds(wallOpenings);
            if (IsOpeningNumberNull()) return;
            _uidoc.ShowElements(wallOpenings);
        }

        public void SelectFloorOpenings()
        {
            List<ElementId> floorOpenings = new List<ElementId>();
            foreach (RectangularOpening ro in _rectangularOpenings)
            {
                if (ro.OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    floorOpenings.Add(ro.FamilyInstance.Id);
                    rectCounter++;
                }
            }
            foreach (RoundOpening ro in _roundOpenings)
            {
                if (ro.OpeningHost == OpeningHost.FloorOrCeiling)
                {
                    floorOpenings.Add(ro.FamilyInstance.Id);
                    roundCounter++;
                }
            }
            _uidoc.Selection.SetElementIds(floorOpenings);
            if (IsOpeningNumberNull()) return;
            _uidoc.ShowElements(floorOpenings);
        }

        public void SelectRoofOpenings()
        {
            List<ElementId> roofOpenings = new List<ElementId>();
            foreach (RectangularOpening ro in _rectangularOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Roof)
                {
                    roofOpenings.Add(ro.FamilyInstance.Id);
                    rectCounter++;
                }
            }
            foreach (RoundOpening ro in _roundOpenings)
            {
                if (ro.OpeningHost == OpeningHost.Roof)
                {
                    roofOpenings.Add(ro.FamilyInstance.Id);
                    roundCounter++;
                }
            }
            _uidoc.Selection.SetElementIds(roofOpenings);
            if (IsOpeningNumberNull()) return;
            _uidoc.ShowElements(roofOpenings);
        }

        public bool IsOpeningNumberNull()
        {
            bool result = true;
            if (rectCounter + roundCounter > 0) result = false;
            return result;
        }

        public void ShowReport()
        {
            string header = String.Format("Selected {0} rectangular openings", rectCounter) + String.Format(", and {0} round openings.", roundCounter) + Environment.NewLine;
            string info = "";
            foreach (RectangularOpening ro in _rectangularOpenings)
            {
                if (ro.OpeningHost == OpeningHost.NotAssociated)
                {
                    info += "Rectangular opening id: " + ro.FamilyInstance.Id.IntegerValue.ToString() + " is not associated to any host" + Environment.NewLine;
                }
                if (ro.OpeningHost == OpeningHost.Unknown)
                {
                    info += "Rectangular opening id: " + ro.FamilyInstance.Id.IntegerValue.ToString() + " host is not supported" + Environment.NewLine;
                }
            }
            foreach (RoundOpening ro in _roundOpenings)
            {
                if (ro.OpeningHost == OpeningHost.NotAssociated)
                {
                    info += "Round opening id: " + ro.FamilyInstance.Id.IntegerValue.ToString() + " is not associated to any host" + Environment.NewLine;
                }
                if (ro.OpeningHost == OpeningHost.Unknown)
                {
                    info += "Round opening id: " + ro.FamilyInstance.Id.IntegerValue.ToString() + " host is not supported" + Environment.NewLine;
                }
            }
            MessageBox.Show(header + Environment.NewLine +  info);
        }
    }
}
