using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using VentileSizeFix;

namespace Functions
{
    public class VentileFix
    {
        public VentileFixAction Action { get; set; }
        public List<ChangedValve> ChangedValves { get; set; }
        public List<ChangedValve> SelectedValves { get; set; }
        public List<VentileFixViewModel> VentileFixViewModels { get; set; }
        public ExternalEvent TheEvent { get; set; }
        public ElementId SelectedItem { get; set; }
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);

        Document _doc;
        List<FamilyInstance> _familyInstances;

        public VentileFix()
        {

        }

        public void Initialize(Document doc)
        {
            _doc = doc;
            FindThoseValves();
            SetChangedValvesList();
            SetViewModel();
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                VentileFixWindow window = new VentileFixWindow(this);
                window.ShowDialog();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void SetStartEvent(ExternalEvent externalEvent)
        {
            TheEvent = externalEvent;
        }

        public void FixAll()
        {
            using(Transaction tx = new Transaction(_doc, "VentileFixAlle"))
            {
                tx.Start();
                foreach (ChangedValve chv in ChangedValves)
                {
                    chv.ChangeType();
                }
                tx.Commit();
            }
        }

        public void FixSelected()
        {
            using (Transaction tx = new Transaction(_doc, "VentileFixAusgewahlte"))
            {
                tx.Start();
                foreach (ChangedValve chv in SelectedValves)
                {
                    chv.ChangeType();
                }
                tx.Commit();
            }
        }

        private void SetViewModel()
        {
            VentileFixViewModels = new List<VentileFixViewModel>();
            foreach (ChangedValve changedValve in ChangedValves)
            {
                VentileFixViewModel ventileFixViewModel = new VentileFixViewModel(changedValve);
                ventileFixViewModel.SetProperties();
                VentileFixViewModels.Add(ventileFixViewModel);
            }
        }

        private void SetChangedValvesList()
        {
            ChangedValves = new List<ChangedValve>();
            foreach (FamilyInstance fi in _familyInstances)
            {
                ChangedValve changedValve = ChangedValve.Initialize(fi, _doc);
                ChangedValves.Add(changedValve);
            }
        }

        private void FindThoseValves()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            _familyInstances = ficol.OfClass(typeof(FamilyInstance)).Select(e => e as FamilyInstance)
                                        .Where(e => e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory 
                                            && IsOfRequiredSystemType(e) 
                                                && IsOfPartTypeValve(e) 
                                                    && IsConnectedToTransitions(e)).ToList();
        }

        private bool IsOfRequiredSystemType(FamilyInstance familyInstance)
        {
            bool result = false;
            Parameter systemType = familyInstance.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            if (systemType != null)
            {
                string systemTypeText = systemType.AsValueString();
                if (systemTypeText.Contains("SAN_Warmwasser") || systemTypeText.Contains("SAN_Kaltwasser") || systemTypeText.Contains("SAN_Zirkulation"))
                {
                    result = true;
                }
            }
            return result;
        }

        private bool IsOfPartTypeValve(FamilyInstance familyInstance)
        {
            bool result = false;
            Family f = familyInstance.Symbol.Family;
            Parameter partType = f.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
            if (partType != null)
            {
                string partTypeString = partType.AsValueString();
                if (partTypeString.Contains("Valve") || partTypeString.Contains("Ventil"))
                {
                    result = true;
                }
            }
            return result;
        }

        private bool IsConnectedToTransitions(FamilyInstance familyInstance)
        {
            bool result = false;
            MEPModel mepModel = familyInstance.MEPModel;
            ConnectorSet cs = mepModel.ConnectorManager.Connectors;
            int check = 0;
            foreach (Connector ca in cs)
            {
                foreach (Connector cb in ca.AllRefs)
                {
                    Element element = cb.Owner;
                    if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        FamilyInstance fi = element as FamilyInstance;
                        Family f = fi.Symbol.Family;
                        Parameter partType = f.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE);
                        if (partType != null)
                        {
                            if (partType.AsInteger() == 7) check++;
                        }
                    }
                }
            }
            if (check == 2) result = true;
            return result;
        }
    }
}
