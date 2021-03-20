using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipeFlowTool
{
    public class ViewSystemElements
    {
        public View View { get; set; }
        public string SystemType { get; set; }
        public List<Element> SystemElements { get; set; }
    }
}
