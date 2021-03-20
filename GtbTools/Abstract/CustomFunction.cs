using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Abstract
{
    public interface CustomFunction
    {
        ExternalEvent TheEvent { get; set; }
        ManualResetEvent SignalEvent { get; set; }

        void SetEvent(ExternalEvent theEvent);
        void DisplayWindow();
        void Initialize(UIDocument uIDocument);
    }
}
