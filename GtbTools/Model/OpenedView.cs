using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class OpenedView
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public ViewType ViewType { get; set; }
        public IList<XYZ> ZoomCorners { get; set; }
    }
}
