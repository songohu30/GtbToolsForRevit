using Autodesk.Revit.UI;
using ExternalLinkControl;
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
    public class ExternalLinkTool : Abstract.CustomFunction
    {
        public ExternalLinkToolViewModel ExternalLinkToolViewModel { get; set; }
        public ExternalEvent TheEvent { get; set; }
        public ManualResetEvent SignalEvent { get; set; }
        public ExternalLinkToolAction Action { get; set; }

        public void DisplayWindow()
        {
            Thread windowThread = new Thread(delegate ()
            {
                ExternalLinksWindow window = new ExternalLinksWindow(this);
                window.ShowDialog();
                Dispatcher.Run();
            });
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
        }

        public void Initialize(UIDocument uIDocument)
        {
            SignalEvent = new ManualResetEvent(false);
            ExternalLinkToolViewModel = new ExternalLinkToolViewModel(uIDocument.Document);
            ExternalLinkToolViewModel.CreateViewModels();

        }

        public void SetEvent(ExternalEvent theEvent)
        {
            TheEvent = theEvent;
        }
    }
}
