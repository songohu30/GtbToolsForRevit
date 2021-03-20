using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public enum DurchbruchMemoryAction
    {
        None,
        ShowElement,
        ShowPosition,
        DeletePosition,
        DeleteRemainingMarkers,
        SetNewOffset,
        SetNewDiameter,
        ShowElements
    }
}
