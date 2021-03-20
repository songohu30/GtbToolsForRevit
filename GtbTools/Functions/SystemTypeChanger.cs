using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Functions
{
    public class SystemTypeChanger : Abstract.CustomFunction
    {
        public ExternalEvent TheEvent { get; set; }
        public ManualResetEvent SignalEvent { get; set; }
        public List<PipingSystemType> SystemTypes { get; set; }
        public PipingSystemType SelectedSystemType { get; set; }
        public Action Action { get; set; }

        UIDocument _uidoc;
        Document _doc;

        public SystemTypeChanger()
        {
            SignalEvent = new ManualResetEvent(false);
        }

        public void Initialize(UIDocument uIDocument)
        {
            _uidoc = uIDocument;
            _doc = uIDocument.Document;
            GetPipingSystemTypes();
        }

        public void ChangeType()
        {
            Connector pipeConnector = null;
            Connector elementConnector = null; //allowed elements - PIF, PIA, PF, ME

            int connected = 0;
            Element e;
            if(CheckSelection(out e))
            {
                Pipe pipe = e as Pipe;
                foreach (Connector pc in pipe.ConnectorManager.Connectors)
                {
                    if(pc.IsConnected)
                    {
                        connected++;
                        pipeConnector = pc;
                    }
                    if(connected == 1)
                    {
                        foreach (Connector ec in pipeConnector.AllRefs)
                        {
                            if(ec.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves 
                                || ec.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting
                                    || ec.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeAccessory
                                        || ec.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment)
                            {
                                elementConnector = ec;
                            }
                        }
                    }
                }

                if(pipeConnector != null && elementConnector != null)
                {
                    using(Transaction tx = new Transaction(_doc, "Disconnect"))
                    {
                        int limit = 0;
                        do
                        {
                            tx.Start();
                            pipeConnector.DisconnectFrom(elementConnector);
                            limit++;
                            tx.Commit();
                        }
                        while (pipeConnector.IsConnectedTo(elementConnector) && limit < 10);
                                            }
                    using (Transaction tx = new Transaction(_doc, "DivideChangeConnect"))
                    {
                        tx.Start();
                        if(!pipeConnector.IsConnected)
                        {
                            pipe.MEPSystem.DivideSystem(_doc);
                            pipe.SetSystemType(SelectedSystemType.Id);
                            pipeConnector.ConnectTo(elementConnector);
                        }
                        tx.Commit();
                    }
                }
            }
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                SystemTypeWindow window = new SystemTypeWindow(this);
                window.ShowDialog();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void SetEvent(ExternalEvent theEvent)
        {
            TheEvent = theEvent;
        }

        private void GetPipingSystemTypes()
        {
            FilteredElementCollector ficol = new FilteredElementCollector(_doc);
            SystemTypes = ficol.OfClass(typeof(PipingSystemType)).Select(e => e as PipingSystemType).ToList();
            SystemTypes = SystemTypes.OrderBy(e => e.Name).ToList();
        }

        private bool CheckSelection(out Element e)
        {
            e = null;
            bool result = false;
            var selection = _uidoc.Selection.GetElementIds();
            if(selection != null && selection.Count == 1)
            {
                ElementId eId = selection.First();
                e = _doc.GetElement(eId);
                if(e != null && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                {
                    result = true;
                }
            }
            return result;
        }
    }

    public enum Action
    {
        None,
        Initialize,
        Apply
    }

}
