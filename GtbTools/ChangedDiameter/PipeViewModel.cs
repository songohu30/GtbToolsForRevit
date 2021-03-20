using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangedDiameter
{
    public class PipeViewModel
    {
        public ElementId ElementId { get; set; }
        public double CurrentDiameter { get; set; }
        public double OldDiameter { get; set; }
    }
}
