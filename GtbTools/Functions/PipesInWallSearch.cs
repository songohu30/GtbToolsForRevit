using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GUI;
using PipesInWall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Functions
{
    public class PipesInWallSearch
    {
        public ExternalEvent TheEvent { get; set; }
        public PipesInWallViewModel PipesInWallViewModel {get; set;}
        public PipesInWallAction Action { get; set; }
        public ManualResetEvent SignalEvent = new ManualResetEvent(false);

        UIDocument _uidoc;
        Document _doc;

        public PipesInWallSearch()
        {

        }

        public void Initialize(UIDocument uIDocument)
        {
            _uidoc = uIDocument;
            _doc = uIDocument.Document;
            PipesInWallViewModel = PipesInWallViewModel.Initialize(_doc);
        }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                PipesInWallWindow window = new PipesInWallWindow(this);
                window.ShowDialog();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void SelectElement()
        {
            ElementId id = PipesInWallViewModel.SelectedElement;
            _uidoc.Selection.SetElementIds(new List<ElementId>() { id });
        }

        public void SetEvent(ExternalEvent externalEvent)
        {
            TheEvent = externalEvent;
        }
    }
}
